class Program
{
    static int ppc64leHelloWorld()
    {
          int a = 50;

          if (a == 3)
          {
             a = 30;
             goto RetVal;
          }
          else if (a == 5)
          {
             a = 50;
             goto RetVal;
          }
          else
          {
             a = 90;
             goto RetVal;
          }

RetVal:
          return a;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Hello World -> After returning from singled named! Return Value = {0}", retVal);
    }
}

