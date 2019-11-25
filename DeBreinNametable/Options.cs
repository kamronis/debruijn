using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnNametable
{
    class Options
    {
        public static string readsfilename = @"C:\Home\data\deBrein\reads.txt";
        public static string breadsfilename = @"C:\Home\data\deBrein\breads.bin";
        public static string creadsfilename = @"C:\Home\data\deBrein\creads.bin";
        public static string nodelistfilename = @"C:\Home\data\deBrein\nlist.bin";
        public static string nodelist_net1 = @"C:\Home\data\deBrein\nlist_net1.bin";
        public static int nsymbols = 20;
        public static string[] partvars = new string[] { "s" };
        public static int nparts = 1;
        public static int nshift = 0; // смещение в коде для раздела частей и локальных номеров
    }
}
