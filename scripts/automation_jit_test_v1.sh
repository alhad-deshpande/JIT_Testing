#!/bin/bash
set -euo pipefail
set -x

# =========================================================
# Combined JIT Testing Script
# This script:
# 1. Clones runtime repo
# 2. Installs dotnet
# 3. (Phase 1 — optional) Iterates through plain function
#    names from Functions_list.txt and adds each one
#    CUMULATIVELY to coreclrInitializationInterpreterFallbackFunctions[].
#    Skipped automatically when the file is missing or empty.
# 4. (Phase 2 — optional) Iterates through ClassName:FunctionName
#    pairs from Interpreting-functions.txt and adds each pair
#    CUMULATIVELY to jitInclusionList[] as:
#      { "ClassName","FunctionName"},
#    Skipped automatically when the file is missing or empty.
# 5. For every entry in both phases:
#    - Builds runtime
#    - Tests with hello_world via corerun
#    - If build/hello_world/JIT tests fail: removes entry from cpp
#      AND from the source list file
#    - If all three pass: keeps entry in cpp and source list file
# =========================================================

export DEBIAN_FRONTEND=noninteractive

# =========================================================
# Configuration
# =========================================================
WORKSPACE_DIR="$(pwd)"
RUNTIME_DIR="$WORKSPACE_DIR/runtime"
TEST_DIR="$WORKSPACE_DIR/JIT_Testing"
FUNCTIONS_FILE="$WORKSPACE_DIR/Functions_list.txt"
INTERPRETING_FUNCTIONS_FILE="$WORKSPACE_DIR/Interpreting-functions.txt"
JITINTERFACE_CPP="$RUNTIME_DIR/src/coreclr/vm/jitinterface.cpp"

# Log directories
LOG_DIR="$WORKSPACE_DIR/logs"
FAILED_FUNCTIONS_FILE="$LOG_DIR/failed_functions.txt"
SUCCESS_LOG="$LOG_DIR/cpp_handled_functions.log"
PROCESSED_FUNCTIONS_FILE="$LOG_DIR/processed_functions.txt"
FAILED_INCLUSIONS_FILE="$LOG_DIR/failed_inclusions.txt"
SUCCESS_INCLUSIONS_LOG="$LOG_DIR/successful_inclusions.log"

# Build configuration
MAX_BUILD_RETRIES=2
STEP_MAX_RETRIES=3        # retries per individual build step (runtime / libs / tests)
STEP_RETRY_SLEEP=60       # seconds to wait before retrying a stuck/failed step

# Hello World test paths
HELLO_WORLD_DIR="$WORKSPACE_DIR/ppc64_HelloWorld/ppc64_HelloWorld"
HELLO_WORLD_DLL="$HELLO_WORLD_DIR/bin/Debug/net9.0/ppc64_HelloWorld.dll"
CORERUN_PATH="$RUNTIME_DIR/artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root/corerun"

mkdir -p "$LOG_DIR"

# =========================================================
# Per-Step Retry Helper
# Retries a single build command up to STEP_MAX_RETRIES times.
# Usage: run_build_step <label> <log_file> <cmd> [args...]
# =========================================================
run_build_step() {
    local label="$1"
    local log_file="$2"
    shift 2
    local attempt=1

    while [ $attempt -le $STEP_MAX_RETRIES ]; do
        log_info "[$label] attempt $attempt/$STEP_MAX_RETRIES..."
        if "$@" 2>&1 | tee "$log_file"; then
            log_success "[$label] succeeded on attempt $attempt"
            log_info "[$label] Sleeping 5s..."
            sleep 5
            return 0
        fi
        log_error "[$label] failed on attempt $attempt"
        if [ $attempt -lt $STEP_MAX_RETRIES ]; then
            log_info "[$label] Waiting ${STEP_RETRY_SLEEP}s before retry..."
            sleep "$STEP_RETRY_SLEEP"
        fi
        attempt=$((attempt + 1))
    done

    log_error "[$label] failed after $STEP_MAX_RETRIES attempts"
    return 1
}

# =========================================================
# Utility Functions
# =========================================================

log_info() {
    echo "[INFO] $(date '+%Y-%m-%d %H:%M:%S') - $*"
}

log_error() {
    echo "[ERROR] $(date '+%Y-%m-%d %H:%M:%S') - $*" >&2
}

log_success() {
    echo "[SUCCESS] $(date '+%Y-%m-%d %H:%M:%S') - $*"
}

# Returns the number of usable (non-empty, non-comment) lines in a file.
# Returns 0 when the file is missing.
count_usable_lines() {
    local file="$1"
    [ -f "$file" ] || { echo 0; return; }
    grep -cvE '^\s*$|^\s*#' "$file" 2>/dev/null || echo 0
}

# =========================================================
# Setup Function
# =========================================================
setup() {
    log_info "Installing dependencies..."
    
    apt-get update
    apt-get install -y \
        bc automake clang curl findutils git hostname libtool \
        libkrb5-dev ninja-build llvm make python3 cmake \
        liblttng-ust-dev tar wget jq lld \
        build-essential zlib1g-dev libssl-dev libbrotli-dev \
        ca-certificates coreutils

    log_info "Cloning runtime repository..."
    if [ -d "$RUNTIME_DIR" ]; then
        log_info "Runtime directory already exists, removing..."
        rm -rf "$RUNTIME_DIR"
    fi

    git clone --recurse-submodules https://github.com/alhad-deshpande/runtime.git "$RUNTIME_DIR"
    cd "$RUNTIME_DIR"
    git checkout ppc64le_coreclr_jit

    log_info "Cloning JIT_Testing repository..."
    if [ -d "$TEST_DIR" ]; then
        log_info "JIT_Testing directory already exists, removing..."
        rm -rf "$TEST_DIR"
    fi

    git clone https://github.com/alhad-deshpande/JIT_Testing.git "$TEST_DIR"
    log_success "JIT_Testing cloned to $TEST_DIR"

    # =========================================================
    # Auto-clean disabled NuGet feeds
    # =========================================================
    log_info "Cleaning NuGet feeds..."
    
    for CONFIG in NuGet.config eng/NuGet.config; do
        if [ -f "$CONFIG" ]; then
            log_info "Processing $CONFIG"
            cp "$CONFIG" "$CONFIG.bkp"
            
            grep 'value="https://pkgs.dev.azure.com' "$CONFIG" | while read -r line; do
                FEED_URL=$(echo "$line" | sed -n 's/.*value="\([^"]*\)".*/\1/p')
                log_info "Checking: $FEED_URL"
                
                HTTP_CODE=$(curl -L -s -o /dev/null -w "%{http_code}" "$FEED_URL" || echo "000")
                
                if [ "$HTTP_CODE" = "404" ]; then
                    log_info "Removing disabled feed: $FEED_URL"
                    ESCAPED=$(printf '%s\n' "$FEED_URL" | sed 's/[\/&]/\\&/g')
                    sed -i "\|$ESCAPED|d" "$CONFIG"
                fi
            done
        fi
    done

    log_info "Setting up .NET SDK..."
    SDK_VERSION=$(jq -r '.sdk.version' "$RUNTIME_DIR/global.json")

    export DOTNET_DIR="/dotnet-sdk-$(uname -m)"
    mkdir -p "$DOTNET_DIR"

    pushd "$DOTNET_DIR"

    wget "https://github.com/IBM/dotnet-s390x/releases/download/v${SDK_VERSION}/dotnet-sdk-${SDK_VERSION}-linux-$(uname -m).tar.gz"

    mkdir -p .dotnet
    tar xvf dotnet-sdk-*linux-$(uname -m).tar.gz -C .dotnet > /dev/null

    export DOTNET_ROOT="$(realpath .dotnet)"
    export PATH="$DOTNET_ROOT:$PATH"
    
    popd
    
    log_success "DOTNET installed: $(dotnet --version)"
    
    # Save environment variables
    echo "export DOTNET_ROOT=$DOTNET_ROOT" > /env.sh
    echo "export PATH=$DOTNET_ROOT:\$PATH" >> /env.sh
    
    cd "$WORKSPACE_DIR"
}

# =========================================================
# Add Function to coreclrInitializationInterpreterFallbackFunctions[]
# (plain function-name only, CUMULATIVE)
# =========================================================
add_function_to_cpp() {
    local function_name="$1"
    local cpp_file="$JITINTERFACE_CPP"

    log_info "Adding function '$function_name' to $cpp_file (cumulative)"

    # Check if function already exists (with or without trailing comma)
    if grep -qE "\"${function_name}\",?" "$cpp_file"; then
        log_info "Function '$function_name' already exists in cpp file, skipping"
        return 0
    fi

    # Use awk to:
    #   1. Find the coreclrInitializationInterpreterFallbackFunctions array closing brace
    #   2. Ensure the last string entry before it has a trailing comma
    #   3. Insert the new entry on its own properly-indented line before the closing brace
    local tmp_file="${cpp_file}.tmp"
    awk -v fn="$function_name" '
        BEGIN { in_array = 0; found_array = 0; done = 0 }

        # Detect the array declaration
        /coreclrInitializationInterpreterFallbackFunctions\[\]/ {
            found_array = 1
            in_array = 1
        }

        # When inside the array, detect the closing brace line
        in_array && /^    \};/ && !done {
            # Print the new entry before the closing brace
            print "        \"" fn "\","
            done = 1
        }

        {
            # Track the last string-entry line inside the array so we can fix
            # a missing trailing comma before we insert our new entry.
            if (in_array && /^[[:space:]]*"[^"]*"[[:space:]]*,?[[:space:]]*$/) {
                prev_string_line = $0
                prev_string_no_comma = ($0 !~ /,$/)
            }
            print
        }
    ' "$cpp_file" > "$tmp_file"

    # awk cannot rewrite already-printed lines, so handle the trailing-comma
    # fix in a second focused pass: find the last bare string entry inside the
    # array (no trailing comma) and add one.
    awk '
        BEGIN { in_array = 0 }
        /coreclrInitializationInterpreterFallbackFunctions\[\]/ { in_array = 1 }
        in_array && /^    \};/ { in_array = 0 }
        { lines[NR] = $0 }
        END {
            # Find the last line inside the array that is a string entry without comma
            last_fix = 0
            in_arr = 0
            for (i = 1; i <= NR; i++) {
                if (lines[i] ~ /coreclrInitializationInterpreterFallbackFunctions\[\]/) in_arr = 1
                if (in_arr && lines[i] ~ /^    \};/) in_arr = 0
                if (in_arr && lines[i] ~ /^[[:space:]]*"[^"]*"[[:space:]]*$/) last_fix = i
            }
            for (i = 1; i <= NR; i++) {
                if (i == last_fix) {
                    # Append comma to this line (strip trailing whitespace first)
                    sub(/[[:space:]]*$/, "", lines[i])
                    print lines[i] ","
                } else {
                    print lines[i]
                }
            }
        }
    ' "$tmp_file" > "${tmp_file}2"

    mv "${tmp_file}2" "$cpp_file"
    rm -f "$tmp_file"

    local fn_count
    fn_count=$(grep -c '        "' "$cpp_file" 2>/dev/null) || fn_count=0
    log_success "Function '$function_name' added to cpp file (now has ${fn_count} functions)"
    return 0
}

# =========================================================
# Remove Function from coreclrInitializationInterpreterFallbackFunctions[]
# =========================================================
remove_function_from_cpp() {
    local function_name="$1"
    local cpp_file="$JITINTERFACE_CPP"
    
    log_info "Removing function '$function_name' from $cpp_file"

    # Remove the line containing the quoted function entry (e.g.        "Grow",)
    sed -i "/[[:space:]]*\"${function_name}\"[[:space:]]*,\?[[:space:]]*$/d" "$cpp_file" || true
    
    log_success "Function '$function_name' removed from cpp file"
}

# =========================================================
# Remove Function from Functions_list.txt
# =========================================================
remove_function_from_list() {
    local function_name="$1"
    
    log_info "Removing function '$function_name' from Functions_list.txt"
    
    # Create a temporary file without the failed function
    grep -v "\"${function_name}\"" "$FUNCTIONS_FILE" > "${FUNCTIONS_FILE}.tmp" || true
    mv "${FUNCTIONS_FILE}.tmp" "$FUNCTIONS_FILE"
    
    log_success "Function '$function_name' removed from Functions_list.txt"
}

# =========================================================
# Add { "ClassName","FunctionName"}, to jitInclusionList[]
#
# The struct has two separate fields:
#   struct JitInclusionEntry { const char* className;
#                              const char* functionName; };
# and the lookup does two separate strcmp calls, so class name
# and function name MUST be two separate string literals.
#
# Format written:   \t{ "ClassName","FunctionName"},
# which matches every existing entry, e.g.:
#   { "System.Collections.Generic.List`1[__Canon]",".cctor"},
# =========================================================
add_jit_inclusion_entry() {
    local class_name="$1"
    local func_name="$2"
    local cpp_file="$JITINTERFACE_CPP"
    local label="${class_name}:${func_name}"

    log_info "Adding jitInclusionList entry '${label}'"

    # Skip if an identical pair already exists on the same line
    if grep -qE "\"${class_name}\"[[:space:]]*,[[:space:]]*\"${func_name}\"" "$cpp_file"; then
        log_info "Entry '${label}' already exists in jitInclusionList, skipping"
        return 0
    fi

    # Insert before the closing }; of jitInclusionList[]
    local tmp_file="${cpp_file}.jit_tmp"
    awk -v cls="$class_name" -v fn="$func_name" '
        BEGIN { in_list = 0; done = 0 }
        /static const JitInclusionEntry jitInclusionList\[\]/ { in_list = 1 }
        in_list && /^[[:space:]]*\};/ && !done {
            printf "\t{ \"%s\",\"%s\"},\n", cls, fn
            done = 1
        }
        { print }
    ' "$cpp_file" > "$tmp_file"

    mv "$tmp_file" "$cpp_file"

    log_success "Entry '${label}' added to jitInclusionList"
    return 0
}

# =========================================================
# Remove { "ClassName","FunctionName"}, from jitInclusionList[]
#
# Uses awk with fixed-string matching (findex) so that special
# characters in class names (backticks, brackets, dots, etc.)
# are never misinterpreted as regex metacharacters — the same
# problem that caused sed-based removal to silently no-op.
# =========================================================
remove_jit_inclusion_entry() {
    local class_name="$1"
    local func_name="$2"
    local cpp_file="$JITINTERFACE_CPP"
    local label="${class_name}:${func_name}"

    log_info "Removing jitInclusionList entry '${label}'"

    local tmp_file="${cpp_file}.rm_tmp"

    # Delete any line that contains both "ClassName" and "FunctionName"
    # as adjacent quoted literals (the exact format written by add_jit_inclusion_entry).
    # findex() is a plain substring search — no regex escaping needed.
    awk -v cls="\"${class_name}\"" -v fn="\"${func_name}\"" '
        !(index($0, cls) && index($0, fn))
    ' "$cpp_file" > "$tmp_file"

    mv "$tmp_file" "$cpp_file"

    log_success "Entry '${label}' removed from jitInclusionList"
}

# =========================================================
# Remove a ClassName:FunctionName line from
# Interpreting-functions.txt
# =========================================================
remove_entry_from_interpreting_list() {
    local class_name="$1"
    local func_name="$2"
    local raw_line="${class_name}:${func_name}"

    log_info "Removing '${raw_line}' from Interpreting-functions.txt"

    local esc_line
    esc_line=$(printf '%s\n' "$raw_line" | sed 's/[]\[`.*^$]/\\&/g')

    grep -v "${esc_line}" "$INTERPRETING_FUNCTIONS_FILE" \
        > "${INTERPRETING_FUNCTIONS_FILE}.tmp" || true
    mv "${INTERPRETING_FUNCTIONS_FILE}.tmp" "$INTERPRETING_FUNCTIONS_FILE"

    log_success "'${raw_line}' removed from Interpreting-functions.txt"
}

# =========================================================
# Combined Build Function (Runtime + Libs + Tests)
# All 3 steps run with a combined timeout of 1hr 40min
# =========================================================
build_all_with_timeout() {
    local function_name="$1"
    local retry_count=0
    local build_success=false
    
    cd "$RUNTIME_DIR"
    export PATH="$DOTNET_ROOT:$PATH"
    export DOTNET_MULTILEVEL_LOOKUP=0
    export UseInstalledDotNetCli=true
    # NuGet/MSBuild HTTP timeout — prevents indefinite hang on package restore
    export NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS=30
    export NUGET_PLUGIN_REQUEST_TIMEOUT_IN_SECONDS=30
    export NUGET_HTTP_TIMEOUT_SECONDS=60
    export MSBUILDTERMINALLOGGER=off
    
    while [ $retry_count -lt $MAX_BUILD_RETRIES ]; do
        log_info "Building runtime for '$function_name' (attempt $((retry_count + 1))/$MAX_BUILD_RETRIES)..."

        # Create a script to run only the runtime build step (libs+tests run once via build_initial)
        cat > /tmp/build_all.sh << 'EOF'
#!/bin/bash
set -uo pipefail

# First argument is RUNTIME_DIR — cd into it so all relative paths are correct
cd "$1"

# NuGet/MSBuild HTTP timeout — prevents indefinite hang on package restore
export NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS=30
export NUGET_PLUGIN_REQUEST_TIMEOUT_IN_SECONDS=30
export NUGET_HTTP_TIMEOUT_SECONDS=60
export MSBUILDTERMINALLOGGER=off

STEP_MAX_RETRIES=3
STEP_RETRY_SLEEP=60

run_step() {
    local label="$1"; shift
    local attempt=1
    while [ $attempt -le $STEP_MAX_RETRIES ]; do
        echo "[BUILD] [$label] attempt $attempt/$STEP_MAX_RETRIES..."
        if "$@"; then
            echo "[BUILD] [$label] succeeded on attempt $attempt. Sleeping 5s..."
            sleep 5
            return 0
        fi
        echo "[BUILD] [$label] failed on attempt $attempt"
        if [ $attempt -lt $STEP_MAX_RETRIES ]; then
            echo "[BUILD] [$label] Waiting ${STEP_RETRY_SLEEP}s before retry..."
            sleep "$STEP_RETRY_SLEEP"
        fi
        attempt=$((attempt + 1))
    done
    echo "[BUILD] [$label] failed after $STEP_MAX_RETRIES attempts"
    return 1
}

# Step 1: Build runtime only (libs and tests were built once by build_initial)
echo "[BUILD] Step 1/1: Building runtime (clr+clr.hosts)..."
run_step "runtime" ./build.sh clr+clr.hosts \
    /p:SkipManagedTools=true \
    /p:PrimaryRuntimeFlavor=CoreCLR \
    /p:PublishAot=false \
    /p:SupportsNativeAotComponents=false

# Copy updated runtime binaries into Core_Root
CORE_ROOT="./artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root"
cp ./artifacts/bin/coreclr/linux.ppc64le.Debug/libcoreclr.so     "${CORE_ROOT}/libcoreclr.so"
cp ./artifacts/bin/coreclr/linux.ppc64le.Debug/libcoreclr.so.dbg "${CORE_ROOT}/libcoreclr.so.dbg"
cp ./artifacts/bin/coreclr/linux.ppc64le.Debug/libclrjit.so      "${CORE_ROOT}/libclrjit.so"
cp ./artifacts/bin/coreclr/linux.ppc64le.Debug/libclrjit.so.dbg  "${CORE_ROOT}/libclrjit.so.dbg"

echo "[BUILD] Runtime build and copy steps completed successfully"
EOF
        
        chmod +x /tmp/build_all.sh
        
        # Run runtime build step (pass RUNTIME_DIR so the script can cd into it)
        if bash /tmp/build_all.sh "$RUNTIME_DIR" 2>&1 | tee "$LOG_DIR/build_all_${function_name}.log"; then
            build_success=true
            log_success "Runtime build succeeded for '$function_name'"
            break
        else
            log_error "Runtime build failed for '$function_name' (attempt $((retry_count + 1)))"
            retry_count=$((retry_count + 1))
            
            if [ $retry_count -lt $MAX_BUILD_RETRIES ]; then
                log_info "Retrying all build steps for same function..."
                sleep 5
            fi
        fi
    done
    
    rm -f /tmp/build_all.sh
    cd "$WORKSPACE_DIR"
    
    if [ "$build_success" = false ]; then
        log_error "Runtime build failed after $MAX_BUILD_RETRIES attempts for '$function_name'"
        return 1
    fi
    
    return 0
}

# =========================================================
# Initial Build (for setup only)
# =========================================================
build_initial() {
    log_info "Performing initial build (clr + libs + tests)..."
    cd "$RUNTIME_DIR"
    # NuGet/MSBuild HTTP timeout — prevents indefinite hang on package restore
    export NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS=30
    export NUGET_PLUGIN_REQUEST_TIMEOUT_IN_SECONDS=30
    export NUGET_HTTP_TIMEOUT_SECONDS=60
    export MSBUILDTERMINALLOGGER=off

    if ! run_build_step "initial-runtime" "$LOG_DIR/build_clr_initial.log" \
        ./build.sh clr+clr.hosts \
            /p:SkipManagedTools=true \
            /p:PrimaryRuntimeFlavor=CoreCLR \
            /p:PublishAot=false \
            /p:SupportsNativeAotComponents=false; then
        log_error "Initial runtime build failed after all retries"
        cd "$WORKSPACE_DIR"
        return 1
    fi

    if ! run_build_step "initial-libs" "$LOG_DIR/build_libs_initial.log" \
        ./build.sh libs; then
        log_error "Initial libraries build failed after all retries"
        cd "$WORKSPACE_DIR"
        return 1
    fi

    if ! run_build_step "initial-tests" "$LOG_DIR/build_tests_initial.log" \
        ./src/tests/build.sh /p:LibrariesConfiguration=Debug; then
        log_error "Initial tests build failed after all retries"
        cd "$WORKSPACE_DIR"
        return 1
    fi

    CORE_ROOT="./artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root"
    cp "${CORE_ROOT}/IL/System.Private.CoreLib.dll" "${CORE_ROOT}/System.Private.CoreLib.dll"

    log_success "Initial build completed"
    cd "$WORKSPACE_DIR"
    return 0
}

# =========================================================
# Test Hello World with corerun
# =========================================================
test_hello_world() {
    local function_name="$1"
    local core_root="$RUNTIME_DIR/artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root"
    local publish_dir="$HELLO_WORLD_DIR/bin/Debug/net9.0"

    log_info "Testing hello_world with corerun for: $function_name"

    # Build hello world project using the custom dotnet SDK
    cd "$HELLO_WORLD_DIR"
    if ! "$DOTNET_ROOT/dotnet" build 2>&1 | tee "$LOG_DIR/helloworld_build_${function_name}.log"; then
        log_error "Hello world build failed for '$function_name'"
        cd "$WORKSPACE_DIR"
        return 1
    fi

    # Do NOT copy .so files into the publish dir — doing so creates a shadow
    # library directory that, combined with a trailing colon in LD_LIBRARY_PATH,
    # causes the current directory (".") to be searched first, loading stale
    # binaries instead of the freshly built Core_Root versions.

    # Run ppc64_HelloWorld.dll via corerun.
    # CORE_ROOT tells corerun where to find the managed/native runtime libraries.
    # Set LD_LIBRARY_PATH cleanly — do NOT append the existing value, which may
    # already contain core_root and would produce a double-entry with a trailing
    # colon (implicitly adding "." to the search path).
    export CORE_ROOT="$core_root"
    export LD_LIBRARY_PATH="$core_root"

    local output_file="$LOG_DIR/helloworld_${function_name}_output.log"

    # Redirect all output to log file, then print to stdout
    local corerun_exit=0
    "$CORERUN_PATH" "$HELLO_WORLD_DLL" \
        > "$output_file" 2>&1 || corerun_exit=$?
    # Echo captured output to console so it appears in the outer log stream
    cat "$output_file"

    if [ "$corerun_exit" -eq 0 ]; then
        log_success "Hello world test PASSED for: $function_name"
        cd "$WORKSPACE_DIR"
        return 0
    else
        log_error "Hello world test FAILED (exit $corerun_exit) for: $function_name"

        # Save failure log
        local fail_log="$LOG_DIR/helloworld_${function_name}_fail.log"
        echo "Entry: $function_name" > "$fail_log"
        echo "Timestamp: $(date)" >> "$fail_log"
        echo "Exit code: $corerun_exit" >> "$fail_log"
        echo "---" >> "$fail_log"
        cat "$output_file" >> "$fail_log"

        cd "$WORKSPACE_DIR"
        return 1
    fi
}

# =========================================================
# Run JIT Test Suite
# =========================================================
run_jit_tests() {
    local function_name="$1"

    log_info "Running JIT test suite for: $function_name"

    cd "$TEST_DIR"
    chmod +x run_test.sh

    local test_log="$LOG_DIR/jit_tests_${function_name}.log"

    # Run the test suite, streaming output live to the terminal via tee while
    # also writing to the log file.  tee always exits 0, so we capture
    # run_test.sh's real exit code via PIPESTATUS[0].
    ./run_test.sh "$DOTNET_ROOT" "$RUNTIME_DIR" 2>&1 | tee "$test_log"
    local test_exit=${PIPESTATUS[0]}

    # Parse the summary line to extract pass/fail counts:
    #   "Testcases Passed    : <N>"
    #   "Testcases Failed    : <N>"
    local passed failed total
    passed=$(grep -m1 "^Testcases Passed" "$test_log" | sed 's/.*:[[:space:]]*//')
    failed=$(grep -m1 "^Testcases Failed" "$test_log" | sed 's/.*:[[:space:]]*//')
    total=$(grep -m1 "^Total Testcases Run" "$test_log" | sed 's/.*:[[:space:]]*//')

    # Default to sentinel values if the summary was not found
    passed=${passed:-0}
    failed=${failed:-1}
    total=${total:-0}

    log_info "JIT test summary for '$function_name': total=$total passed=$passed failed=$failed"

    # A function is kept ONLY when run_test.sh exits 0 AND all 93 tests passed
    # AND no test reported a failure.
    if [ "$test_exit" -eq 0 ] && [ "$failed" -eq 0 ] && [ "$total" -eq 93 ] && [ "$passed" -eq 93 ]; then
        sleep 60
        log_success "JIT tests PASSED for: $function_name (93/93)"
        cd "$WORKSPACE_DIR"
        return 0
    fi

    # At least one test failed — log details and return failure
    log_error "JIT tests FAILED for: $function_name (exit=$test_exit total=$total passed=$passed failed=$failed)"

    local fail_log="$LOG_DIR/testcase_${function_name}_fail.log"
    {
        echo "Entry: $function_name"
        echo "Timestamp: $(date)"
        echo "Exit code: $test_exit"
        echo "Total: $total  Passed: $passed  Failed: $failed"
        echo "---"
        cat "$test_log"
    } > "$fail_log"

    cd "$WORKSPACE_DIR"
    return 1
}

# =========================================================
# Process Single Fallback Function
# (coreclrInitializationInterpreterFallbackFunctions[])
# =========================================================
process_function() {
    local function_name="$1"
    
    log_info "=========================================="
    log_info "Processing fallback function: $function_name"
    log_info "Current fallback list size: $(grep -c '        "' "$JITINTERFACE_CPP" 2>/dev/null || echo 0)"
    log_info "=========================================="
    
    # Add function to cpp file (CUMULATIVE)
    if ! add_function_to_cpp "$function_name"; then
        log_error "Failed to add function to cpp file"
        echo "$function_name - Failed to add to cpp" >> "$FAILED_FUNCTIONS_FILE"
        remove_function_from_list "$function_name"
        return 1
    fi
    
    # Build all (runtime + libs + tests) with combined timeout and retry
    if ! build_all_with_timeout "$function_name"; then
        log_error "Build failed for: $function_name - REMOVING from cpp and Functions_list.txt"
        remove_function_from_cpp "$function_name"
        remove_function_from_list "$function_name"
        echo "$function_name - Build failed" >> "$FAILED_FUNCTIONS_FILE"
        return 1
    fi
    
    # Test with hello_world
    if ! test_hello_world "$function_name"; then
        log_error "Hello world test failed for: $function_name - REMOVING from cpp and Functions_list.txt"
        remove_function_from_cpp "$function_name"
        remove_function_from_list "$function_name"
        echo "$function_name - Hello world failed" >> "$FAILED_FUNCTIONS_FILE"
        return 1
    fi
    
    # If we reach here, both build and hello_world passed
    log_success "Function '$function_name' passed build and hello_world"
    echo "$function_name" >> "$PROCESSED_FUNCTIONS_FILE"

    # Run JIT test suite — failure removes the function
    if ! run_jit_tests "$function_name"; then
        log_error "JIT tests failed for: $function_name - REMOVING from cpp and Functions_list.txt"
        remove_function_from_cpp "$function_name"
        remove_function_from_list "$function_name"
        echo "$function_name - JIT tests failed" >> "$FAILED_FUNCTIONS_FILE"
        return 1
    fi

    log_success "Function '$function_name' fully processed successfully and KEPT in cpp file"
    echo "$function_name - $(date)" >> "$SUCCESS_LOG"
    log_info "Total fallback functions in cpp: $(grep -c '        "' "$JITINTERFACE_CPP" 2>/dev/null || echo 0)"
    return 0
}

# =========================================================
# Process Single jitInclusionList Entry
#
# Input line format (from Interpreting-functions.txt):
#   ClassName:FunctionName
#
# Split on the FIRST colon only, so generic class names such as
#   System.Collections.Generic.Dictionary`2[Char,__Canon]
# are kept intact.  The entry is written as:
#   \t{ "ClassName","FunctionName"},
# which matches every existing entry, e.g.:
#   { "System.Collections.Generic.List`1[__Canon]",".cctor"},
# =========================================================
process_jit_inclusion() {
    local raw_line="$1"

    # Split on first ':' only
    local class_name func_name
    class_name="$(echo "${raw_line%%:*}" | xargs)"
    func_name="$(echo "${raw_line#*:}"   | xargs)"

    if [[ -z "$class_name" || -z "$func_name" ]]; then
        log_error "Skipping malformed line (cannot split ClassName:FunctionName): '$raw_line'"
        return 1
    fi

    local label="${class_name}:${func_name}"
    # Sanitise label for use in log filenames (strip chars unsafe for filenames)
    local safe_label
    safe_label="$(echo "$label" | tr -cs 'A-Za-z0-9_.' '_')"

    log_info "=========================================="
    log_info "Processing jitInclusionList entry: $label"
    log_info "  className    = $class_name"
    log_info "  functionName = $func_name"
    log_info "=========================================="

    if ! add_jit_inclusion_entry "$class_name" "$func_name"; then
        log_error "Failed to add jitInclusionList entry '$label'"
        echo "$label - Failed to add to cpp" >> "$FAILED_INCLUSIONS_FILE"
        remove_entry_from_interpreting_list "$class_name" "$func_name"
        return 1
    fi

    if ! build_all_with_timeout "$safe_label"; then
        log_error "Build failed for '$label' — removing entry from cpp and list"
        remove_jit_inclusion_entry "$class_name" "$func_name"
        remove_entry_from_interpreting_list "$class_name" "$func_name"
        echo "$label - Build failed" >> "$FAILED_INCLUSIONS_FILE"
        return 1
    fi

    if ! test_hello_world "$safe_label"; then
        log_error "Hello world failed for '$label' — removing entry from cpp and list"
        remove_jit_inclusion_entry "$class_name" "$func_name"
        remove_entry_from_interpreting_list "$class_name" "$func_name"
        echo "$label - Hello world failed" >> "$FAILED_INCLUSIONS_FILE"
        return 1
    fi

    log_success "Entry '$label' passed build and hello_world"

    # JIT test failure removes the entry
    if ! run_jit_tests "$safe_label"; then
        log_error "JIT tests failed for '$label' — removing entry from cpp and list"
        remove_jit_inclusion_entry "$class_name" "$func_name"
        remove_entry_from_interpreting_list "$class_name" "$func_name"
        echo "$label - JIT tests failed" >> "$FAILED_INCLUSIONS_FILE"
        return 1
    fi

    log_success "Entry '$label' fully processed and KEPT in jitInclusionList"
    echo "$label - $(date)" >> "$SUCCESS_INCLUSIONS_LOG"
    return 0
}

# =========================================================
# Main Workflow
# =========================================================
main() {
    log_info "Starting combined JIT testing workflow"
    log_info "Phase 1 : plain function names -> coreclrInitializationInterpreterFallbackFunctions[]"
    log_info "Phase 2 : ClassName:FunctionName pairs -> jitInclusionList[]"
    log_info "Each phase is skipped automatically if its source file is missing or empty."

    # Setup environment
    setup

    # Initial builds
    log_info "Performing initial builds..."
    build_initial

    # ----------------------------------------------------------
    # Phase 1 – coreclrInitializationInterpreterFallbackFunctions[]
    #
    # Skipped automatically when Functions_list.txt is missing
    # or contains no usable (non-empty, non-comment) lines.
    # ----------------------------------------------------------
    local total_functions=0 successful_functions=0 failed_functions=0
    local fn_count
    fn_count=$(count_usable_lines "$FUNCTIONS_FILE")

    if [ "$fn_count" -eq 0 ]; then
        log_info "=========================================="
        log_info "Phase 1 SKIPPED — Functions_list.txt is missing or empty"
        log_info "=========================================="
    else
        log_info "=========================================="
        log_info "Phase 1: Processing $fn_count entries from Functions_list.txt"
        log_info "Functions will be added CUMULATIVELY to coreclrInitializationInterpreterFallbackFunctions[]"
        log_info "=========================================="

        cp "$FUNCTIONS_FILE" "${FUNCTIONS_FILE}.original"

        while IFS= read -r line; do
            # Skip empty lines and comments
            [[ -z "$line" || "$line" =~ ^[[:space:]]*# ]] && continue

            # Remove quotes, commas, and carriage returns (CRLF -> LF safety)
            local function_name
            function_name=$(echo "$line" | tr -d '\r' | sed 's/[",]//g' | xargs)

            [[ -z "$function_name" ]] && continue

            total_functions=$((total_functions + 1))

            if process_function "$function_name"; then
                successful_functions=$((successful_functions + 1))
            else
                failed_functions=$((failed_functions + 1))
            fi

        done < "${FUNCTIONS_FILE}.original"

        log_info "Phase 1 complete — total: $total_functions, ok: $successful_functions, failed: $failed_functions"
    fi

    # ----------------------------------------------------------
    # Phase 2 – jitInclusionList[]
    #
    # Skipped automatically when Interpreting-functions.txt is
    # missing or contains no usable (non-empty, non-comment) lines.
    # ----------------------------------------------------------
    local total_inclusions=0 successful_inclusions=0 failed_inclusions=0
    local inc_count
    inc_count=$(count_usable_lines "$INTERPRETING_FUNCTIONS_FILE")

    if [ "$inc_count" -eq 0 ]; then
        log_info "=========================================="
        log_info "Phase 2 SKIPPED — Interpreting-functions.txt is missing or empty"
        log_info "=========================================="
    else
        log_info "=========================================="
        log_info "Phase 2: Processing $inc_count entries from Interpreting-functions.txt"
        log_info "Entries will be added CUMULATIVELY to jitInclusionList[]"
        log_info "=========================================="

        cp "$INTERPRETING_FUNCTIONS_FILE" "${INTERPRETING_FUNCTIONS_FILE}.original"

        while IFS= read -r line; do
            # Skip empty lines and comments
            [[ -z "$line" || "$line" =~ ^[[:space:]]*# ]] && continue

            # Strip carriage returns and leading/trailing whitespace
            line=$(echo "$line" | tr -d '\r' | xargs)
            [[ -z "$line" ]] && continue

            # Must contain at least one colon to be a valid ClassName:FunctionName pair
            if [[ "$line" != *:* ]]; then
                log_error "Skipping line with no ':' separator: '$line'"
                continue
            fi

            total_inclusions=$((total_inclusions + 1))

            if process_jit_inclusion "$line"; then
                successful_inclusions=$((successful_inclusions + 1))
            else
                failed_inclusions=$((failed_inclusions + 1))
            fi

        done < "${INTERPRETING_FUNCTIONS_FILE}.original"

        log_info "Phase 2 complete — total: $total_inclusions, ok: $successful_inclusions, failed: $failed_inclusions"
    fi

    # ----------------------------------------------------------
    # Final Summary
    # ----------------------------------------------------------
    log_info "=========================================="
    log_info "FINAL SUMMARY"
    log_info "=========================================="
    log_info "--- Phase 1 (coreclrInitializationInterpreterFallbackFunctions[]) ---"
    log_info "  Status           : $([ "$fn_count" -eq 0 ] && echo 'SKIPPED (empty/missing)' || echo 'RAN')"
    log_info "  Total processed  : $total_functions"
    log_info "  Successful       : $successful_functions"
    log_info "  Failed           : $failed_functions"
    log_info "--- Phase 2 (jitInclusionList[]) ---"
    log_info "  Status           : $([ "$inc_count" -eq 0 ] && echo 'SKIPPED (empty/missing)' || echo 'RAN')"
    log_info "  Total processed  : $total_inclusions"
    log_info "  Successful       : $successful_inclusions"
    log_info "  Failed           : $failed_inclusions"
    log_info "--- Logs ---"
    log_info "  Phase 1 success  : $SUCCESS_LOG"
    log_info "  Phase 1 failed   : $FAILED_FUNCTIONS_FILE"
    log_info "  Phase 2 success  : $SUCCESS_INCLUSIONS_LOG"
    log_info "  Phase 2 failed   : $FAILED_INCLUSIONS_FILE"
    log_info "  All logs         : $LOG_DIR"
    log_info "=========================================="

    if [ $failed_functions -gt 0 ] || [ $failed_inclusions -gt 0 ]; then
        log_error "Some entries failed processing and were removed"
        exit 1
    fi

    log_success "All entries processed successfully!"
}

# =========================================================
# Execute Main
# =========================================================
main "$@"

# Made with IBM Bob
