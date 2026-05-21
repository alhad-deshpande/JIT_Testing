using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        double x = 10.56;
        double y = 5.25;
        double z = 2.5;
        
        double result = (x * y) / z;
        
        if (result > 22.0)
            goto Success;
        
        goto Failure;
        
    Success:
        return 1;
    Failure:
        return 0;
    }
    
    static void Main()
    {
        Console.WriteLine("Answer: {0}", ppc64leHelloWorld());
    }
}

