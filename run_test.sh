#!/bin/bash

set +e

# ======================================
# DOTNET Setup
# ======================================

export DOTNET_ROOT=/root/Ipshita_JIT/runtime/.dotnet/
export PATH=$PATH:$DOTNET_ROOT

# ======================================
# Paths
# ======================================

HELLO_WORLD_DIR="/root/Ipshita_JIT/ppc64_HelloWorld"
RUNTIME_DIR="/root/Ipshita_JIT/runtime"
TESTCASE_DIR="/root/Ipshita_JIT/testcases"
EXPECTED_FILE="$TESTCASE_DIR/Outputs.txt"

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
# Run all testcases
# ======================================

#for testcase in $TESTCASE_DIR/*.cs
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

	dotnet build

	cp $RUNTIME_DIR/artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root/*.so \
	$HELLO_WORLD_DIR/bin/Debug/net9.0/

	echo ""
	echo "Build succeeded"
	echo ""

    # ======================================
    # Run testcase
    # ======================================

    cd "$RUNTIME_DIR"

    export LD_LIBRARY_PATH=$RUNTIME_DIR/artifacts/tests/coreclr/linux.ppc64le.Debug/Tests/Core_Root:$LD_LIBRARY_PATH

    #OUTPUT=$($CORERUN_PATH "$DLL_PATH")
    OUTPUT=$($CORERUN_PATH "$DLL_PATH" 2>&1)
    echo ""

    echo "$OUTPUT"

    # ======================================
    # Extract actual value
    # ======================================

   #ACTUAL=$(echo "$OUTPUT" | grep -oP 'Return Value = \K[0-9]+|Answer C = \K[0-9]+' | tail -1)
   #ACTUAL=$(echo "$OUTPUT" | grep -oP 'Return Value = \K[0-9]+(\.[0-9]+)?|Answer C = \K[0-9]+(\.[0-9]+)?' | tail -1)
   ACTUAL=$(echo "$OUTPUT" | grep -oP 'Answer:\s*\K-?[0-9]+(\.[0-9]+)?|Answer C = \K-?[0-9]+(\.[0-9]+)?|Return Value = \K-?[0-9]+(\.[0-9]+)?' | tail -1)

    # ======================================
    # Get expected value
    # ======================================

    EXPECTED=$(grep "$FILE_NAME" "$EXPECTED_FILE" | cut -d':' -f2)

    echo "Expected Output = $EXPECTED"
    echo "Actual Output   = $ACTUAL"

    # ======================================
    # PASS / FAIL
    # ======================================


    TOTAL=$((TOTAL + 1))

    if [ "$EXPECTED" = "$ACTUAL" ]; then
        echo "RESULT: PASS"
        PASS=$((PASS + 1))
    else
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
