using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int flags = 0;
        
        // Set bit 0 (OR with 1)
        flags = flags | 1;        // flags = 1
        
        // Set bit 2 (OR with 4)
        flags = flags | 4;        // flags = 5
        
        // Clear bit 0 (AND with ~1 = -2)
        flags = flags & (~1);     // flags = 4
        
        // Toggle bit 1 (XOR with 2)
        flags = flags ^ 2;        // flags = 6
        
        return flags;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

