using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 3.2f;
        float y = 5.5f;

        if (x < y)
            goto Less;

        goto NotLess;

    Less:
        return 1;
    NotLess:
        return 0;
    }
    
    static void Main()
    {
        Console.WriteLine("Answer: {0}", ppc64leHelloWorld());
    }
}
