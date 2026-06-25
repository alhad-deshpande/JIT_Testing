using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: 2D Array");
        
        int[,] arr = new int[2, 3];
        
        arr[0, 0] = 1;
        arr[0, 1] = 2;
        arr[0, 2] = 3;
        arr[1, 0] = 4;
        arr[1, 1] = 5;
        arr[1, 2] = 6;
        
        int sum = 0;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                sum += arr[i, j];
            }
        }
        
        Console.WriteLine($"Sum: {sum}, Expected: 21");
        return (sum == 21) ? 10 : 0;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

