class Program
{
    class Test
    {
        public int a;
    }
    static int ppc64leHelloWorld()
    {
        int a = 10;
        int b = 2;
        int c = a + b;
        return c;
    }

    static void Main(string[] args)
    {
        int c = ppc64leHelloWorld();
        Console.WriteLine("Answer C = {0}", c);
        Console.WriteLine("After jitting ppc64leHelloWorld....\n");
    }
}
