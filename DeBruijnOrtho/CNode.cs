using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijn
{
    public struct CNode
    {
        public UInt64 bword;
        public int prev;
        public int next;
    }
}
