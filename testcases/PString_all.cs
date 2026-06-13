using System;
using System.Runtime.CompilerServices;

public class StringTests
{
    // ===== Callee Functions =====
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int ppc64leStringLengthCallee(string s)
    {
        return s.Length;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static char ppc64leStringIndexerCallee(string s, int index)
    {
        return s[index];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool ppc64leStringEqualsCallee(string s1, string s2)
    {
        return s1.Equals(s2);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool ppc64leStringStartsWithCallee(string s, string prefix)
    {
        return s.StartsWith(prefix);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool ppc64leStringEndsWithCallee(string s, string suffix)
    {
        return s.EndsWith(suffix);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static string ppc64leStringConcatCallee(string s1, string s2)
    {
        return s1 + s2;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool ppc64leStringEqualsOrdinalCallee(string s, string cmp)
    {
        return s.Equals(cmp, StringComparison.Ordinal);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool ppc64leStringEqualsIgnoreCaseCallee(string s, string cmp)
    {
        return s.Equals(cmp, StringComparison.OrdinalIgnoreCase);
    }

    // ===== SINGLE HOOK FUNCTION - ALL STRING TESTS =====
    
    static int ppc64leHelloWorld()
    {
        // Test 1: String.Length
        string s1 = "Hello";
        int len = ppc64leStringLengthCallee(s1);
        if (len != 5) return 1;

        // Test 2: String indexer
        char ch = ppc64leStringIndexerCallee(s1, 0);
        if (ch != 'H') return 2;

        // Test 3: String.Equals (same)
        bool eq1 = ppc64leStringEqualsCallee("Hello", "Hello");
        if (!eq1) return 3;

        // Test 4: String.Equals (different)
        bool eq2 = ppc64leStringEqualsCallee("Hello", "World");
        if (eq2) return 4;

        // Test 5: String.Equals with constant
        bool eq3 = ppc64leStringEqualsCallee(s1, "Hello");
        if (!eq3) return 5;

        // Test 6: String.StartsWith
        bool sw = ppc64leStringStartsWithCallee("Hello", "Hel");
        if (!sw) return 6;

        // Test 7: String.EndsWith
        bool ew = ppc64leStringEndsWithCallee("World", "rld");
        if (!ew) return 7;

        // Test 8: String concatenation
        string concat = ppc64leStringConcatCallee("Hello", "World");
        if (concat != "HelloWorld") return 8;

        // Test 9: Empty string
        bool isEmpty = (string.Empty == "");
        if (!isEmpty) return 9;

        // Test 10: Empty string length
        int emptyLen = string.Empty.Length;
        if (emptyLen != 0) return 10;

        // Test 11: String.Equals with Ordinal
        bool eqOrd = ppc64leStringEqualsOrdinalCallee("Test", "Test");
        if (!eqOrd) return 11;

        // Test 12: String.Equals with IgnoreCase
        bool eqIgnore = ppc64leStringEqualsIgnoreCaseCallee("TEST", "test");
        if (!eqIgnore) return 12;

        // Test 13: String literal length
        int litLen = "TestString".Length;
        if (litLen != 10) return 13;

        // Test 14: String literal indexer
        char litChar = "ABC"[1];
        if (litChar != 'B') return 14;

        // Test 15: Multiple string operations
        string s2 = "World";
        if (s1.Length != 5) return 15;
        if (s1[0] != 'H') return 16;
        if (!s1.Equals("Hello")) return 17;
        if (s1.Equals(s2)) return 18;
        
        string s3 = s1 + s2;
        if (s3 != "HelloWorld") return 19;
        if (s3.Length != 10) return 20;
        if (!s3.StartsWith("Hello")) return 21;
        if (!s3.EndsWith("World")) return 22;

        // All tests passed!
        return 100;
    }

    // ===== Main =====
    
    public static int Main()
    {
        Console.WriteLine("=== PowerPC64LE String Operations Test ===\n");
        
        int result = ppc64leHelloWorld();
        
        if (result == 100)
        {
            Console.WriteLine("SUCCESS: All string tests passed!");
            Console.WriteLine("Return code: 100");
	    return 0;
        }
        else
        {
            Console.WriteLine($"FAILED: String test failed at checkpoint {result}");
            Console.WriteLine($"Return code: {result}");
	    return result;
        }
        
        
    }
}

