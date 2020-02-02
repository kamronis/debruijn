using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBruijnDirect
{
    struct PrevNext
    {
        // Ссылки означают >= 0 - номер узла, -1 - не проставлялась, -2 - разрушена
        public Code prev, next;
    }

    struct Code
    {
        private long code;
        public Code(int sec, int nom)
        {
            int sm = DirectOptions.nsections - 1;
            int secshift = 0;
            while (sm != 0) { secshift++; sm = sm >> 1; }
            //Func<int, int, Code> combine = (s, nom) => ;
            code = (long)sec | ((long)nom << secshift);
        }
        public Code(BinaryReader br) { code = br.ReadInt64(); }
        public Code(long v) { code = v; }
        
        public long Value { get { return code; } }
        public void BinaryWrite(BinaryWriter bw) { bw.Write(code); }
        public bool Undefined { get { return code == -1; } }
        public bool Several { get { return code == -2; } }
        public int Sec { get { return (int)(code & (long)(DirectOptions.nsections - 1)); } }
        public int Nom 
        { 
            get 
            {
                int sm = DirectOptions.nsections - 1;
                int secshift = 0;
                while (sm != 0) { secshift++; sm = sm >> 1; }
                return (int)(code >> secshift); 
            } 
        }
    }

}
