using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: Different Element Sizes");
        
        byte[] bytes = new byte[4];
        bytes[0] = 10;
        bytes[1] = 20;
        bytes[2] = 30;
        bytes[3] = 40;
        int byteSum = bytes[0] + bytes[1] + bytes[2] + bytes[3];
        
        short[] shorts = new short[3];
        shorts[0] = 100;
        shorts[1] = 200;
        shorts[2] = 300;
        int shortSum = shorts[0] + shorts[1] + shorts[2];
        
        long[] longs = new long[2];
        longs[0] = 1000L;
        longs[1] = 2000L;
        long longSum = longs[0] + longs[1];
        
        bool pass = (byteSum == 100) && (shortSum == 600) && (longSum == 3000L);
        Console.WriteLine($"Byte: {byteSum}, Short: {shortSum}, Long: {longSum}");
        return pass ? 10 : 0;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

