using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 3.2f;
        float y = 3.2f;

        if (x <= y)
            goto LessOrEqual;

        goto NotLessOrEqual;

    LessOrEqual:
        return 1;
    NotLessOrEqual:
        return 0;
    }
    
    static void Main()
    {
        Console.WriteLine("Answer: {0}", ppc64leHelloWorld());
    }
}
