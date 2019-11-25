using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace DeBreinData
{
    class Program
    {
        public static int nportions = 0, maxarr = 0;
        static void Main(string[] args)
        {
            Console.WriteLine("Start DeBreinData!");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Hello de Brein!");

            //string readsfilename = @"D:\PROJECTS\graphbio\reads.txt";
            string readsfilename = @"D:\Home\data\deBrein\reads.txt";
            int nsymbols = 20;
            //string readsfilename = @"D:\PROJECTS\graphbio\DeBrein\DeBrein\reads7x3.txt";
            //int nsymbols = 3;

            //ClientConnection connection = new ClientConnection("192.168.168.82", 13000);
            ClientConnection connection = new ClientConnection("127.0.0.1", 13000);

            // Буфер для процессирования
            BufferredProcessing<Reed> pbuffer = new BufferredProcessing<Reed>(1000, reeds =>
            {
                Reed[] reed_arr = reeds.ToArray();
                bool[] toask = new bool[reed_arr.Length];
                bool tocontinue = true;
               // int iter = 0;
                while (tocontinue)
                {
                    tocontinue = false;
                    //iter++;
                   // Console.WriteLine(iter);
                    // Подвергаем все риды шагу обработки, формируем маску ридов, по которым надо производить запрос
                    for (int i=0; i<reed_arr.Length; i++)
                    {
                        Reed re = reed_arr[i];
                        if (re.ToProcess)
                        {
                            re.Step();
                            toask[i] = true;
                            tocontinue = true;
                        }
                        else toask[i] = false;
                    }
                    // Преобразуем в поток байтов
                    MemoryStream ms = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(ms);
                    int ncoms = toask.Where(b => b).Count();
                    bw.Write((long)ncoms);
                    for (int i = 0; i < reed_arr.Length; i++)
                    {
                        Reed re = reed_arr[i];
                        if (toask[i])
                        {
                            bw.Write((byte)re.command);
                            if (re.command == 0) // Пустая команда
                            {
                            }
                            else if (re.command == 1) // GetNodeId
                            {
                                bw.Write((string)re.args[0]);
                            }
                            else if (re.command == 2) // SetNodeNext
                            {
                                bw.Write((int)re.args[0]);
                                bw.Write((int)re.args[1]);
                            }
                            else if (re.command == 3)
                            {
                                bw.Write((int)re.args[0]);
                                bw.Write((int)re.args[1]);
                            }
                        }
                    }

                    // Преобразуем поток байтов в массив
                    Byte[] request = ms.ToArray();

                    // Пошлем массив
                    Byte[] response = connection.SendReceive(request);
                    // Примем массив на стороне хранилища


                    // Преобразуем массив в поток байтов
                  

                    // Преобразуем поток байтов rms в массив

                    // Пошлем массив от хранилища мастеру

                    // Примем массив на стороне мастера

                    // Преобразуем массив в поток байтов rma
                    MemoryStream rms = new MemoryStream(response);

                    //System.Threading.Thread.Sleep(1);
                    nportions++;
                    maxarr = Math.Max(maxarr, (int)ms.Length);

                    
                    // снабдим ридером поток rma, будем читать и выполнять заключительыне действия
                    BinaryReader rbr = new BinaryReader(rms);
                    rms.Position = 0L;
                    for (int i = 0; i < reed_arr.Length; i++)
                    {
                        Reed re = reed_arr[i];
                        if (toask[i])
                        {
                            if (re.command == 0)
                            {
                                rbr.ReadByte();
                            }
                            else if (re.command == 1)
                            {
                                object got = rbr.ReadInt32();
                                re.Useresult(got);
                            }
                            else if (re.command == 2)
                            {
                                object got = rbr.ReadByte();
                                re.Useresult(true); // пришедшее несущественно
                            }
                            else if (re.command == 3)
                            {
                                object got = rbr.ReadByte();
                                re.Useresult(true); // пришедшее несущественно
                            }
                        }
                    }
                }
            });

            sw.Start(); // запускаем секундомер
            // Сканируем данные, вычисляем узлы
            using (TextReader reader = new StreamReader(File.Open(readsfilename, FileMode.Open, FileAccess.Read)))
            {
                int lcount = 0;
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    Reed re = new Reed(line, nsymbols);
                    
                    //while (re.ToProcess) { re.Process(); } // результаты складываются в хранилище storage
                    pbuffer.Add(re);
                    lcount++;
                    if (lcount == 10)
                    {
                        break;
                    }
                }
                pbuffer.Flush();
                Byte[] request = BitConverter.GetBytes(3L); //посылаем 3 команды
                Byte[] commands = { 4, 5, 6 };  // команды 4, 5, 6 - count, plinks nlinks
                request = request.Concat(commands).ToArray();
                Byte[] response = connection.SendReceive(request);
                MemoryStream ms = new MemoryStream(response);
                BinaryReader br = new BinaryReader(ms);
                int count = br.ReadInt32();
                int plinks = br.ReadInt32();
                int nlinks = br.ReadInt32();
                Console.WriteLine($"lines:{lcount} words: {Reed.nwords} nodes: {count} prev: {plinks} next: {nlinks}");
                Console.WriteLine($"number of portions={nportions} maxarr={maxarr}");
            }
            sw.Stop(); Console.WriteLine($"Build nodes ok. Duration={sw.ElapsedMilliseconds}");

        }


    }
}
