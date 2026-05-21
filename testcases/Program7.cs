class Program
{
    static int ppc64leHelloWorldCallee()
    {
            return 100;
    }

    static int ppc64leHelloWorld()
    {
          int a = ppc64leHelloWorldCallee();
          return a;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Hello World -> After returning from singled named! Return Value = {0}", retVal);
    }
}
