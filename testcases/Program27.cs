using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 0.0f;
        float y = 0.0f;

        if (x ==  y)
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

