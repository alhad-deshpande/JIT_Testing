class Program
{
    static int ppc64leHelloWorldCallee(int a, int b)
    {
            return a + b;
    }

    static int ppc64leHelloWorld()
    {
          int sum = ppc64leHelloWorldCallee(10, 20);
          return sum;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Hello World -> After returning from JITted functions! Return Value = {0}", retVal);
    }
}

