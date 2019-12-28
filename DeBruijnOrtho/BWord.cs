#define double

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace DeBruijn
{
    public struct BWord : IComparable
    {
        public const int nsymbols = 55;
        public const string biochars = "ACGT";
        internal UInt64 uword;
#if double
        internal UInt64 uword2; // Это можно коментарить
#endif
        public BWord(byte[] bytes, int start)
        {
            uword = 0;
            int ind = 0;
            for (; ind < 32; ind++)
            {
                // сдвигаем влево и делаем "или" с байтом
                uword = (uword << 2) | bytes[start + ind];
                if (ind >= nsymbols - 1) { ind++; break; }
            }
#if double
            // Этот раздел можно коментарить вместе со строчкой 
            uword2 = 0;
            if (ind == nsymbols) return;
            for (; ind < 64; ind++)
            {
                // сдвигаем влево и делаем "или" с байтом
                uword2 = (uword2 << 2) | bytes[start + ind];
                if (ind >= nsymbols - 1) { break; }
            }
            if (ind == nsymbols-1) return;
            throw new Exception();
#endif
        }
        public int Lay
        {
            get
            {
                return (int)((uword >> Options.nshift) & (ulong)(Options.npasses - 1));
            }
        }
        public int Part
        {
            get
            {
                return (int)(uword & (ulong)(Options.nparts - 1));
            }
        }
        public static BWord ReadBWord(BinaryReader reader)
        {
            var uw = reader.ReadUInt64();
            BWord bw = new BWord() { uword = uw };
#if double
            var uw1 = reader.ReadUInt64();
            bw.uword2 = uw1;
#endif
            return bw; 
        }
        public static void WriteBWord(BWord bword, BinaryWriter writer)
        {
            writer.Write(bword.uword);
#if double
            writer.Write(bword.uword2);
#endif
        }
        public override string ToString()
        {
            char[] chars = new char[BWord.nsymbols];
            UInt64 uwd = uword;
            int number = BWord.nsymbols;
            // Если к-во BWord.nsymbols <= 32, то одно решение
            if (number <= 32)
            {
                for (int i = 0; i < BWord.nsymbols; i++)
                {
                    chars[BWord.nsymbols - 1 - i] = biochars[(int)(uwd & 3)];
                    uwd = uwd >> 2;
                }
            }
            else
            {
                for (int i = 0; i < 32; i++) // 32 раза!
                {
                    chars[31 - i] = biochars[(int)(uwd & 3)];
                    uwd = uwd >> 2;
                }
#if double
                uwd = uword2;
                for (int i = 0; i < BWord.nsymbols - 32; i++)
                {
                    chars[BWord.nsymbols - 1 - i] = biochars[(int)(uwd & 3)];
                    uwd = uwd >> 2;
                }
#endif
            }
            return new string(chars);
        }
        // Эти переопредления можно убрать без потери производительности
        public override int GetHashCode()
        {
            return uword.GetHashCode()
#if double
                ^ uword2.GetHashCode()
#endif
                ;
        }
        public override bool Equals(object obj)
        {
            return uword.Equals(((BWord)obj).uword)
#if double
                && uword2.Equals(((BWord)obj).uword2)
#endif
                ;
        }

        int HashFAQ6(byte[] str)
        {
            int hash = 0;
            for (int i = 0; i < str.Length; i++)
            {
                int c = str[i];
                hash += c;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }

        public int CompareTo(object obj)
        {
            int cmp = uword.CompareTo(((BWord)obj).uword);
#if double
            if (cmp == 0) cmp = uword2.CompareTo(((BWord)obj).uword2);
#endif
            return cmp;
        }
    }
    public struct LNode
    {
        public NCode prev;
        public NCode next;
    }
}
