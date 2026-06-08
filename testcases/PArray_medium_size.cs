using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: Medium Array (100 elements)");
        int[] arr = new int[100];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = i;
        }
        int sum = 0;
        for (int i = 0; i < 50; i++)
        {
            sum += arr[i];
        }
        Console.WriteLine($"Sum of first 50: {sum}, Expected: 1225");
        return (sum == 1225) ? 10 : 0;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

