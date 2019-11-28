using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace DeBruijn
{
    partial class Program
    {
        public static void Main44()
        {
            Console.WriteLine("Start DeBruijnNametable Main44");

            string tmp0 = Options.masterfileplace + "tmp0.bin";
            string tmp1 = Options.masterfileplace + "tmp1.bin";

            sw.Restart();

            // Файл и поток бинарных ридов
            Stream breadstream = File.Open(Options.breadsfilename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(breadstream);

            // Два альтернирующих файла, по очереди исполняющих роль входного и выходного потоков для кодированных ридов 
            FileStream fs0 = File.Open(tmp0, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream fs1 = File.Open(tmp1, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream[] files = new FileStream[] { fs0, fs1 };
            FileStream filein, fileout;

            graph.InitParts();

            // Создание cread
            sw.Restart();
            // Маска разрядов бинарного слова
            UInt64 mask = (UInt64)(Options.nparts - 1);


            // Кодирование узлов будем производить в несколько проходов (слоев). Выделим несколько битов в bword и на каждом 
            // проходе будем сравнивать с номером прохода (слоя)
            int lay;
            for (lay = 0; lay < Options.npasses; lay++)
            {
                Console.Write($"pass {lay} ");
                // Перемотаем на начало бинарный рид 
                breadstream.Position = 0L;
                // инициализируем входной и выходной стримы
                filein = files[lay & 1]; fileout = files[(lay + 1) & 1];
                BinaryReader binr = new BinaryReader(filein);
                BinaryWriter binw = new BinaryWriter(fileout);
                filein.Position = 0L; fileout.Position = 0L;

                ActionBuffer group = new ActionBuffer(Options.bwordsbuffer, objs =>
                {
                    object[] elements = objs.ToArray();

                    // Надо собрать массовую команду обращения к хранилищу, т.е. собрать массив слов bword
                    UInt64[] bwords = elements
                    .Cast<object[]>()
                    .Where(pars => (int)pars[0] == 1)
                    .Select(pars => (UInt64)pars[1])
                    .ToArray();

                    // Обратиться к хранилищу
                    //int[] codes = bwords.Select(bword => graph.GetSetNode(bword)).ToArray();
                    int[] codes = graph.GetSetNodes(bwords).ToArray();

                    // Выполнить итоговые действия, в том числе, с использованием полученного массива кодов
                    int icode = 0;
                    foreach (object[] pars in elements)
                    {
                        int comm = (int)pars[0];
                        switch (comm)
                        {
                            case 1:
                                {
                                    //UInt64 _bword = (UInt64)pars[1];
                                    //int _code = graph.GetSetNode(_bword);
                                    int _code = codes[icode]; icode++;
                                    binw.Write(_code);
                                    break;
                                }
                            case 2:
                                {
                                    int _code = (int)pars[1];
                                    binw.Write(_code);
                                    break;
                                }
                            case 3:
                                {
                                    long _nwords = (long)pars[1];
                                    binw.Write(_nwords);
                                    break;
                                }
                            default: throw new Exception("siujwi");
                        }
                    }
                });

                // Теперь читаем, читаем, пишем, при первом слое входной стрим не читаем
                long nreeds = br.ReadInt64();
                if (lay > 0) binr.ReadInt64();
                binw.Write(nreeds);


                for (long ind = 0; ind < nreeds; ind++)
                {
                    if (ind % 10_000 == 0) Console.Write($"{ind / 10_000} ");
                    // читаем и пишем длину бинарного рида
                    int nwords = (int)br.ReadInt64();
                    if (lay > 0) binr.ReadInt64();
                    //binw.Write((long)nwords);
                    group.Add(new object[] { 3, (long)nwords });


                    for (int nom = 0; nom < nwords; nom++)
                    {
                        // Читаем, читаем, пишем
                        UInt64 bword = br.ReadUInt64();
                        int code = -4;
                        if (lay > 0) code = binr.ReadInt32();
                        if (((bword >> Options.nshift) & (ulong)(Options.npasses - 1)) == (ulong)lay)
                        {
                            //code = graph.GetSetNode(bword);
                            //binw.Write(code);
                            group.Add(new object[] { 1, bword });
                        }
                        else
                        {
                            //binw.Write(code);
                            group.Add(new object[] { 2, code });
                        }

                    }
                }
                group.Flush();
                // Пошлем команду на освобождение словаря
                graph.DropDictionary();
                GC.Collect();
                Console.WriteLine();
            }
            fs0.Close(); fs1.Close();
            string lastfname = (lay & 1) == 0 ? tmp0 : tmp1;
            if (File.Exists(Options.creadsfilename)) File.Delete(Options.creadsfilename);
            File.Move(lastfname, Options.creadsfilename);
            if (File.Exists(tmp0)) File.Delete(tmp0);
            if (File.Exists(tmp1)) File.Delete(tmp1);

            Console.WriteLine($"Memory used: {GC.GetTotalMemory(false)}");
            graph.MakePrototype();

            graph.Close();

            sw.Stop();

            //Console.WriteLine($"nodes.Count: {list.Count} Memory used {GC.GetTotalMemory(false)}");
            Console.WriteLine($"Create coded binary reeds file ok. duration: {sw.ElapsedMilliseconds}");
            breadstream.Close();
            //creadstream.Close();

        }
    }
}
