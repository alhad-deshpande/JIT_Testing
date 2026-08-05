using System;

public class Program
{
	public enum Level
	{
		Low,      // 0
		Medium,   // 1
		High      // 2
	}

	public static Level ppc64leHelloWorld()
	{
		return Level.Medium;
	}

	public static int Main()
	{
		Level result  = ppc64leHelloWorld();
		Console.WriteLine(result);
		return (int)result == 1 ? 0 : 1;
	}
}
