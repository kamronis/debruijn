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
        void Restore();
        int Count();
        void Save();
        void Close();
        void DropDictionary();
        CNode GetNodeLocal(int nom);

        int GetSetNode(UInt64 bword);
        IEnumerable<int> GetSetNodes(IEnumerable<UInt64> bwords);
        void MakePrototype();

        void SetNodePrev(int local, int prevlink);
        void SetNodeNext(int local, int nextlink);

        IEnumerable<CNode> GetNodes(IEnumerable<int> codes);
    }
    public class NodesPart : INodePart
    {
        private FileStream fs;
        private BinaryReader br;
        private BinaryWriter bw;
        private Dictionary<UInt64, int> dic = new Dictionary<ulong, int>();
        private List<CNode> local_nodes = new List<CNode>();
        private string nodepartfilename;
        public NodesPart(string nodepartfilename)
        {
            this.nodepartfilename = nodepartfilename;
        }
        public void Init()
        { 
            fs = File.Open(nodepartfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            br = new BinaryReader(fs);
            bw = new BinaryWriter(fs);
            local_nodes = new List<CNode>();
        }
        public int GetSetNode(UInt64 bword)
        {
            int code;
            if (!dic.TryGetValue(bword, out code))
            {
                code = local_nodes.Count;
                dic.Add(bword, code);
                local_nodes.Add(new CNode() { bword = bword });
            }
            return code;
        }
        public IEnumerable<int> GetSetNodes(IEnumerable<UInt64> bwords)
        {
            return bwords.Select(bw => GetSetNode(bw));
        }
        public CNode GetNodeLocal(int nom) { return local_nodes[nom]; }
        public void MakePrototype()
        {
            bw.Seek(0, SeekOrigin.Begin);
            bw.Write((long)local_nodes.Count);
            foreach (var node in local_nodes)
            {
                bw.Write(node.bword);
                bw.Write(-1); // поля, которые в дальнейшем будут заполняться. -1 - null
                bw.Write(-1);
            }
            bw.Flush();
            //fs.Close();
        }
        public int Count() { return local_nodes.Count; }
        public void Close() { fs.Close(); }
        public void Save()
        {
            //fs.Position = 0L;
            bw.Seek(0, SeekOrigin.Begin);
            long nnodes = local_nodes.Count;
            bw.Write(nnodes);
            for (int i = 0; i < nnodes; i++)
            {
                var node = local_nodes[i];
                bw.Write(node.bword);
                bw.Write(node.prev);
                bw.Write(node.next);
            }
            fs.Close();
        }
        public void Restore()
        {
            // Восстанавливаем файл и потоки
            //fs = File.Open(nodepartfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //br = new BinaryReader(fs);
            //bw = new BinaryWriter(fs);
            // Восстанавливаем список
            local_nodes = new List<CNode>();
            fs.Position = 0L;
            long nnodes = br.ReadInt64();
            for (int i = 0; i < nnodes; i++)
            {
                UInt64 bword = br.ReadUInt64();
                int prev = br.ReadInt32();
                int next = br.ReadInt32();
                local_nodes.Add(new CNode() { bword = bword, prev = prev, next = next });
            }
        }
        
        public void DropDictionary() { dic = new Dictionary<ulong, int>(); }

        public void SetNodePrev(int local, int prevlink)
        {
            int node_nom = local;
            var dnode = local_nodes[node_nom];
            if (dnode.prev == -1) { dnode.prev = prevlink; }
            else if (dnode.prev == prevlink) { }
            else if (dnode.prev == -2) { }
            else { dnode.prev = -2; }
            local_nodes[node_nom] = dnode;
        }
        public void SetNodeNext(int local, int nextlink)
        {
            int node_nom = local;
            var dnode = local_nodes[node_nom];
            if (dnode.next == -1) { dnode.next = nextlink; }
            else if (dnode.next == nextlink) { }
            else if (dnode.next == -2) { }
            else { dnode.next = -2; }
            local_nodes[node_nom] = dnode;
        }

        public IEnumerable<CNode> GetNodes(IEnumerable<int> codes)
        {
            return codes.Select(code =>
            {
                int node_nom = code >> Options.nshift;
                var dnode = local_nodes[node_nom];
                return dnode;
            });
        }
    }
}
