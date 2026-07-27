using System;

// Test: Basic switch statement with string return values
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

    static void Main(string[] args)
    {
        string result0  = ppc64leHelloWorldCallee(0);
        string result1  = ppc64leHelloWorldCallee(1);
        string result2  = ppc64leHelloWorldCallee(2);
        string result3  = ppc64leHelloWorldCallee(3);
        string result4  = ppc64leHelloWorldCallee(4);
        string result5  = ppc64leHelloWorldCallee(5);
        string result10 = ppc64leHelloWorldCallee(10);

        Console.WriteLine("Test 0: Expected Zero, Got {0}", result0);
        Console.WriteLine("Status: {0}", result0 == "Zero" ? "PASS" : "FAIL");
        Console.WriteLine("Test 1: Expected One, Got {0}", result1);
        Console.WriteLine("Status: {0}", result1 == "One" ? "PASS" : "FAIL");
        Console.WriteLine("Test 2: Expected Two, Got {0}", result2);
        Console.WriteLine("Status: {0}", result2 == "Two" ? "PASS" : "FAIL");
        Console.WriteLine("Test 3: Expected Three, Got {0}", result3);
        Console.WriteLine("Status: {0}", result3 == "Three" ? "PASS" : "FAIL");
        Console.WriteLine("Test 4: Expected Four, Got {0}", result4);
        Console.WriteLine("Status: {0}", result4 == "Four" ? "PASS" : "FAIL");
        Console.WriteLine("Test 5: Expected Five, Got {0}", result5);
        Console.WriteLine("Status: {0}", result5 == "Five" ? "PASS" : "FAIL");
        Console.WriteLine("Test 10: Expected Other, Got {0}", result10);
        Console.WriteLine("Status: {0}", result10 == "Other" ? "PASS" : "FAIL");

        int passCount = 0;
        if (result0  == "Zero")  passCount++;
        if (result1  == "One")   passCount++;
        if (result2  == "Two")   passCount++;
        if (result3  == "Three") passCount++;
        if (result4  == "Four")  passCount++;
        if (result5  == "Five")  passCount++;
        if (result10 == "Other") passCount++;

        Console.WriteLine("Passed: {0}/7", passCount);
        Console.WriteLine("Status: {0}", passCount == 7 ? "PASS" : "FAIL");
    }
}

// Made with Bob
