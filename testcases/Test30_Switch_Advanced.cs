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

    static int ppc64leHelloWorld()
    {
        int passCount = 0;
        
        // Test each case - separate calls not nested in if
        int result0 = ppc64leHelloWorldCallee(0);
        if (result0 == 100) passCount++;
        
        int result1 = ppc64leHelloWorldCallee(1);
        if (result1 == 101) passCount++;
        
        int result2 = ppc64leHelloWorldCallee(2);
        if (result2 == 102) passCount++;
        
        int result3 = ppc64leHelloWorldCallee(3);
        Console.WriteLine($"Test 3: Expected 103, Got {result3}, {(result3 == 103 ? "PASS" : "FAIL")}");
        if (result3 == 103) passCount++;
        
        int result4 = ppc64leHelloWorldCallee(4);
        Console.WriteLine($"Test 4: Expected 104, Got {result4}, {(result4 == 104 ? "PASS" : "FAIL")}");
        if (result4 == 104) passCount++;
        
        int result5 = ppc64leHelloWorldCallee(5);
        Console.WriteLine($"Test 5: Expected 105, Got {result5}, {(result5 == 105 ? "PASS" : "FAIL")}");
        if (result5 == 105) passCount++;
        
        int result10 = ppc64leHelloWorldCallee(10);
        Console.WriteLine($"Test 10: Expected 999, Got {result10}, {(result10 == 999 ? "PASS" : "FAIL")}");
        if (result10 == 999) passCount++;
        
        return passCount;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Passed: {0}/7 tests", retVal);
        Console.WriteLine("Status: {0}", retVal == 7 ? "PASS" : "FAIL");
    }
}

// Test for PPC64LE switch statement implementation

// Made with Bob
