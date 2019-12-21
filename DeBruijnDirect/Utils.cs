using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnDirect
{
    struct PrevNext
    {
        // Ссылки означают >= 0 - номер узла, -1 - не проставлялась, -2 - разрушена
        public int prev, next;
    }
}
