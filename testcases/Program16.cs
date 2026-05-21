class Program
{
    static float ppc64leHelloWorld()
    {
        float fa = 1238.98645f;
        float fb = 1098.56441f;
        float c = fa - fb;
        return c;
    }

    static void Main(string[] args)
    {
        float c = ppc64leHelloWorld();
        Console.WriteLine("Answer C = {0}", c);
    }
}
