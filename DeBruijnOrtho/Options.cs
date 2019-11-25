using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnNametable
{
    class Options
    {
        public static string masterfileplace = @"D:\Home\data\deBrein\";
        public static string clientfileplace = @"D:\Home\data\deBrein\";
        public static string readsfilename = masterfileplace + "reads.txt";
        public static string breadsfilename = masterfileplace + "breads.bin";
        public static string creadsfilename = masterfileplace + "creads.bin";
        public static string masterlistfilename = masterfileplace + "nlist.bin";
        public static string clientlistfilename = clientfileplace + "nlist_net1.bin";
        public static int nsymbols = 20;
        public static int nparts = 1; // должна быть степень двойки
        public static int nslaves = 1;// nparts - 1; // Стандартном случае - на единицу меньше, чем число частей может и совпадать
        internal static int nshift = 0; // смещение в коде для раздела частей и локальных номеров - число 1 в бинарном представлении nparts-1
    }
}
