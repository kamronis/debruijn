using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeBreinOrtho
{
    public class Reed
    {
        private string line;
        private int portion;
        private StorageClient storage;
        public Reed(string line, int portion, StorageClient storage)
        {
            this.line = line;
            this.portion = portion;
            this.storage = storage;
        }

        private int nwords = 0;
        private int prevCode = -1, currCode;
        // Сканирует линию, выделяет слова, на основе слов формирует узлы и получает текущий коды узлов, коды используются для установления ссылок
        public void ProcessLine()
        {
            for (int nom = 0; nom < line.Length - portion + 1; nom++)
            {
                var word = DBNode.Combine(line.Substring(nom, portion));
                nwords++;
                // Теперь надо запросить узел
                currCode = storage.GetSetNode(word);
                // state1
                if (prevCode != -1)
                {
                    // state2
                    storage.SetNodeNext(prevCode, currCode);
                    // state3
                    storage.SetNodePrev(currCode, prevCode);
                }
                // state4
                prevCode = currCode;
            }
        }
        public static long totalbytessent = 0, totalbytesreceived = 0; 
        public static void ProcessLines(IEnumerable<Reed> reeds, StorageClient storage)
        {
            // === Отрогональный вариант с сетевым запросом
            Reed[] reed_arr = reeds.ToArray();

            bool tocontinue = true;
            while (tocontinue)
            {
                tocontinue = false;
                // == Подвергаем все риды шагу обработки
                // Шаг                    
                for (int i = 0; i < reed_arr.Length; i++)
                {
                    Reed re = reed_arr[i];
                    if (re.state != -1)
                    {
                        re.StepBeforeSend();
                        tocontinue = true;
                    }
                }
                // Есть наполовину выполненные шаги для элементов массива reed_arr. 
                // Надо сформировать поток команд к хранилищу, разбить поток по секциям, отправить, выполнить, принять,
                // раскидать результаты, выполнить итоговые действия
                SecondAndThirdPhase(reed_arr, storage);
            }

        }

        internal class CommToSend
        {
            internal StorageClient store;
            internal Command comm;
            internal object[] args;
            internal Action<object> resultAction;
        }
        internal static Func<CommToSend, object> SendReceive = cts =>
        {
            if (cts.comm == Command.getsetnode)
            {
                UInt64 word = (UInt64)cts.args[0];
                int code = cts.store.GetSetNode(word);
                return code;
            }
            else if (cts.comm == Command.setnodenext)
            {
                int c1 = (int)cts.args[0];
                int c2 = (int)cts.args[1];
                cts.store.SetNodeNext(c1, c2);
                return (byte)255; // Техническая посылка
            }
            else if (cts.comm == Command.setnodeprev)
            {
                int c1 = (int)cts.args[0];
                int c2 = (int)cts.args[1];
                cts.store.SetNodePrev(c1, c2);
                return (byte)255; // Техническая посылка
            }
            else throw new Exception("Err: 29283");
        };
        private static void SecondAndThirdPhase(Reed[] reed_arr, StorageClient storage)
        {
            var commands = reed_arr
                .Where(r => r.state != -1)
                .Select(r => new CommToSend()
                {
                    store = storage,
                    comm = r.command_tosend,
                    args = r.args_tosend,
                    resultAction = r.resultAction
                })
                .ToArray();
            object[] res_objects = commands
                .Select(cts => Reed.SendReceive(cts))
                .ToArray();
            //for (int i = 0; i < commands.Count(); i++)
            //{
            //    commands[i].resultAction(res_objects[i]);
            //}
            commands.Select((cts, i) => { cts.resultAction(res_objects[i]); return true; }).Count();

            //foreach (var co in commands)
            //{
            //    object ob = Reed.SendReceive(co);
            //    co.resultAction(ob);
            //}
        }

        private static void SecondAndThirdPhase0(Reed[] reed_arr, StorageClient storage)
        {
            // Преобразуем в поток байтов
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            int ncoms = reed_arr.Where(r => r.state != -1).Count();
            bw.Write((long)ncoms);
            for (int i = 0; i < reed_arr.Length; i++)
            {
                Reed re = reed_arr[i];
                if (re.state != -1)
                {
                    bw.Write((byte)(re.command_tosend));
                    if (re.command_tosend == Command.getsetnode) // GetSetNode
                    {
                        bw.Write((UInt64)(re.args_tosend[0]));
                    }
                    else if (re.command_tosend == Command.setnodenext) // SetNodeNext
                    {
                        bw.Write((int)re.args_tosend[0]);
                        bw.Write((int)re.args_tosend[1]);
                    }
                    else if (re.command_tosend == Command.setnodeprev)
                    {
                        bw.Write((int)re.args_tosend[0]);
                        bw.Write((int)re.args_tosend[1]);
                    }
                }
            }

            totalbytessent += ms.Length;

            // Преобразуем поток байтов в массив
            //Byte[] request = ms.ToArray();


            // Пошлем массив
            //Byte[] response = connection.SendReceive(request);
            // Примем массив на стороне хранилища
            // -- пока это response, предполагается, что массив дошел до сервера

            // Преобразуем массив в поток байтов
            // -- пока это ms

            // Прочитаем, выполним и запишем в выходной поток
            BinaryReader br = new BinaryReader(ms);
            ms.Position = 0L;
            MemoryStream rms = new MemoryStream();
            BinaryWriter rbw = new BinaryWriter(rms);
            long ncoms2 = br.ReadInt64();
            if (ncoms2 != ncoms) throw new Exception("Err: 29283");
            rbw.Write((long)ncoms2);
            for (int j = 0; j < ncoms2; j++)
            {
                byte co = br.ReadByte();
                Command comm2 = (Command)co;
                rbw.Write(co);
                //object arg1, arg2, res;
                if (comm2 == Command.getsetnode)
                {
                    UInt64 word = br.ReadUInt64();
                    int code = storage.GetSetNode(word);
                    rbw.Write(code);
                }
                else if (comm2 == Command.setnodenext)
                {
                    int c1 = br.ReadInt32();
                    int c2 = br.ReadInt32();
                    storage.SetNodeNext(c1, c2);
                    rbw.Write((byte)255); // Техническая посылка
                }
                else if (comm2 == Command.setnodeprev)
                {
                    int c1 = br.ReadInt32();
                    int c2 = br.ReadInt32();
                    storage.SetNodePrev(c1, c2);
                    rbw.Write((byte)255); // Техническая посылка
                }
                else throw new Exception("Err: 29283");
            }

            totalbytesreceived += rms.Length;

            // Преобразуем поток байтов rms в массив

            // Пошлем массив от хранилища мастеру

            // Примем массив на стороне мастера

            // Преобразуем массив в поток байтов rma
            // -- сейчас это rms
            BinaryReader rbr = new BinaryReader(rms);
            rms.Position = 0L;

            // Чтение результатов
            long ncoms3 = rbr.ReadInt64();
            for (int i = 0; i < reed_arr.Length; i++)
            {
                Reed re = reed_arr[i];
                if (re.state != -1)
                {
                    byte b3 = rbr.ReadByte();
                    Command comm3 = (Command)b3;
                    if (comm3 == Command.getsetnode)
                    {
                        int code = rbr.ReadInt32();
                        re.resultAction(code);
                    }
                    else if (comm3 == Command.setnodenext)
                    {
                        byte b = rbr.ReadByte();
                        re.resultAction(b);
                    }
                    else if (comm3 == Command.setnodeprev)
                    {
                        byte b = rbr.ReadByte();
                        re.resultAction(b);
                    }
                    else throw new Exception("29288");
                }
            }
        }

        public bool Finished() { return state == -1; }
        public void Steps()
        {
            while (state != -1) Step();
        }
        public void Step()
        {
            switch (state)
            {
                case 0:
                    {
                        //string word = line.Substring(nom_in_line, portion);
                        var word = DBNode.Combine(line.Substring(nom_in_line, portion));
                        nom_in_line++;
                        nwords++;
                        //storage.GetSetNodeOrtho(word, code =>
                        //{
                        //    currCode = code;
                        //    if (prevCode != -1) state = 2;
                        //    else
                        //    {
                        //        prevCode = currCode;
                        //        if (nom_in_line > line.Length - portion) { state = -1; }
                        //        else state = 0;
                        //    }

                        //});
                        storage.SendActionUseResult(Command.getsetnode, new object[] { word }, code =>
                        {
                            currCode = (int)code;
                            if (prevCode != -1) state = 2;
                            else
                            {
                                prevCode = currCode;
                                if (nom_in_line > line.Length - portion) { state = -1; }
                                else state = 0;
                            }

                        });

                        break;
                    }
                case 1:
                    { // Нет такого
                        throw new Exception("3939989");
                        //break;
                    }
                case 2:
                    {
                        //storage.SetNodeNextOrtho(prevCode, currCode, () => { state = 3; });
                        storage.SendActionUseResult(Command.setnodenext, new object[] { prevCode, currCode }, (_) =>
                        {
                            state = 3;
                        });
                        break;
                    }
                case 3:
                    {
                        //storage.SetNodePrevOrtho(currCode, prevCode, () => 
                        //{
                        //    prevCode = currCode;
                        //    if (nom_in_line > line.Length - portion) { state = -1; }
                        //    else { state = 0; }
                        //});
                        storage.SendActionUseResult(Command.setnodeprev, new object[] { currCode, prevCode }, (_) =>
                        {
                            prevCode = currCode;
                            if (nom_in_line > line.Length - portion) { state = -1; }
                            else { state = 0; }
                        });
                        break;
                    }
                case 4:
                    {
                        //prevCode = currCode;
                        //nom_in_line++;
                        //nwords++;
                        //if (nom_in_line > line.Length - portion) { state = -1; }
                        //else state = 0;
                        throw new Exception("2934244"); //break;
                    }
                default:
                    {
                        throw new Exception("Err in Reed Step()");
                    }
            }
        }

        // Состояние для задачи процессирования линии
        private int nom_in_line = 0;
        private int state = 0;

        // Состояние для посылки результата и постобрабтки
        Command command_tosend = Command.none;
        object[] args_tosend = null;
        Action<object> resultAction;

        /// <summary>
        /// Шаг действия, заканичающийся подготовкой команды для посылки серверу
        /// </summary>
        public void StepBeforeSend()
        {
            switch (state)
            {
                case 0:
                    {
                        var word = DBNode.Combine(line.Substring(nom_in_line, portion));
                        nom_in_line++;
                        command_tosend = Command.getsetnode;
                        args_tosend = new object[] { word };
                        resultAction = code =>
                        {
                            currCode = (int)code;
                            if (prevCode != -1) state = 2;
                            else
                            {
                                prevCode = currCode;
                                if (nom_in_line > line.Length - portion) { state = -1; }
                                else state = 0;
                            }
                        };

                        break;
                    }
                case 1:
                    { // Нет такого
                        throw new Exception("3939989");
                        //break;
                    }
                case 2:
                    {
                        command_tosend = Command.setnodenext;
                        args_tosend = new object[] { prevCode, currCode };
                        resultAction = (_) =>
                        {
                            state = 3;
                        };
                        break;
                    }
                case 3:
                    {
                        command_tosend = Command.setnodeprev;
                        args_tosend = new object[] { currCode, prevCode };
                        resultAction = (_) =>
                        {
                            prevCode = currCode;
                            if (nom_in_line > line.Length - portion) { state = -1; }
                            else { state = 0; }
                        };
                        break;
                    }
                default:
                    {
                        throw new Exception("Err in Reed Step()");
                    }
            }
        }

    }
}
