using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int score = 0;
        
        // Test 1: NOT operation (~1 should be -2)
        if (~1 == -2) score += 1;
        
        // Test 2: AND with registers (15 & 7 should be 7)
        if ((15 & 7) == 7) score += 1;
        
        // Test 3: AND with immediate (255 & 15 should be 15)
        if ((255 & 15) == 15) score += 1;
        
        // Test 4: OR with registers (8 | 4 should be 12)
        if ((8 | 4) == 12) score += 1;
        
        // Test 5: OR with immediate (16 | 3 should be 19)
        if ((16 | 3) == 19) score += 1;
        
        // Test 6: XOR with registers (12 ^ 10 should be 6)
        if ((12 ^ 10) == 6) score += 1;
        
        // Test 7: XOR with immediate (255 ^ 15 should be 240)
        if ((255 ^ 15) == 240) score += 1;
        
        // Test 8: Combined (~1 & 7 should be 6)
        if ((~1 & 7) == 6) score += 1;
        
        // Test 9: Chained ((15 & 7) | (8 ^ 4) should be 15)
        if (((15 & 7) | (8 ^ 4)) == 15) score += 1;
        
        // Test 10: Nested (~(15 & 7) should be -8)
        if (~(15 & 7) == -8) score += 1;
        
        // Test 11: Multiple NOTs (~~5 should be 5)
        if (~~5 == 5) score += 1;
        
        // Test 12: AND with 0 (12345 & 0 should be 0)
        if ((12345 & 0) == 0) score += 1;
        
        // Test 13: OR with 0 (42 | 0 should be 42)
        if ((42 | 0) == 42) score += 1;
        
        // Test 14: XOR with 0 (99 ^ 0 should be 99)
        if ((99 ^ 0) == 99) score += 1;
        
        // Test 15: XOR with self (123 ^ 123 should be 0)
        if ((123 ^ 123) == 0) score += 1;
        
        // Test 16: AND with -1 (77 & -1 should be 77)
        if ((77 & -1) == 77) score += 1;
        
        // Test 17: OR with -1 (55 | -1 should be -1)
        if ((55 | -1) == -1) score += 1;
        
        // Test 18: Bit manipulation pattern
        int flags = 0;
        flags = flags | 1;      // Set bit 0: flags = 1
        flags = flags | 4;      // Set bit 2: flags = 5
        flags = flags & (~1);   // Clear bit 0: flags = 4
        flags = flags ^ 2;      // Toggle bit 1: flags = 6
        if (flags == 6) score += 1;
        
        return score;  // Returns 18 if all tests pass
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}
