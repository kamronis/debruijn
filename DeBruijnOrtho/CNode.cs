using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnNametable
{
    public struct CNode
    {
        public UInt64 bword;
        public int prev;
        public int next;
    }
}
