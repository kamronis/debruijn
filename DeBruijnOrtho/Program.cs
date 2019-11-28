using System;
using System.Collections.Generic;
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
            if (args.Length == 0)
            {
                //MainMaster(0);
                MainClient(new string[] { "client n2.bin" });
            }
            else
            {
                int nclients;
                if (Int32.TryParse(args[0], out nclients)) MainMaster(nclients);
                else if (args[0] == "client") MainClient(args);
                else throw new Exception("Error: wrong Main args");
            }
        }
        public static void MainMaster(int nclients)
        {
            Console.WriteLine($"Start MainMaster {nclients}");
            
            // Зафиксируем и вычислим некоторые параметры
            Options.nparts = nclients + 1;
            Options.nslaves = nclients;
            
            int mask = Options.nparts - 1;
            Options.nshift = 0;
            while (mask != 0) { mask >>= 1; Options.nshift++; }


            // Сначала создадим сетевую конфигурацию
            ServerConnection sc = null;
            if (nclients > 0)
            {
                sc = new ServerConnection(Options.host, Options.port);
                sc.Start(nclients); 
            }

            if (sc != null)
            {
                // Проверяем клиентов тем, что пишем нулевую команду
                foreach (var c in sc.clients) { c.BWriter.Write((byte)0); }
                //sc.clients[0].BWriter.Write((byte)0);
                Console.WriteLine($"Master sent command 0 to client {0}");
                var result = sc.clients[0].BReader.ReadByte();
                Console.WriteLine($"Master received value {result} from client {0}");
                //sc.clients[0].BWriter.Write((byte)255);
            }
            sw = new System.Diagnostics.Stopwatch();
            graph = new DeBruGraph(sc)
            {
            };

            //Main3(args); Main4(args); Main5(args); Main6(args);

            Main3();
            Main44();
            Main51();
            Main62();

            if (sc != null)
            {
                foreach (var c in sc.clients) { c.BWriter.Write((byte)255); }
                sc.Release();
            }
        }
        public static void MainClient(string[] args) 
        {
            Console.WriteLine($"Start MainClient for {Options.host}");
            string fname = Options.clientlistfilename;
            if (args.Length > 1) fname = args[1];
            NodesPart storage = new NodesPart(Options.clientlistfilename);
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
