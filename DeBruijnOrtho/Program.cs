using System;
using System.Collections.Generic;
using System.IO; 
using System.Text;

namespace DeBruijn
{
    partial class Program
    {
        private static System.Diagnostics.Stopwatch sw;
        public static DeBruGraph graph;
        /// <summary>
        /// Работает в режиме master и client, если аргументов нет, то запускается мастер без сети, если нулевой аргумент - число,
        /// то это мастер, число означает количество клиентов, которое должно быть запущено позже,
        /// клиенту нулевым аргументом запуска передается IP-адрес мастера или слово client 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Зафиксируем и вычислим некоторые параметры
            Options.nslaves = Options.nparts - 1;
            int mask = Options.nparts - 1;
            Options.nshift = 0;
            while (mask != 0) { mask >>= 1; Options.nshift++; }

            if (args.Length == 0)
            {
                MainMaster(args);
                //MainClient(new string[] { "client", @"D:\home\data\deBruijn\w2", @"D:\home\data\deBruijn\l2" });
            }
            else
            {
                if (args[0] == "master") MainMaster(args);
                else if (args[0] == "client") MainClient(args);
                else throw new Exception("Error: wrong Main args");
            }
        }
        public static void MainMaster(string[] args)
        {
            int nclients = Options.nparts - 1;
            Console.WriteLine($"Start MainMaster for {nclients} clients");

            // Сначала создадим сетевую конфигурацию
            ServerConnection sc = null;
            if (nclients > 0)
            {
                sc = new ServerConnection(Options.host, Options.port);
                sc.Start(nclients); 
            }

            if (sc != null)
            {
                //// Проверяем клиентов тем, что пишем нулевую команду -- здесь ОШИБКА
                //foreach (var c in sc.clients) { c.BWriter.Write((byte)0); }
                ////sc.clients[0].BWriter.Write((byte)0);
                //Console.WriteLine($"Master sent command 0 to client {0}");
                //var result = sc.clients[0].BReader.ReadByte();
                //Console.WriteLine($"Master received value {result} from client {0}");
                ////sc.clients[0].BWriter.Write((byte)255);
            }
            sw = new System.Diagnostics.Stopwatch();
            graph = new DeBruGraph(sc)
            {
            };

            DateTime tt0 = DateTime.Now;

            //Main31();
            if (File.Exists(Options.wnodesfilename)) File.Delete(Options.wnodesfilename);
            if (File.Exists(Options.lnodesfilename)) File.Delete(Options.lnodesfilename);
            if (File.Exists(Options.wnodesfilename_net)) File.Delete(Options.wnodesfilename_net);
            if (File.Exists(Options.lnodesfilename_net)) File.Delete(Options.lnodesfilename_net);
            Main45();
            Main51();
            Main62();

            DateTime tt1 = DateTime.Now;
            if (sc != null)
            {
                foreach (var c in sc.clients) { c.BWriter.Write((byte)255); }
                sc.Release();
            }

            Console.WriteLine($"Total time: {(tt1-tt0).TotalMilliseconds} ms.");
        }
        public static void MainClient(string[] args) 
        {
            Console.WriteLine($"Start MainClient for {Options.host}");
            string f1 = Options.wnodesfilename_net;
            string f2 = Options.lnodesfilename_net;
            if (args.Length > 2)
            {
                f1 = args[1]; Options.wnodesfilename_net = f1;
                f2 = args[2]; Options.lnodesfilename_net = f2;
            }

            NodesPart storage = new NodesPart(Options.wnodesfilename_net, Options.lnodesfilename_net);
            //storage.Init();
            ClientConnection connection = new ClientConnection(Options.host, Options.port, storage);
            while (true) 
            {
                bool ok = connection.ReceiveAndExecuteCommand();
                if (!ok) break;
            }
        }
    }
}
