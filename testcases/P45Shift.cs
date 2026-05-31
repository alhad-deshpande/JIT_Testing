using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int score = 0;
        
        // Test 1: Left shift by constant (8 << 2 = 32)
        if ((8 << 2) == 32) score += 1;
        
        // Test 2: Right shift by constant (32 >> 2 = 8)
        if ((32 >> 2) == 8) score += 1;
        
        // Test 3: Left shift 64-bit (16L << 3 = 128L)
        if ((16L << 3) == 128L) score += 1;
        
        // Test 4: Arithmetic right shift preserves sign (-16 >> 2 = -4)
        if ((-16 >> 2) == -4) score += 1;
        
        // Test 5: Shift by variable
        int shiftAmount = 4;
        if ((256 >> shiftAmount) == 16) score += 1;
        
        // Test 6: Left shift by variable
        if ((1 << shiftAmount) == 16) score += 1;
        
        // Test 7: Shift by 0 (identity)
        if ((42 << 0) == 42) score += 1;
        
        // Test 8: Combine with other operations
        if (((8 << 2) + (32 >> 2)) == 40) score += 1;
        
        // Test 9: Multiply by power of 2 (7 << 3 = 56)
        if ((7 << 3) == 56) score += 1;
        
        // Test 10: Divide by power of 2 (64 >> 3 = 8)
        if ((64 >> 3) == 8) score += 1;
        
        return score;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

