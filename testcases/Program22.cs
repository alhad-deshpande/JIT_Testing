using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 2.5f;
        float y = 2.5f;

        if (x == y)
            goto Equal;

        goto NotEqual;

    Equal:
        return 1;
    NotEqual:
        return 0;
    }
    
    static void Main()
    {
        Console.WriteLine("Answer: {0}", ppc64leHelloWorld());
    }
}
