class Program
{
    static double ppc64leHelloWorld()
    {
        double aa = 108980.58946;
        double bb = 57.973;
        return aa * bb;
    }

    static void Main(string[] args)
    {
        double c = ppc64leHelloWorld();
        Console.WriteLine("Answer C = {0}", c);
    }
}

