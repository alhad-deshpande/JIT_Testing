using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        double x = 10.56;
        double y = 5.25;

        if (x >  y)
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
