using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeBreinOrtho;

namespace DeBruijnNametable
{
    partial class Program
    {
        public static void Main5(string[] args)
        {
            Console.WriteLine("Start Main5");
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Restart();
            Stream creadstream = File.Open(Options.creadsfilename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(creadstream);
            Stream nodeliststream = File.Open(Options.nodelistfilename, FileMode.Open, FileAccess.ReadWrite);
            BinaryReader brnl = new BinaryReader(nodeliststream);
            BinaryWriter bwnl = new BinaryWriter(nodeliststream);

            // Восстанавливаем список
            List<CNode> nodes = new List<CNode>();
            long nnodes = brnl.ReadInt64();
            for (int i=0; i<nnodes; i++)
            {
                UInt64 bword = brnl.ReadUInt64();
                int prev = brnl.ReadInt32();
                int next = brnl.ReadInt32();
                nodes.Add(new CNode() { bword = bword, prev = prev, next = next });
            }

            // Обработаем риды, сформируем граф
            sw.Restart();
            long nreeds = br.ReadInt64();
            for (long ind = 0; ind < nreeds; ind++)
            {
                // читаем длину бинарного рида
                int nwords = (int)br.ReadInt64();
                int codeprev = -1;
                for (int nom = 0; nom < nwords; nom++)
                {
                    int code = br.ReadInt32();
                    if (nom != 0) // точно есть текущий и предыдущий узлы
                    {
                        SetNodePrev(code, codeprev, nodes);
                        SetNodeNext(codeprev, code, nodes);
                    }
                    codeprev = code;
                }
            }

            sw.Stop();
            Console.WriteLine($"Create Graph ok. duration: {sw.ElapsedMilliseconds}");

            nodeliststream.Position = 0L;
            bwnl.Write(nnodes);
            for (int i = 0; i < nnodes; i++)
            {
                var node = nodes[i];
                bwnl.Write(node.bword);
                bwnl.Write(node.prev);
                bwnl.Write(node.next);
            }
            creadstream.Close();
            nodeliststream.Close();

            //ExtractChains(nodes, nsymbols);
        }

        public static void SetNodePrev(int node, int prevlink, List<CNode> list)
        {
            var dnode = list[node];
            if (dnode.prev == -1) { dnode.prev = prevlink; }
            else if (dnode.prev == prevlink) { }
            else if (dnode.prev == -2) { }
            else { dnode.prev = -2; }
            list[node] = dnode;
        }
        public static void SetNodeNext(int node, int nextlink, List<CNode> list)
        {
            var dnode = list[node];
            if (dnode.next == -1) { dnode.next = nextlink; }
            else if (dnode.next == nextlink) { }
            else if (dnode.next == -2) { }
            else { dnode.next = -2; }
            list[node] = dnode;
        }


    }
}
