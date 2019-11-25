using System;
using System.Collections.Generic;
using System.Text;

namespace DeBreinData
{
    public class ModelStorage
    {
        private List<DBNode> nodes = new List<DBNode>();
        private Dictionary<string, int> wordToCode = new Dictionary<string, int>();
        public ModelStorage() {  }

        public int plinks = 0, nlinks = 0;
        public int GetNodeId(string w)
        {
            int code = -1;
            if (!wordToCode.TryGetValue(w, out code))
            {
                code = nodes.Count;
                nodes.Add(new DBNode(code, w));
                wordToCode.Add(w, code);
            }
            return code;
        }
        public void SetNodePrev(int node, int prevlink)
        {
            DBNode dnode = nodes[node];
            if      (dnode.prev == -1)       { dnode.prev = prevlink; plinks++; }
            else if (dnode.prev == prevlink) { }
            else if (dnode.prev == -2)       { }
            else                             { dnode.prev = -2; plinks--; }
        }
        public void SetNodeNext(int node, int nextlink)
        {
            DBNode dnode = nodes[node];
            if (dnode.next == -1) { dnode.next = nextlink; nlinks++; }
            else if (dnode.next == nextlink) { }
            else if (dnode.next == -2) { }
            else { dnode.next = -2; nlinks--; }
        }
        public int NodeCount() { return nodes.Count;  }
    }
    public class DBNode
    {
        internal int code;
        internal int prev = -1;
        internal int next = -1;
        internal bool waschecked = false;
        internal string word;
        public DBNode(int code, string word)
        {
            this.code = code;
            this.word = word;
        }
    }
}
