using System;

// Test 10: Edge case - all registers used (8 ints + 13 floats, no stack)
class Program
{
    static double ppc64leHelloWorldCallee(int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8,
                                          double f1, double f2, double f3, double f4, double f5,
                                          double f6, double f7, double f8, double f9, double f10,
                                          double f11, double f12, double f13)
    {
        return i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 +
               f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13;
    }

    static double ppc64leHelloWorld()
    {
        double sum = ppc64leHelloWorldCallee(1, 2, 3, 4, 5, 6, 7, 8,
                                              1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0,
                                              9.0, 10.0, 11.0, 12.0, 13.0);
        return sum;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 10: All registers used (21 args: 8 int + 13 float)");
        Console.WriteLine("Result: {0:F1}, Expected: 127.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 127.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob
