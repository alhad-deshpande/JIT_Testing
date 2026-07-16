using System;

// Test: Switch statement with nested calls in if conditions and string return values
class Program
{
    static string ppc64leHelloWorldCallee(int value)
    {
        switch (value)
        {
            case 0:
                return "Zero";
            case 1:
                return "One";
            case 2:
                return "Two";
            case 3:
                return "Three";
            case 4:
                return "Four";
            case 5:
                return "Five";
            default:
                return "Other";
        }
    }

    static int ppc64leHelloWorld()
    {
        int passCount = 0;
        
        // Test each case - calls nested directly in if conditions
        if (ppc64leHelloWorldCallee(0) == "Zero") passCount++;
        if (ppc64leHelloWorldCallee(1) == "One") passCount++;
        if (ppc64leHelloWorldCallee(2) == "Two") passCount++;
        if (ppc64leHelloWorldCallee(3) == "Three") passCount++;
        if (ppc64leHelloWorldCallee(4) == "Four") passCount++;
        if (ppc64leHelloWorldCallee(5) == "Five") passCount++;
        if (ppc64leHelloWorldCallee(10) == "Other") passCount++;
        
        return passCount;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Passed: {0}/7 tests", retVal);
        Console.WriteLine("Status: {0}", retVal == 7 ? "PASS" : "FAIL");
    }
}

// Test for PPC64LE switch statement with nested calls and string returns

// Made with Bob