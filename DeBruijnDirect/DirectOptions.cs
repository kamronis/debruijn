using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnDirect
{
    class DirectOptions
    {
        public static string readsfilename = @"D:\PROJECTS\DeBrein\500k_1_reads.txt";
        //public static string readsfilename = @"C:\data\deBruijn\Gen_reads.txt";
        public static string bytereadsfilename = @"D:\PROJECTS\DeBrein\bytereads.bin";
        public static string workdir = @"D:\PROJECTS\DeBrein\";

        public static int nsymbols = 20;
        public static int npasses = 1;
        public static int nsections = 1;

    }
}
