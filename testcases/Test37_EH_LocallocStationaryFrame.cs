using System;
using System.Runtime.CompilerServices;

class Program
{
    static int expectedMode;
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void ThrowException()
    {
        throw new InvalidOperationException("Localloc unwind test");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void ppc64leHelloWorld(int mode)
    {
        int size = 64;

        if (mode == 0)
            ThrowException();

        Span<byte> first = stackalloc byte[size];
        first[0] = 0x11;
        first[size - 1] = 0x33;

        if (mode == 1)
            ThrowException();

        Span<byte> second = stackalloc byte[size * 2];
        second[0] = 0x22;
        second[second.Length - 1] = 0x44;

        if (first[0] != 0x11 ||
            first[size - 1] != 0x33 ||
            second[0] != 0x22 ||
            second[second.Length - 1] != 0x44)
        {
            throw new Exception("Stack corruption");
        }

        if (mode == 2)
            ThrowException();
    }
    static int Main()
{
    Console.WriteLine("PPC64LE localloc regression test");

    int caught = 0;

    for (int mode = 0; mode < 3; mode++)
    {
	expectedMode = mode;

        Console.WriteLine($"Running mode");

        try
        {
            ppc64leHelloWorld(mode);

            Console.WriteLine($"FAIL: Mode {mode} returned without throwing");
            return 1;
        }
        catch (InvalidOperationException)
        {
            if (mode != expectedMode)
        {
            Console.WriteLine("FAIL: Mode corrupted after unwind");

            Console.Write("Expected: ");
            Console.WriteLine(expectedMode);

            Console.Write("Actual: ");
            Console.WriteLine(mode);

            return 2;
        }
            caught++;
            Console.WriteLine($"PASS: Exception caught");
        }
    }

    Console.WriteLine("Running mode 3: normal return");

    try
    {
        ppc64leHelloWorld(3);
        Console.WriteLine("PASS: Normal return");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FAIL: Mode 3 threw {ex.GetType().Name}: {ex.Message}");
        return 1;
    }

    Console.WriteLine($"Caught: {caught}/3");

    if (caught != 3)
    {
        Console.WriteLine("FAIL: Unexpected catch count");
        return 1;
    }

    Console.WriteLine("PASS");
    return 0;
}
}
