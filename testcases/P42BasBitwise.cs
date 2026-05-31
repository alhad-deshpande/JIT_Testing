using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int a = 12;  // 0b1100
        int b = 10;  // 0b1010
        
        int and_result = a & b;   // 0b1000 = 8
        int or_result = a | b;    // 0b1110 = 14
        int xor_result = a ^ b;   // 0b0110 = 6
        
        return and_result + or_result + xor_result;  // 8 + 14 + 6 = 28
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

