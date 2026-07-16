using System;

// Test: Switch statement with caller/callee pattern
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
        if (result3 == 103) passCount++;
        
        int result4 = ppc64leHelloWorldCallee(4);
        if (result4 == 104) passCount++;
        
        int result5 = ppc64leHelloWorldCallee(5);
        if (result5 == 105) passCount++;
        
        int result10 = ppc64leHelloWorldCallee(10);
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

// Test for PPC64LE switch statement with caller/callee pattern

// Made with Bob