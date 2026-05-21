using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 3.2f;
        float y = 3.0f;

        if (x >= y)
            goto GreaterOrEqual;

        goto NotGreaterOrEqual;

    GreaterOrEqual:
        return 1;
    NotGreaterOrEqual:
        return 0;
    }
    
    static void Main()
    {
        Console.WriteLine("Answer: {0}", ppc64leHelloWorld());
    }
}
