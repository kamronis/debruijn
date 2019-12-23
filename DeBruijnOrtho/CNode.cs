using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijn
{
    public struct BWord
    {
        private const int nbytes = 32;
        //public BWord(UInt64 uword) { this.uword = uword; }
        private UInt64 uword;
        public BWord(byte[] bytes, int start, int number)
        {
            if (number > 32) throw new Exception("298923");
            uword = 0;
            for (int j = 0; j < number; j++)
            {
                // сдвигаем влево и делаем "или" с байтом
                uword = (uword << 2) | bytes[start + j];
            }
        }
        public UInt64 ToUInt64() { return uword; }
        public int Lay { get { return (int)((uword >> Options.nshift) & (ulong)(Options.npasses - 1)); } }
    }
    public struct WNode
    {
        public UInt64 bword;
    }
    public struct LNode
    {
        public int prev;
        public int next;
    }
}
