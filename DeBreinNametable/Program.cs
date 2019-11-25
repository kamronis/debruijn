using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnNametable
{
    partial class Program
    {
        private static System.Diagnostics.Stopwatch sw;
        public static DeBruGraph graph;
        /// <summary>
        /// Работает в режиме master и client, если аргументов нет, то запускается мастер без сети, если нулевой аргумент - число,
        /// то это мастер, число означает количество клиентов, которое должно быть запущено позже,
        /// клиенту нулевым аргументом запуска передается IP-адрес мастера 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MainMaster(1);
                //MainClient(new string[] { "127.0.0.1" });
            }
            else
            {
                int nclients;
                if (Int32.TryParse(args[0], out nclients)) MainMaster(nclients);
                else MainClient(args);
            }
        }
        static string host = "127.0.0.1";
        static int port = 8788;
        public static void MainMaster(int nclients)
        {
            Console.WriteLine($"Start MainMaster {nclients}");
            // Сначала создадим сетевую конфигурацию
            ServerConnection sc = null;
            if (nclients > 0)
            {
                sc = new ServerConnection(host, port);
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
                //sc.clients[0].BWriter.Write((byte)255);
            }
        }
        public static void MainClient(string[] args) 
        {
            Console.WriteLine($"Start MainClient {args[0]}");
            NodesPart storage = new NodesPart(Options.nodelist_net1);
            //storage.Init();
            ClientConnection connection = new ClientConnection(host, port, storage);
            while (true) 
            {
                bool ok = connection.ReceiveAndExecuteCommand();
                if (!ok) break;
            }
        }
    }
}
