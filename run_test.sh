#!/bin/bash

set +e

# ======================================
# Base Directory (Dynamic)
# ======================================
BASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"


# ======================================
# Input Arguments
# Usage:
#   ./run_test.sh <dotnet_root> <runtime_dir>
# ======================================

DOTNET_ROOT_ARG="$1"
RUNTIME_DIR_ARG="$2"

# Use passed arguments OR fallback to repo paths
if [ -n "$DOTNET_ROOT_ARG" ]; then
    DOTNET_ROOT="$DOTNET_ROOT_ARG"
else
    DOTNET_ROOT="$BASE_DIR/runtime/.dotnet"
fi

if [ -n "$RUNTIME_DIR_ARG" ]; then
    RUNTIME_DIR="$RUNTIME_DIR_ARG"
else
    RUNTIME_DIR="$BASE_DIR/runtime"
fi

# Add to PATH (optional but ok)
export PATH="$DOTNET_ROOT:$PATH"


# ======================================
# Validate dotnet
# ======================================
if [ ! -f "$DOTNET_ROOT/dotnet" ]; then
    echo "ERROR: dotnet not found at $DOTNET_ROOT/dotnet"
    echo "Usage: ./run_test.sh <dotnet_root_path> <runtime_path>"
    exit 1
fi

chmod +x "$DOTNET_ROOT/dotnet"

echo "Using DOTNET_ROOT: $DOTNET_ROOT"
echo "Using RUNTIME_DIR: $RUNTIME_DIR"


# ======================================
# Check Required Commands
# ======================================
for cmd in  grep sed; do
    if ! command -v "$cmd" > /dev/null 2>&1; then
        echo "ERROR: Required command '$cmd' not found"
        exit 1
    fi
done


# ======================================
# Paths (Dynamic)
# ======================================
HELLO_WORLD_DIR="$BASE_DIR/ppc64_HelloWorld"
TESTCASE_DIR="$BASE_DIR/testcases"

PROGRAM_CS="$HELLO_WORLD_DIR/Program.cs"
DLL_PATH="$HELLO_WORLD_DIR/bin/Debug/net9.0/ppc64_HelloWorld.dll"
CORERUN_PATH="$RUNTIME_DIR/artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root/corerun"


# ======================================
# Counters
# ======================================

TOTAL=0
PASS=0
FAIL=0

# ======================================
# Normalize Output
# ======================================

normalize_output() {
    printf "%s" "$1" \
    | tr -d '\r' \
    | sed 's/\x1B\[[0-9;:]*[a-zA-Z]//g' \
    | sed 's/\x1B][0-9;]*//g' \
    | tr -cd '\11\12\15\40-\176' \
    | sed 's/^\\//' \
    | sed '/^$/d' \
    | sed '/^Function name is/d' \
    | sed '/^realization.EnableUnsafeBinaryFormatterSerialization/d' \
    | sed 's/&gt;/>/g; s/&lt;/</g; s/&amp;/\&/g' \
    | sed 's/^[[:space:]]*//' \
    | sed 's/[[:space:]]*$//' \
    | tr -s ' '
}

# ======================================
# Run all testcases
# ======================================

for testcase in $(ls $TESTCASE_DIR/*.cs | sort -V)
do

    FILE_NAME=$(basename "$testcase")

    echo "========================================"
    echo "Running Testcase: $FILE_NAME"
    echo "========================================"

    # ======================================
    # Copy testcase to Program.cs
    # ======================================

    cp "$testcase" "$PROGRAM_CS"

    # ======================================
    # Build project
    # ======================================

    cd "$HELLO_WORLD_DIR"

    echo ""
    echo "Running: dotnet build"
    echo ""

	if ! "$DOTNET_ROOT/dotnet" build; then

        echo "ERROR: Build failed"

        FAIL=$((FAIL + 1))
        TOTAL=$((TOTAL + 1))

        continue
    fi

    cp $RUNTIME_DIR/artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root/*.so \
       $HELLO_WORLD_DIR/bin/Debug/net9.0/

    echo ""
    echo "Build succeeded"
    echo ""

    # ======================================
    # Expected Output = dotnet run
    # ======================================

    echo ""
    echo "Running: dotnet run"
    echo ""

	if ! EXPECTED_RAW=$("$DOTNET_ROOT/dotnet" run 2>&1); then
        echo "ERROR: dotnet run failed"

        echo "$EXPECTED_RAW"

        FAIL=$((FAIL + 1))
        TOTAL=$((TOTAL + 1))

        continue
    fi

    # ======================================
    # Actual Output = corerun
    # ======================================

    cd "$RUNTIME_DIR"

    export LD_LIBRARY_PATH=$RUNTIME_DIR/artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root:$LD_LIBRARY_PATH

    echo ""
    echo "Running: corerun"
    echo ""

    if ! ACTUAL_RAW=$($CORERUN_PATH "$DLL_PATH" 2>&1); then

        echo "ERROR: corerun execution failed"

        echo "$ACTUAL_RAW"

        FAIL=$((FAIL + 1))
        TOTAL=$((TOTAL + 1))

        continue
    fi

    # ======================================
    # Normalize Outputs
    # ======================================

    EXPECTED=$(normalize_output "$EXPECTED_RAW")

    ACTUAL=$(normalize_output "$ACTUAL_RAW")

	EXPECTED=$(normalize_output "$EXPECTED_RAW" | xargs)

	ACTUAL=$(normalize_output "$ACTUAL_RAW" | xargs)

    # ======================================
    # Empty Output Validation
    # ======================================

    if [ -z "${EXPECTED// }" ]; then

        echo "ERROR: Expected output is empty"

        FAIL=$((FAIL + 1))
        TOTAL=$((TOTAL + 1))

        continue
    fi

    if [ -z "${ACTUAL// }" ]; then

        echo "ERROR: Actual output is empty"

        FAIL=$((FAIL + 1))
        TOTAL=$((TOTAL + 1))

        continue
    fi

    # ======================================
    # Print Outputs
    # ======================================

    printf "\nExpected Output = [%s]\n" "$EXPECTED"

    printf "\nActual Output   = [%s]\n" "$ACTUAL"

    # ======================================
    # PASS / FAIL
    # ======================================

    TOTAL=$((TOTAL + 1))

    if [ "$EXPECTED" = "$ACTUAL" ]; then

        echo ""
        echo "RESULT: PASS"

        PASS=$((PASS + 1))

    else

        echo ""
        echo "RESULT: FAIL"

        FAIL=$((FAIL + 1))
    fi

    echo ""

done

# ======================================
# Final Summary
# ======================================

echo "========================================"
echo "TEST SUMMARY"
echo "========================================"

echo "Total Testcases Run : $TOTAL"

echo "Testcases Passed    : $PASS"

echo "Testcases Failed    : $FAIL"

echo "========================================"
