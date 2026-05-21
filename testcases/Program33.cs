using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 10.5f;
        float y = 5.4f;
        
        if (x > y)
            goto Test2;
        goto Failure;
        
    Test2:
        if (x >= y)
            goto Test3;
        goto Failure;
        
    Test3:
        if (y < x)
            goto Test4;
        goto Failure;
        
    Test4:
        if (y <= x)
            goto Test5;
        goto Failure;
        
    Test5:
        if (x != y)
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

