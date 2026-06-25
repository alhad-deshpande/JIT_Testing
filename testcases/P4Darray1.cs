using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: 4D Array");
        
        // Small 4D array: [2,2,2,2] = 16 elements
        int[,,,] hyper = new int[2, 2, 2, 2];
        
        // Fill with values 1-16
        int val = 1;
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                for (int k = 0; k < 2; k++)
                    for (int l = 0; l < 2; l++)
                        hyper[i, j, k, l] = val++;
        
        // Sum all: 1+2+...+16 = 136
        int sum = 0;
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                for (int k = 0; k < 2; k++)
                    for (int l = 0; l < 2; l++)
                        sum += hyper[i, j, k, l];
        
        Console.WriteLine($"4D sum: {sum}, Expected: 136");
        return (sum == 136) ? 10 : 0;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

