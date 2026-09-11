using System;
using System.Runtime.CompilerServices;

class Program
{
    static volatile bool CatchEntered;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Baz()
    {
        throw new InvalidOperationException("PPC64LE EH test");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Bar()
    {
        Baz();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool Foo()
    {
        bool result = false;

        try
        {
            Bar();
        }
        catch (ArgumentException)
        {
            // Must not select this handler.
            result = false;
        }
        catch (InvalidOperationException ex)
        {
            CatchEntered = true;

            if (ex.Message == "PPC64LE EH test")
            {
                result = true;
            }
        }

        return result;
    }
    public static int Main()
	{
	    CatchEntered = false;
	    bool result = Foo();

	    if (!CatchEntered)
	    {
		Console.WriteLine("FAIL: catch was not entered");
		return 1;
	    }

	    if (!result)
	    {
		Console.WriteLine("FAIL: exception object, handler selection, unwind, or parent local update failed");
		return 1;
	    }

	    Console.WriteLine("PASS");
	    return 0;
	}
}
