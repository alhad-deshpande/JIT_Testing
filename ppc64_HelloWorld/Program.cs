class Program
{
    static double ppc64leHelloWorldCallee(double a, double b)
    {
            return a + b;
    }

    static double ppc64leHelloWorld()
    {
          double sum = ppc64leHelloWorldCallee(10.250025, 20.500050);
          return sum;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Hello World -> After returning from JITted functions! Return Value = {0}", retVal);
    }
}
