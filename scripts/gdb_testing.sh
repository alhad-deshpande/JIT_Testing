#!/bin/bash
set -euo pipefail
set -x

# =========================================================
# four_step_Build.sh
#
# Run this script AFTER the 3-step initial build
# (build.sh clr+clr.hosts  →  build.sh libs  →
#  src/tests/build.sh) has already completed.
#
# What this script does:
# 1. Step 1/1 — Rebuilds runtime (clr+clr.hosts) only,
#    then copies the four freshly built .so files into
#    Core_Root.  Libs and tests are NOT rebuilt.
#
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
#    - Tests ppc64_HelloWorld.dll via gdb (non-interactive,
#      with a hard timeout to catch hangs)
#    - GDB logs saved per-entry under logs/gdb_logs/
#    - If build/gdb test fails or times out: removes entry
#      from cpp AND from the source list file
#    - If successful: keeps entry
# =========================================================

export DEBIAN_FRONTEND=noninteractive

# Source saved environment (DOTNET_ROOT / PATH) written by setup()
[ -f /env.sh ] && source /env.sh

# =========================================================
# Configuration
# =========================================================
WORKSPACE_DIR="$(pwd)"
RUNTIME_DIR="$WORKSPACE_DIR/runtime"
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
STEP_MAX_RETRIES=3        # retries per individual build step
STEP_RETRY_SLEEP=30       # seconds to wait before retrying a stuck/failed step

# Hello World / GDB test paths
HELLO_WORLD_DLL="$WORKSPACE_DIR/ppc64_HelloWorld/bin/Debug/net9.0/ppc64_HelloWorld.dll"
CORE_ROOT="$RUNTIME_DIR/artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root"
CORERUN_PATH="$CORE_ROOT/corerun"

# GDB log directory (one file per entry)
GDB_LOG_DIR="$LOG_DIR/gdb_logs"

# Seconds before gdb is killed as "stuck" (tune as needed)
GDB_TIMEOUT=120

mkdir -p "$LOG_DIR"
mkdir -p "$GDB_LOG_DIR"

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
# Add Function to coreclrInitializationInterpreterFallbackFunctions[]
# (plain function-name only, CUMULATIVE)
# =========================================================
add_function_to_cpp() {
    local function_name="$1"
    local cpp_file="$JITINTERFACE_CPP"

    log_info "Adding function '$function_name' to $cpp_file (cumulative)"

    # Skip if function already exists (with or without trailing comma)
    if grep -qE "\"${function_name}\",?" "$cpp_file"; then
        log_info "Function '$function_name' already exists in cpp file, skipping"
        return 0
    fi

    local tmp_file="${cpp_file}.tmp"
    awk -v fn="$function_name" '
        BEGIN { in_array = 0; found_array = 0; done = 0 }

        /coreclrInitializationInterpreterFallbackFunctions\[\]/ {
            found_array = 1
            in_array = 1
        }

        in_array && /^    \};/ && !done {
            print "        \"" fn "\","
            done = 1
        }

        {
            if (in_array && /^[[:space:]]*"[^"]*"[[:space:]]*,?[[:space:]]*$/) {
                prev_string_line = $0
                prev_string_no_comma = ($0 !~ /,$/)
            }
            print
        }
    ' "$cpp_file" > "$tmp_file"

    # Second pass: fix any missing trailing comma on the last existing entry
    awk '
        BEGIN { in_array = 0 }
        /coreclrInitializationInterpreterFallbackFunctions\[\]/ { in_array = 1 }
        in_array && /^    \};/ { in_array = 0 }
        { lines[NR] = $0 }
        END {
            last_fix = 0
            in_arr = 0
            for (i = 1; i <= NR; i++) {
                if (lines[i] ~ /coreclrInitializationInterpreterFallbackFunctions\[\]/) in_arr = 1
                if (in_arr && lines[i] ~ /^    \};/) in_arr = 0
                if (in_arr && lines[i] ~ /^[[:space:]]*"[^"]*"[[:space:]]*$/) last_fix = i
            }
            for (i = 1; i <= NR; i++) {
                if (i == last_fix) {
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
    sed -i "/[[:space:]]*\"${function_name}\"[[:space:]]*,\?[[:space:]]*$/d" "$cpp_file" || true
    log_success "Function '$function_name' removed from cpp file"
}

# =========================================================
# Remove Function from Functions_list.txt
# =========================================================
remove_function_from_list() {
    local function_name="$1"

    log_info "Removing function '$function_name' from Functions_list.txt"
    grep -v "\"${function_name}\"" "$FUNCTIONS_FILE" > "${FUNCTIONS_FILE}.tmp" || true
    mv "${FUNCTIONS_FILE}.tmp" "$FUNCTIONS_FILE"
    log_success "Function '$function_name' removed from Functions_list.txt"
}

# =========================================================
# Add { "ClassName","FunctionName"}, to jitInclusionList[]
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
# Uses awk findex() so special chars are not misread as regex.
# =========================================================
remove_jit_inclusion_entry() {
    local class_name="$1"
    local func_name="$2"
    local cpp_file="$JITINTERFACE_CPP"
    local label="${class_name}:${func_name}"

    log_info "Removing jitInclusionList entry '${label}'"

    local tmp_file="${cpp_file}.rm_tmp"
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
# Build Runtime Only (Step 1/1)
# Libs and tests were already built by the 3-step initial
# build, so only the runtime is rebuilt here.
# After a successful build the four .so files are copied
# into Core_Root.
# =========================================================
build_all_with_timeout() {
    local function_name="$1"
    local retry_count=0
    local build_success=false

    cd "$RUNTIME_DIR"
    export PATH="$DOTNET_ROOT:$PATH"
    export DOTNET_MULTILEVEL_LOOKUP=0
    export UseInstalledDotNetCli=true
    export NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS=30
    export NUGET_PLUGIN_REQUEST_TIMEOUT_IN_SECONDS=30
    export NUGET_HTTP_TIMEOUT_SECONDS=60
    export MSBUILDTERMINALLOGGER=off

    while [ $retry_count -lt $MAX_BUILD_RETRIES ]; do
        log_info "Building runtime for '$function_name' (attempt $((retry_count + 1))/$MAX_BUILD_RETRIES)..."

        cat > /tmp/build_runtime_only.sh << 'EOF'
#!/bin/bash
set -uo pipefail

# First argument is RUNTIME_DIR — cd into it so all relative paths are correct
cd "$1"

export NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS=30
export NUGET_PLUGIN_REQUEST_TIMEOUT_IN_SECONDS=30
export NUGET_HTTP_TIMEOUT_SECONDS=60
export MSBUILDTERMINALLOGGER=off

STEP_MAX_RETRIES=3
STEP_RETRY_SLEEP=30

run_step() {
    local label="$1"; shift
    local attempt=1
    while [ $attempt -le $STEP_MAX_RETRIES ]; do
        echo "[BUILD] [$label] attempt $attempt/$STEP_MAX_RETRIES..."
        if "$@"; then
            echo "[BUILD] [$label] succeeded on attempt $attempt. Sleeping 30s..."
            sleep 30
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

        chmod +x /tmp/build_runtime_only.sh

        if bash /tmp/build_runtime_only.sh "$RUNTIME_DIR" 2>&1 | tee "$LOG_DIR/build_all_${function_name}.log"; then
            build_success=true
            log_success "Runtime build succeeded for '$function_name'"
            break
        else
            log_error "Runtime build failed for '$function_name' (attempt $((retry_count + 1)))"
            retry_count=$((retry_count + 1))

            if [ $retry_count -lt $MAX_BUILD_RETRIES ]; then
                log_info "Retrying build for same function..."
                sleep 5
            fi
        fi
    done

    rm -f /tmp/build_runtime_only.sh
    cd "$WORKSPACE_DIR"

    if [ "$build_success" = false ]; then
        log_error "Runtime build failed after $MAX_BUILD_RETRIES attempts for '$function_name'"
        return 1
    fi

    return 0
}

# =========================================================
# Test ppc64_HelloWorld.dll via GDB (non-interactive)
#
# Runs:
#   gdb --batch --args <corerun> <HelloWorld.dll>
# with a hard timeout to catch hangs.
#
# Success criteria (ALL must hold):
#   - timeout did not fire (exit 124)
#   - gdb exit code is 0
#   - log contains no crash/signal indicators
#
# On ppc64le, gdb does not always emit the
# "[Inferior N exited normally]" line; instead all threads
# exit individually and gdb itself exits 0.  We therefore
# check for the ABSENCE of failure markers rather than the
# presence of a success string.
#
# Failure markers (any one → entry is removed):
#   "Program received signal"
#   "exited with code [1-9]"   (non-zero exit)
#   "Aborted"  /  "SIGABRT"  /  "SIGSEGV"  /  "SIGILL"
#   "SIGBUS"   /  "SIGFPE"   /  "SIGKILL"
#   "No such process"  (only when NOT accompanied by clean thread exits)
#
# All output is saved to:
#   logs/gdb_logs/<label>_gdb.log
# =========================================================
test_gdb() {
    local function_name="$1"
    local gdb_log="$GDB_LOG_DIR/${function_name}_gdb.log"

    log_info "Running GDB test for: $function_name"
    log_info "  corerun : $CORERUN_PATH"
    log_info "  dll     : $HELLO_WORLD_DLL"
    log_info "  log     : $gdb_log"

    export CORE_ROOT="$CORE_ROOT"
    export LD_LIBRARY_PATH="$CORE_ROOT"

    # Run gdb in batch mode:
    #   -batch      : non-interactive; gdb exits with the inferior's exit code
    #   "run"       : start the inferior immediately
    #   "quit"      : ensure gdb exits if the inferior stops for any reason
    # timeout(1) kills the whole process group after GDB_TIMEOUT seconds.
    local gdb_exit=0
    timeout "$GDB_TIMEOUT" \
        gdb --batch \
            -ex "set confirm off" \
            -ex "run" \
            -ex "quit" \
            --args "$CORERUN_PATH" "$HELLO_WORLD_DLL" \
        > "$gdb_log" 2>&1 || gdb_exit=$?

    # Echo captured output so it appears in the outer log stream
    cat "$gdb_log"

    # ── 1. Timeout check ──────────────────────────────────────
    # timeout exits 124 when it kills the child (stuck / hung)
    if [ "$gdb_exit" -eq 124 ]; then
        log_error "GDB test TIMED OUT (>${GDB_TIMEOUT}s) for: $function_name"
        echo "--- TIMED OUT after ${GDB_TIMEOUT}s ---" >> "$gdb_log"
        return 1
    fi

    # ── 2. gdb exit-code check ────────────────────────────────
    if [ "$gdb_exit" -ne 0 ]; then
        log_error "GDB test FAILED (gdb exited $gdb_exit) for: $function_name"
        return 1
    fi

    # ── 3. Crash / signal marker check ───────────────────────
    # On ppc64le, gdb may not print "[Inferior N exited normally]"
    # even on a clean run.  Instead we look for known failure strings.
    if grep -qE \
        "Program received signal|\
exited with code [1-9]|\
Aborted|SIGABRT|SIGSEGV|SIGILL|SIGBUS|SIGFPE|SIGKILL|\
Program terminated with signal" \
        "$gdb_log"; then
        log_error "GDB test FAILED (crash/signal detected in log) for: $function_name"
        return 1
    fi

    log_success "GDB test PASSED for: $function_name"
    return 0
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

    if ! add_function_to_cpp "$function_name"; then
        log_error "Failed to add function to cpp file"
        echo "$function_name - Failed to add to cpp" >> "$FAILED_FUNCTIONS_FILE"
        remove_function_from_list "$function_name"
        return 1
    fi

    if ! build_all_with_timeout "$function_name"; then
        log_error "Build failed for: $function_name - REMOVING from cpp and Functions_list.txt"
        remove_function_from_cpp "$function_name"
        remove_function_from_list "$function_name"
        echo "$function_name - Build failed" >> "$FAILED_FUNCTIONS_FILE"
        return 1
    fi

    if ! test_gdb "$function_name"; then
        log_error "GDB test failed for: $function_name - REMOVING from cpp and Functions_list.txt"
        remove_function_from_cpp "$function_name"
        remove_function_from_list "$function_name"
        echo "$function_name - GDB test failed" >> "$FAILED_FUNCTIONS_FILE"
        return 1
    fi

    log_success "Function '$function_name' passed build and GDB test - KEEPING in cpp file"
    echo "$function_name - $(date)" >> "$SUCCESS_LOG"
    echo "$function_name"           >> "$PROCESSED_FUNCTIONS_FILE"
    log_info "Total fallback functions in cpp: $(grep -c '        "' "$JITINTERFACE_CPP" 2>/dev/null || echo 0)"
    return 0
}

# =========================================================
# Process Single jitInclusionList Entry
#
# Input line format (from Interpreting-functions.txt):
#   ClassName:FunctionName
# Split on the FIRST colon only, so generic class names such as
#   System.Collections.Generic.Dictionary`2[Char,__Canon]
# are kept intact.
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

    if ! test_gdb "$safe_label"; then
        log_error "GDB test failed for '$label' — removing entry from cpp and list"
        remove_jit_inclusion_entry "$class_name" "$func_name"
        remove_entry_from_interpreting_list "$class_name" "$func_name"
        echo "$label - GDB test failed" >> "$FAILED_INCLUSIONS_FILE"
        return 1
    fi

    log_success "Entry '$label' passed build and GDB test - KEEPING in jitInclusionList"
    echo "$label - $(date)" >> "$SUCCESS_INCLUSIONS_LOG"
    return 0
}

# =========================================================
# Main Workflow
# =========================================================
main() {
    log_info "Starting four_step_Build workflow"
    log_info "Assumes the 3-step initial build (clr, libs, tests) is already complete."
    log_info "Phase 1 : plain function names -> coreclrInitializationInterpreterFallbackFunctions[]"
    log_info "Phase 2 : ClassName:FunctionName pairs -> jitInclusionList[]"
    log_info "Each phase is skipped automatically if its source file is missing or empty."

    # ----------------------------------------------------------
    # Phase 1 – coreclrInitializationInterpreterFallbackFunctions[]
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
            [[ -z "$line" || "$line" =~ ^[[:space:]]*# ]] && continue

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
            [[ -z "$line" || "$line" =~ ^[[:space:]]*# ]] && continue

            line=$(echo "$line" | tr -d '\r' | xargs)
            [[ -z "$line" ]] && continue

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
