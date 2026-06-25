using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: 2D Array Diagonal Sum");
        
        int[,] matrix = new int[3, 3];
        
        // Fill matrix: 1,2,3,4,5,6,7,8,9
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                matrix[i, j] = i * 3 + j + 1;
            }
        }
        
        // Calculate diagonal: 1 + 5 + 9 = 15
        int diagSum = 0;
        for (int i = 0; i < 3; i++)
        {
            diagSum += matrix[i, i];
        }
        
        Console.WriteLine($"Diagonal sum: {diagSum}, Expected: 15");
        return (diagSum == 15) ? 10 : 0;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

