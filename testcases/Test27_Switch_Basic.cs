using System;

// Test: Basic switch statement with integer cases
class Program
{
    static int ppc64leHelloWorldCallee(int value)
    {
        switch (value)
        {
            case 0:
                return 100;
            case 1:
                return 101;
            case 2:
                return 102;
            case 3:
                return 103;
            case 4:
                return 104;
            case 5:
                return 105;
            default:
                return 999;
        }
    }

    static void Main(string[] args)
    {
        // Test each case individually and print results
        int result0 = ppc64leHelloWorldCallee(0);
        int result1 = ppc64leHelloWorldCallee(1);
        int result2 = ppc64leHelloWorldCallee(2);
        int result3 = ppc64leHelloWorldCallee(3);
        int result4 = ppc64leHelloWorldCallee(4);
        int result5 = ppc64leHelloWorldCallee(5);
        int result10 = ppc64leHelloWorldCallee(10);
        
        Console.WriteLine("Test 0: Expected 100, Got {0}, {1}", result0, result0 == 100 ? "PASS" : "FAIL");
        Console.WriteLine("Test 1: Expected 101, Got {0}, {1}", result1, result1 == 101 ? "PASS" : "FAIL");
        Console.WriteLine("Test 2: Expected 102, Got {0}, {1}", result2, result2 == 102 ? "PASS" : "FAIL");
        Console.WriteLine("Test 3: Expected 103, Got {0}, {1}", result3, result3 == 103 ? "PASS" : "FAIL");
        Console.WriteLine("Test 4: Expected 104, Got {0}, {1}", result4, result4 == 104 ? "PASS" : "FAIL");
        Console.WriteLine("Test 5: Expected 105, Got {0}, {1}", result5, result5 == 105 ? "PASS" : "FAIL");
        Console.WriteLine("Test 10: Expected 999, Got {0}, {1}", result10, result10 == 999 ? "PASS" : "FAIL");
        
        int passCount = 0;
        if (result0 == 100) passCount++;
        if (result1 == 101) passCount++;
        if (result2 == 102) passCount++;
        if (result3 == 103) passCount++;
        if (result4 == 104) passCount++;
        if (result5 == 105) passCount++;
        if (result10 == 999) passCount++;
        
        Console.WriteLine("\nPassed: {0}/7 tests", passCount);
        Console.WriteLine("Status: {0}", passCount == 7 ? "PASS" : "FAIL");
    }
}

// Test for PPC64LE switch statement implementation

// Made with Bob
