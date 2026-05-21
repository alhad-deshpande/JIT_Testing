using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 1.5f;
        float y = 2.5f;

        if (x != y)
            goto NotEqual;

        goto Equal;

    NotEqual:
        return 1;
    Equal:
        return 0;
    }
    
    static void Main()
    {
        Console.WriteLine("Answer: {0}", ppc64leHelloWorld());
    }
}

