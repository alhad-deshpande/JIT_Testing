using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: 2D Arrays Different Types");
        
        int score = 0;
        
        // Test byte array
        byte[,] bytes = new byte[2, 2];
        bytes[0, 0] = 1;
        bytes[0, 1] = 2;
        bytes[1, 0] = 3;
        bytes[1, 1] = 4;
        int byteSum = bytes[0, 0] + bytes[0, 1] + bytes[1, 0] + bytes[1, 1];
        Console.WriteLine($"Byte sum: {byteSum}, Expected: 10");
        if (byteSum == 10) score += 5;
        
        // Test long array
        long[,] longs = new long[2, 2];
        longs[0, 0] = 100L;
        longs[0, 1] = 200L;
        longs[1, 0] = 300L;
        longs[1, 1] = 400L;
        long longSum = longs[0, 0] + longs[0, 1] + longs[1, 0] + longs[1, 1];
        Console.WriteLine($"Long sum: {longSum}, Expected: 1000");
        if (longSum == 1000L) score += 5;
        
        return score;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

