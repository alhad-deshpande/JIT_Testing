using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        int x = 15;
        
        if (x > 10)
            goto CheckUpper;
        
        goto TooSmall;
        
    CheckUpper:
        if (x < 20)
            goto InRange;
        
        goto TooLarge;
        
    InRange:
        return 1;
    TooSmall:
        return 0;
    TooLarge:
        return 2;
    }
    
    static void Main()
    {
        Console.WriteLine("Answer: {0}", ppc64leHelloWorld());
    }
}

