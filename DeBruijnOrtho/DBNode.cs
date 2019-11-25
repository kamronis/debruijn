using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnNametable
{
    public class DBNode
    {
        internal int code;
        internal int prev = -1;
        internal int next = -1;
        // расшифровка: -1 - не устанавливалась, -2 - устанавливалась более одного раза
        internal UInt64 word32x2;
        public DBNode(int code, UInt64 word)
        {
            this.code = code;
            this.word32x2 = word;
        }
        public static UInt64 Combine(string sword)
        {
            UInt64 w = 0;
            for (int i = 0; i < sword.Length; i++)
            {
                char c = sword[i];
                UInt64 bits = 0;
                if (c == 'A') bits = 0;
                else if (c == 'C') bits = 1;
                else if (c == 'G') bits = 2;
                else bits = 3; // (c == 'T') и другие варианты
                w = (w << 2) | bits;
            }
            return w;
        }
        private static char[] symbols = new char[] { 'A', 'C', 'G', 'T' };
        public static string UnCombine(UInt64 word, int len)
        {
            char[] char_arr = new char[len];
            UInt64 w = word;
            for (int i = 0; i < len; i++)
            {
                char_arr[len - i - 1] = symbols[w & 3];
                w >>= 2;
            }
            return new string(char_arr);
        }
    }
}
