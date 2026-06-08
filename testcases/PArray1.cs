using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Minimal Array Test");
        
        // Simplest possible array test
        int[] arr = new int[3];
        arr[0] = 10;
        arr[1] = 20;
        arr[2] = 30;
        
        int result = arr[0] + arr[1] + arr[2];
        Console.WriteLine($"Result: {result}");
        
        return result;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}
