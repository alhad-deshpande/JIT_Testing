using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("=== Array Addition Tests ===\n");
        
        // Test 1: Add two arrays element-wise
        Console.WriteLine("Test 1: Element-wise Addition");
        int[] a = new int[5];
        int[] b = new int[5];
        int[] c = new int[5];
        
        a[0] = 1; a[1] = 2; a[2] = 3; a[3] = 4; a[4] = 5;
        b[0] = 10; b[1] = 20; b[2] = 30; b[3] = 40; b[4] = 50;
        
        c[0] = a[0] + b[0];
        c[1] = a[1] + b[1];
        c[2] = a[2] + b[2];
        c[3] = a[3] + b[3];
        c[4] = a[4] + b[4];
        
        Console.WriteLine($"  a[0]={a[0]}, b[0]={b[0]}, c[0]={c[0]}");
        Console.WriteLine($"  a[1]={a[1]}, b[1]={b[1]}, c[1]={c[1]}");
        Console.WriteLine($"  a[2]={a[2]}, b[2]={b[2]}, c[2]={c[2]}");
        Console.WriteLine($"  a[3]={a[3]}, b[3]={b[3]}, c[3]={c[3]}");
        Console.WriteLine($"  a[4]={a[4]}, b[4]={b[4]}, c[4]={c[4]}");
        
        // Test 2: Sum all elements
        Console.WriteLine("\nTest 2: Sum All Elements");
        int sum = c[0] + c[1] + c[2] + c[3] + c[4];
        Console.WriteLine($"  Sum: {sum}");
        Console.WriteLine($"  Expected: 165 (11+22+33+44+55)");
        
        // Test 3: Different data types
        Console.WriteLine("\nTest 3: Byte Array Addition");
        byte[] x = new byte[3];
        byte[] y = new byte[3];
        
        x[0] = 5; x[1] = 10; x[2] = 15;
        y[0] = 3; y[1] = 6; y[2] = 9;
        
        int byteSum = (x[0] + y[0]) + (x[1] + y[1]) + (x[2] + y[2]);
        Console.WriteLine($"  Byte sum: {byteSum}");
        Console.WriteLine($"  Expected: 48 (8+16+24)");
        
        // Test 4: Long array
        Console.WriteLine("\nTest 4: Long Array Addition");
        long[] p = new long[2];
        long[] q = new long[2];
        
        p[0] = 1000L; p[1] = 2000L;
        q[0] = 500L; q[1] = 1500L;
        
        long longSum = (p[0] + q[0]) + (p[1] + q[1]);
        Console.WriteLine($"  Long sum: {longSum}");
        Console.WriteLine($"  Expected: 5000");
        
        // Calculate score
        int score = 0;
        if (sum == 165) score += 25;
        if (byteSum == 48) score += 25;
        if (longSum == 5000) score += 25;
        if (c[2] == 33) score += 25;
        
        Console.WriteLine($"\n=== Score: {score}/100 ===");
        return score;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

