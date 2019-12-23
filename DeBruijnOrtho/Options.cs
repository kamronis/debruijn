using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijn
{
    class Options
    {
        public static string masterfileplace = @"D:\Home\data\deBruijn\";
        //public static string masterfileplace = @"D:\PROJECTS\DeBrein\";
        public static string clientfileplace = @"D:\Home\data\deBruijn\";
        public static string readsfilename = masterfileplace + "reads.txt";
        //public static string readsfilename = masterfileplace + "Gen_reads.txt";
        //public static string readsfilename = masterfileplace + "50mil_reads.txt";
        public static string breadsfilename = masterfileplace + "breads.bin"; // Уже не нужен
        public static string bytereadsfilename = masterfileplace + "bytereads.bin";
        public static string creadsfilename = masterfileplace + "creads.bin";

        public static string wnodesfilename = masterfileplace + "wnodes.bin";
        public static string wnodesfilename_net = masterfileplace + "wnodes_net.bin";
        public static string lnodesfilename = masterfileplace + "lnodes.bin";
        public static string lnodesfilename_net = masterfileplace + "lnodes_net.bin";

        // Эта часть опций должны вычисляться. Сейчас надо их задать!

        public static int nsymbols = 20;
        public static int nparts = 1; // должна быть степень двойки
        public static int npasses = 1; // Число проходов при кодировании слов (Program44)
        public static int readslimit = Int32.MaxValue; // Пределньный пропуск количества ридов

        public static string host = "127.0.0.1"; // IP компьютера, на котором располагается мастер
        public static int port = 8788;

        public static int bwordsbuffer = 1000;
        public static int bufferSize = 2000; // Размер буферов на сетевых стримах
        public static int buffcnodeslength = 2000;

        public static bool toprintcontig = false;
        // Эти - вычисляются по nparts
        public static int nslaves;// nparts - 1; // Стандартном случае - на единицу меньше, чем число частей может и совпадать
        internal static int nshift; // смещение в коде для раздела частей и локальных номеров - число единичек в бинарном представлении nparts-1
    }
}
