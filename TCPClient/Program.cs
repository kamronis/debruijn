using System;
using System.IO;

namespace TCP
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start TCP_bin_connection");
            string host = "127.0.0.1";
            int port = 8888;

            ClientConnection cconnection = new ClientConnection(host, port);

            // Посылаемый и принимаемый массивы
            byte[] data;
            byte[] res;
            Func<byte[], int> ToInt = (byte[] r) =>
            {
                BinaryReader reader = new BinaryReader(new MemoryStream(r));
                return reader.ReadInt32();
            };

            int test = 1;
            Console.WriteLine($"Test {test}");
            if (test == 0)
            {
                // Посылаем массив
                data = new byte[1024];
                res = cconnection.SendReceive(data);
                Console.WriteLine($"done {ToInt(res)}");

                // Посылаем пустой массив для завершения сеанса
                res = cconnection.SendReceive(new byte[0]);
                Console.WriteLine($"done {ToInt(res)}");
            }
            else if (test == 1)
            {

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                int nprobe = 10000;
                int ndata = 1_000;

                data = new byte[ndata];

                sw.Restart();
                for (int i=0; i<nprobe; i++)
                {
                    // Посылаем массив, принимаем результат
                    res = cconnection.SendReceive(data);
                    //Console.WriteLine($"done {ToInt(res)}");
                }
                sw.Stop();

                // Посылаем пустой массив для завершения сеанса
                res = cconnection.SendReceive(new byte[0]);
                Console.WriteLine($"done. arrlength {ndata}. {nprobe} probes. duration={sw.ElapsedMilliseconds} ms.");

                // localhost
                // 10000 раз 1_000 байт - 220 мс.
                // 10000 раз 10_000 байт - 330 мс.
                // 10000 раз 100_000 байт - 1400 мс.
                // 10000 раз 1000_000 байт - 15000 мс.
                // 

            }


        }
    }
}
