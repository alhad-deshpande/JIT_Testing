using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: 3D Array");
        
        int[,,] cube = new int[2, 2, 2];
        
        // Fill cube: 1,2,3,4,5,6,7,8
        int val = 1;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    cube[i, j, k] = val++;
                }
            }
        }
        
        // Sum all elements: 1+2+3+4+5+6+7+8 = 36
        int sum = 0;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    sum += cube[i, j, k];
                }
            }
        }
        
        Console.WriteLine($"3D array sum: {sum}, Expected: 36");
        return (sum == 36) ? 10 : 0;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

