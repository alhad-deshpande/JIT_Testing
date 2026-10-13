using System;
using System.Runtime.CompilerServices;

public class Program
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void FillBuffer(Span<byte> buffer, int seed)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)((i * 17 + seed) & 255);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool CheckBuffer(Span<byte> buffer, int seed)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            byte expected = (byte)((i * 17 + seed) & 255);

            if (buffer[i] != expected)
            {
                return false;
            }
        }

        return true;
    }

    // Force a separate managed call with stack-passed arguments.
    [MethodImpl(MethodImplOptions.NoInlining)]
    static int Sum13(
        int a1, int a2, int a3, int a4,
        int a5, int a6, int a7, int a8,
        int a9, int a10, int a11, int a12,
        int a13)
    {
        return a1 + a2 + a3 + a4 +
               a5 + a6 + a7 + a8 +
               a9 + a10 + a11 + a12 +
               a13;
    }

    // Runtime-sized allocation forces the variable localloc path.
    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool TestSingleAllocation(int size)
    {
        Span<byte> buffer = stackalloc byte[size];

        FillBuffer(buffer, 0x5A);

        if (!CheckBuffer(buffer, 0x5A))
            return false;

        int sum = Sum13(
            1, 2, 3, 4, 5, 6, 7,
            8, 9, 10, 11, 12, 13);

        if (sum != 91)
            return false;

        return CheckBuffer(buffer, 0x5A);
    }

    // Verify multiple dynamic allocations within one JIT frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool TestMultipleAllocations(int pageSize)
    {
        int size1 = pageSize - 17;
        int size2 = pageSize + 31;
        int size3 = 2 * pageSize + 63;

        Span<byte> first = stackalloc byte[size1];
        FillBuffer(first, 0x11);

        Span<byte> second = stackalloc byte[size2];
        FillBuffer(second, 0x33);

        if (!CheckBuffer(first, 0x11))
            return false;

        Span<byte> third = stackalloc byte[size3];
        FillBuffer(third, 0x55);

        if (!CheckBuffer(first, 0x11) ||
            !CheckBuffer(second, 0x33) ||
            !CheckBuffer(third, 0x55))
        {
            return false;
        }

        int sum = Sum13(
            first[0], second[0], third[0],
            4, 5, 6, 7, 8, 9, 10, 11, 12, 13);

        int expected =
            0x11 + 0x33 + 0x55 +
            4 + 5 + 6 + 7 + 8 + 9 + 10 + 11 + 12 + 13;

        if (sum != expected)
            return false;

        return CheckBuffer(first, 0x11) &&
               CheckBuffer(second, 0x33) &&
               CheckBuffer(third, 0x55);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool TestManagedCall(int pageSize)
    {
        int size = pageSize + 33;

        Span<byte> buffer = stackalloc byte[size];

        FillBuffer(buffer, 0x77);

        int result = Sum13(
            1, 2, 3, 4, 5, 6, 7,
            8, 9, 10, 11, 12, 13);

        if (result != 91)
            return false;

        return CheckBuffer(buffer, 0x77);
    }

    // =========================================================
    // PPC64LE JIT HOOK
    // =========================================================

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool ppc64leHelloWorld(int pageSize)
    {
        // Keep a live stackalloc buffer in the JIT hook itself
        // while calling the other managed test methods.
        int anchorSize = pageSize - 1;

        Span<byte> anchor = stackalloc byte[anchorSize];
        FillBuffer(anchor, 0x29);

        int[] sizes =
        {
            1,
            31,
            pageSize - 257,
            pageSize - 1,
            pageSize,
            pageSize + 1,
            2 * pageSize - 1,
            2 * pageSize,
            2 * pageSize + 1
        };

        foreach (int size in sizes)
        {
            if (!TestSingleAllocation(size))
            {
                Console.WriteLine(
                    "FAIL: Single allocation size " + size);
                return false;
            }

            // Check that nested managed calls and stack
            // allocations do not corrupt the hook's frame.
            if (!CheckBuffer(anchor, 0x29))
            {
                Console.WriteLine(
                    "FAIL: JIT hook buffer corrupted");
                return false;
            }
        }

        Console.WriteLine(
            "PASS: Single allocations across page boundaries");

        if (!TestMultipleAllocations(pageSize))
        {
            Console.WriteLine(
                "FAIL: Multiple allocations corrupted buffers");
            return false;
        }

        Console.WriteLine(
            "PASS: Multiple allocations preserve earlier buffers");

        if (!TestManagedCall(pageSize))
        {
            Console.WriteLine(
                "FAIL: Managed call corrupted stack allocation");
            return false;
        }

        Console.WriteLine(
            "PASS: Managed calls with stack-passed arguments");

        if (!CheckBuffer(anchor, 0x29))
        {
            Console.WriteLine(
                "FAIL: JIT hook frame corrupted");
            return false;
        }

        Console.WriteLine("PASS: JIT hook frame preserved");

        return true;
    }

    public static int Main(string[] args)
    {
        Console.WriteLine(
            "PPC64LE localloc page-boundary regression test");

        try
        {
            int pageSize = Environment.SystemPageSize;

            if (pageSize < 512 ||
                pageSize > int.MaxValue / 3)
            {
                Console.WriteLine("FAIL: Unsupported page size");
                return 1;
            }

            bool result = ppc64leHelloWorld(pageSize);

            if (!result)
            {
                Console.WriteLine("FAIL");
                return 1;
            }

            Console.WriteLine("PASS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "FAIL: Unexpected exception: " +
                ex.GetType().Name);

            return 1;
        }
    }
}
