using System;

// Test: Switch statement with caller/callee pattern and string return values
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
        
        // Test each case - separate calls not nested in if
        string result0 = ppc64leHelloWorldCallee(0);
        if (result0 == "Zero") passCount++;
        
        string result1 = ppc64leHelloWorldCallee(1);
        if (result1 == "One") passCount++;
        
        string result2 = ppc64leHelloWorldCallee(2);
        if (result2 == "Two") passCount++;
        
        string result3 = ppc64leHelloWorldCallee(3);
        if (result3 == "Three") passCount++;
        
        string result4 = ppc64leHelloWorldCallee(4);
        if (result4 == "Four") passCount++;
        
        string result5 = ppc64leHelloWorldCallee(5);
        if (result5 == "Five") passCount++;
        
        string result10 = ppc64leHelloWorldCallee(10);
        if (result10 == "Other") passCount++;
        
        return passCount;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Passed: {0}/7 tests", retVal);
        Console.WriteLine("Status: {0}", retVal == 7 ? "PASS" : "FAIL");
    }
}

// Test for PPC64LE switch statement with caller/callee pattern and string returns

// Made with Bob