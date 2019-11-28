using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijn
{
    class Options
    {
        //public static string masterfileplace = @"D:\Home\data\deBrein\";
        //public static string clientfileplace = @"D:\Home\data\deBrein\";
        public static string masterfileplace = @"C:\data\deBruijn\";
        public static string clientfileplace = @"C:\data\deBruijn\";
        public static string readsfilename = masterfileplace + "reads.txt";
        public static string breadsfilename = masterfileplace + "breads.bin";
        public static string creadsfilename = masterfileplace + "creads.bin";
        public static string masterlistfilename = masterfileplace + "nlist.bin";
        public static string clientlistfilename = clientfileplace + "nlist_net.bin";
        public static int nsymbols = 20;

        public static string host = "127.0.0.1"; // IP компьютера, на котором располагается мастер
        public static int port = 8788;

        public static int npasses = 4; // Число проходов при кодировании слов (Program44)
        public static int bwordsbuffer = 1000;
        public static int bufferSize = 2000; // Размер буферов на сетевых стримах
        public static int buffcnodeslength = 2000;

        // Эта часть опций обычно вычисляется. Не задавайти их, если не понимаете!
        public static int nparts = 2; // должна быть степень двойки
        public static int nslaves = 1;// nparts - 1; // Стандартном случае - на единицу меньше, чем число частей может и совпадать
        internal static int nshift = 1; // смещение в коде для раздела частей и локальных номеров - число единичек в бинарном представлении nparts-1
    }
}
