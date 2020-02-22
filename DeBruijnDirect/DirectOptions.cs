using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnDirect
{
    class DirectOptions
    {
        //public static string readsfilename = @"D:\Home\data\deBruijn\reads.txt";
        public static string readsfilename = @"D:\Home\data\deBruijn\Gen_reads.txt";
        public static string bytereadsfilename = @"D:\Home\data\deBruijn\bytereads.txt";
        public static string compressedreadsfilename = @"D:\Home\data\deBruijn\G_reads.bin";
        //public static string compressedreadsfilename = @"D:\Home\data\deBruijn\bytereads_compressed.bin";
        public static string workdir = @"D:\Home\data\deBruijn\";

        public static int nsymbols = 31;
        public static int npasses = 1;
        public static int nsections = 4;

    }
}
