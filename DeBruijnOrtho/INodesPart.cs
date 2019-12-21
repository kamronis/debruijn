using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace DeBruijn
{
    public interface INodePart
    {
        void Init();
        void RestoreWNodes();
        void RestoreInitLNodes();
        void RestoreDeactivateWNodes();
        void RestoreLNodes();
        int Count();
        void Save();
        void Close();
        void DropDictionary();
        LNode GetLNodeLocal(int nom);

        int GetSetNode(UInt64 bword);
        IEnumerable<int> GetSetNodes(IEnumerable<UInt64> bwords);
        void MakePrototype();

        void SetNodePrev(int local, int prevlink);
        void SetNodeNext(int local, int nextlink);

        IEnumerable<LNode> GetNodes(IEnumerable<int> codes);
        IEnumerable<WNode> GetWNodes(IEnumerable<int> codes);
    }

    // Предыдущий вариант
    public class NodesPart1 : INodePart
    {
        private Dictionary<UInt64, int> dic = new Dictionary<ulong, int>();

        private string wnodesfilename;
        private FileStream fsw;
        private BinaryReader brw;
        private BinaryWriter bww;
        private List<WNode> local_wnodes = new List<WNode>();

        private string lnodesfilename;
        private FileStream fsl;
        private BinaryReader brl;
        private BinaryWriter bwl;
        private List<LNode> local_lnodes = new List<LNode>();


        public NodesPart1(string wnodesfilename, string lnodesfilename)
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
            local_wnodes = new List<WNode>();

            fsl = File.Open(lnodesfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            brl = new BinaryReader(fsl);
            bwl = new BinaryWriter(fsl);
            local_lnodes = new List<LNode>();
        }
        public int GetSetNode(UInt64 bword)
        {
            int code;
            if (!dic.TryGetValue(bword, out code))
            {
                code = local_wnodes.Count;
                dic.Add(bword, code);
                local_wnodes.Add(new WNode() { bword = bword }); // Заполняются только слова узлов
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
            bww.Write((long)local_wnodes.Count);
            foreach (var node in local_wnodes)
            {
                bww.Write(node.bword);
            }
            bww.Flush();

            bwl.Seek(0, SeekOrigin.Begin);
            bwl.Write((long)local_wnodes.Count);
            foreach (var node in local_wnodes)
            {
                bwl.Write(-1); // поля, которые в дальнейшем будут заполняться. -1 - null
                bwl.Write(-1);
            }
            bwl.Flush();
        }
        public int Count()
        {
            // Какой-то один из списков не должен быть пустым
            return local_lnodes.Count > 0 ? local_lnodes.Count : local_wnodes.Count;
        }
        public void Close() { fsw.Close(); fsl.Close(); }
        public void Save()
        {
            // Список слов надо записывать ТОЛЬКО если его длина больше нуля
            if (local_wnodes.Count > 0)
            {
                bww.Seek(0, SeekOrigin.Begin);
                long wcount = local_wnodes.Count;
                bww.Write(wcount);
                for (int i = 0; i < wcount; i++)
                {
                    var node = local_wnodes[i];
                    bww.Write(node.bword);
                }
            }
            fsw.Close();

            bwl.Seek(0, SeekOrigin.Begin);
            long lcount = local_lnodes.Count;
            bwl.Write(lcount);
            for (int i = 0; i < lcount; i++)
            {
                var node = local_lnodes[i];
                bwl.Write(node.prev);
                bwl.Write(node.next);
            }
            fsl.Close();
        }
        public void RestoreWNodes()
        {
            local_wnodes = new List<WNode>();
            fsw.Position = 0L;
            long wcount = brw.ReadInt64();
            for (int i = 0; i < wcount; i++)
            {
                UInt64 bword = brw.ReadUInt64();
                local_wnodes.Add(new WNode() { bword = bword });
            }
        }
        public void RestoreInitLNodes()
        {
            local_lnodes = Enumerable.Range(0, local_wnodes.Count).Select(i => new LNode() { prev = -1, next = -1 }).ToList();
        }
        public void RestoreDeactivateWNodes() { local_wnodes = new List<WNode>(); }
        public void RestoreLNodes()
        {
            fsl.Position = 0L;
            long lcount = brl.ReadInt64();
            local_lnodes = new List<LNode>((int)lcount);
            for (int i = 0; i < lcount; i++)
            {
                int prev = brl.ReadInt32();
                int next = brl.ReadInt32();
                local_lnodes.Add(new LNode() { prev = prev, next = next });
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
