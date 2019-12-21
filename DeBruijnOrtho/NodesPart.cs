﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace DeBruijn
{
    public class NodesPart : INodePart
    {
        private Dictionary<UInt64, int> dic = new Dictionary<ulong, int>();

        private string wnodesfilename;
        private FileStream fsw;
        private BinaryReader brw;
        private BinaryWriter bww;
        int local_wnodesCount;
        //private List<WNode> local_wnodes = new List<WNode>();
        private WNode[] local_wnodes = null;

        private string lnodesfilename;
        private FileStream fsl;
        private BinaryReader brl;
        private BinaryWriter bwl;
        //private List<LNode> local_lnodes = new List<LNode>();
        private LNode[] local_lnodes = null;


        public NodesPart(string wnodesfilename, string lnodesfilename)
        {
            //this.nodepartfilename = nodepartfilename;
            this.wnodesfilename = wnodesfilename;
            this.lnodesfilename = lnodesfilename;
        }
        public void Init()
        { 
            fsw = File.Open(wnodesfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            brw = new BinaryReader(fsw);
            bww = new BinaryWriter(fsw);
            //local_wnodes = new List<WNode>();
            if (fsw.Length == 0)
            {
                bww.Write(0L);
                local_wnodesCount = 0;
            }
            else
            {
                fsw.Position = 0L;
                local_wnodesCount = (int)brw.ReadInt64();
            }

            fsl = File.Open(lnodesfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            brl = new BinaryReader(fsl);
            bwl = new BinaryWriter(fsl);
            //local_lnodes = new List<LNode>();
        }
        public int GetSetNode(UInt64 bword)
        {
            int code;
            if (!dic.TryGetValue(bword, out code))
            {
                code = local_wnodesCount;
                dic.Add(bword, code);
                local_wnodesCount++;
                //local_wnodes.Add(new WNode() { bword = bword }); // Заполняются только слова узлов
                bww.Write((UInt64)bword);
            }
            return code;
        }
        public IEnumerable<int> GetSetNodes(IEnumerable<UInt64> bwords)
        {
            return bwords.Select(bw => GetSetNode(bw));
        }
        public LNode GetLNodeLocal(int nom) { return local_lnodes[nom]; }
        
        //TODO: Уже не нужна
        public void MakePrototype()
        {
            bww.Seek(0, SeekOrigin.Begin);
            bww.Write((long)local_wnodesCount);
            //foreach (var node in local_wnodes)
            //{
            //    bww.Write(node.bword);
            //}
            bww.Flush();

            bwl.Seek(0, SeekOrigin.Begin);
            bwl.Write((long)local_wnodesCount);
            //foreach (var node in local_wnodes)
            for (int i = 0; i<local_wnodesCount; i++)
            {
                bwl.Write(-1); // поля, которые в дальнейшем будут заполняться. -1 - null
                bwl.Write(-1);
            }
            bwl.Flush();
        }
        public int Count() 
        {
            // Какой-то один из списков не должен быть пустым
            return local_lnodes != null ? local_lnodes.Length : local_wnodes.Length; 
        }
        public void Close() { fsw.Close(); fsl.Close(); }
        public void Save()
        {
            // Список слов не надо записывать - он не меняется
            bww.Seek(0, SeekOrigin.Begin);
            bww.Write((long)local_wnodesCount);
            bww.Flush(); fsw.Close();

            // Список ссылок надо записать, он мог измениться
            bwl.Seek(0, SeekOrigin.Begin);
            if (local_lnodes != null)
            {
                long lcount = local_lnodes.Length;
                bwl.Write(lcount);
                for (int i = 0; i < lcount; i++)
                {
                    var node = local_lnodes[i];
                    bwl.Write(node.prev);
                    bwl.Write(node.next);
                }
                fsl.Close();
            }
        }
        public void RestoreWNodes()
        {
            //local_wnodes = new List<WNode>();
            fsw.Position = 0L;
            long wcount = brw.ReadInt64();
            local_wnodes = new WNode[wcount];
            for (int i = 0; i < wcount; i++)
            {
                UInt64 bword = brw.ReadUInt64();
                local_wnodes[i].bword = bword;
            }
        }
        public void RestoreInitLNodes()
        {
            local_lnodes = new LNode[local_wnodesCount];
            for (int i = 0; i< local_wnodesCount; i++)
            {
                local_lnodes[i].prev = -1;
                local_lnodes[i].next = -1;
            }
        }
        public void RestoreDeactivateWNodes() { local_wnodes = null; } 
        public void RestoreLNodes()
        { 
            fsl.Position = 0L;
            long lcount = brl.ReadInt64();
            local_lnodes = new LNode[(int)lcount];
            for (int i = 0; i < lcount; i++)
            {
                int prev = brl.ReadInt32();
                int next = brl.ReadInt32();
                local_lnodes[i].prev = prev;
                local_lnodes[i].next = next;
            }
        }

        public void DropDictionary() { dic = new Dictionary<ulong, int>(); }

        public void SetNodePrev(int local, int prevlink)
        {
            int node_nom = local;
            var dnode = local_lnodes[node_nom];
            if (dnode.prev == -1) { dnode.prev = prevlink; }
            else if (dnode.prev == prevlink) { }
            else if (dnode.prev == -2) { }
            else { dnode.prev = -2; }
            local_lnodes[node_nom] = dnode;
        }
        public void SetNodeNext(int local, int nextlink)
        {
            int node_nom = local;
            var dnode = local_lnodes[node_nom];
            if (dnode.next == -1) { dnode.next = nextlink; }
            else if (dnode.next == nextlink) { }
            else if (dnode.next == -2) { }
            else { dnode.next = -2; }
            local_lnodes[node_nom] = dnode;
        }

        public IEnumerable<LNode> GetNodes(IEnumerable<int> codes)
        {
            return codes.Select(code =>
            {
                int node_nom = code >> Options.nshift;
                var dnode = local_lnodes[node_nom];
                return dnode;
            });
        }

        public IEnumerable<WNode> GetWNodes(IEnumerable<int> codes)
        {
            return codes.Select(code =>
            {
                int node_nom = code >> Options.nshift;
                var wnode = local_wnodes[node_nom];
                return wnode;
            });
        }
    }
}
