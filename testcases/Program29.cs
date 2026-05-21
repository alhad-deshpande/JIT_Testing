using System;

class Program
{
    static int ppc64leHelloWorld()
    {
        float x = 10.5f;
        float y = 3.5f;
        float z = 2.0f;
        
        float result = (x * y) + (z / 2.0f);
        
        if (result > 37.0f)
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
