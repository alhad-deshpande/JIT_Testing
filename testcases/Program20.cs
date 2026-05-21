using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 5.5f;
        float y = 3.2f;

        if (x > y)
            goto Greater;

        goto NotGreater;

    Greater:
        return 1;
    NotGreater:
        return 0;
    }
    
    static void Main()
    {
        Console.WriteLine("Answer: {0}", ppc64leHelloWorld());
    }
}

