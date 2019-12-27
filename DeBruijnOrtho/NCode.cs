#define n64

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBruijn
{
#if n64
#else
#endif
    public struct NCode
    {
#if n64
        private long code;
#else
        private int code;
#endif
        public NCode(int numb) { code = numb; }
        public NCode(long numb)
        {
#if n64
            code = numb;
#else
            code = (int)numb;
#endif
        }
        public static void Write(NCode ncode, BinaryWriter bw) { bw.Write(ncode.code); }
        public static NCode Read(BinaryReader br)
        {
#if n64
            var v = br.ReadInt64();
#else
            var v = br.ReadInt32();
#endif
            return new NCode(v);
        }
        public int Part
        {
            get
            {
                return (int)(code & (Options.nparts - 1));
            }
        }
        public int Local
        {
            get
            {
                return (int)(code >> Options.nshift);
            }
        }
        public static NCode Construct(int part, int local)
        {
            NCode code = new NCode((local << Options.nshift) | part);
            return code;
        }
        public static NCode none = new NCode(-1);
        public static NCode many = new NCode(-2);
        public bool Eq(NCode nc)
        {
            return code == nc.code;
        }
        public bool IsSingle() { return code >= 0; }
    }
}
