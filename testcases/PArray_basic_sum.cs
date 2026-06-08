using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: Basic Array Sum");
        int[] arr = new int[10];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = i + 1;
        }
        int sum = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            sum += arr[i];
        }
        Console.WriteLine($"Sum: {sum}, Expected: 55");
        return (sum == 55) ? 10 : 0;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

