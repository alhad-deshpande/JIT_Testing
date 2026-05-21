class Program
{
    static double ppc64leHelloWorld()
    {
        double aa = 1238.98645;
        double bb = 2.2;
        double c = aa % bb;
        return c;
    }

    static void Main(string[] args)
    {
        double c = ppc64leHelloWorld();
        Console.WriteLine("Answer C = {0}", c);
    }
}

