using System;

// Test: Switch statement with nested calls in if conditions
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
        
        // Test each case - calls nested directly in if conditions
        if (ppc64leHelloWorldCallee(0) == 100) passCount++;
        if (ppc64leHelloWorldCallee(1) == 101) passCount++;
        if (ppc64leHelloWorldCallee(2) == 102) passCount++;
        if (ppc64leHelloWorldCallee(3) == 103) passCount++;
        if (ppc64leHelloWorldCallee(4) == 104) passCount++;
        if (ppc64leHelloWorldCallee(5) == 105) passCount++;
        if (ppc64leHelloWorldCallee(10) == 999) passCount++;
        
        return passCount;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Passed: {0}/7 tests", retVal);
        Console.WriteLine("Status: {0}", retVal == 7 ? "PASS" : "FAIL");
    }
}

// Test for PPC64LE switch statement with nested calls in if conditions

// Made with Bob