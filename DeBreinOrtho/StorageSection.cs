using System;
using System.Collections.Generic;
using System.Text;

namespace DeBreinOrtho
{
    public class StorageSection
    {
        private List<DBNode> nodes = new List<DBNode>();
        private Dictionary<UInt64, int> wordToCode = new Dictionary<UInt64, int>();
        
        private int nsymbols;

        public StorageSection(int nsymbols)
        {
            this.nsymbols = nsymbols;
        }
        
        public int NodesCount { get { return nodes.Count; } }
        
        public int GetSetNode(UInt64 w)
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
        public DBNode GetNode(int code) { return nodes[code]; }

        public void SetNodePrev(int node, int prevlink)
        {
            DBNode dnode = nodes[node];
            if (dnode.prev == -1) { dnode.prev = prevlink; }
            else if (dnode.prev == prevlink) { }
            else if (dnode.prev == -2) { }
            else { dnode.prev = -2; }
        }
        public void SetNodeNext(int node, int nextlink)
        {
            DBNode dnode = nodes[node];
            if (dnode.next == -1) { dnode.next = nextlink; }
            else if (dnode.next == nextlink) { }
            else if (dnode.next == -2) { }
            else { dnode.next = -2; }
        }

        // Технические процедуры
        // Определим функции: узел не null и у узла есть единичная ссылка назад или вперед 
        Func<DBNode, bool> HasSinglePrev = (DBNode nd) => nd != null && nd.prev != -1 && nd.prev != -2;
        Func<DBNode, bool> HasSingleNext = (DBNode nd) => nd != null && nd.next != -1 && nd.next != -2;

        /// <summary>
        /// Извлечение цепочек
        /// </summary>
        public void ExtractChains()
        {
            int nchains = 0;
            int maxchain = 0;
            DBNode[] maxlist = new DBNode[0];

            // Основная идея в том, что цепочка может начинаться ТОЛЬКО с узла у которого нет предыдущего или 
            // предыдущих несколько или предыдущий один, но у него несколько следующих.
            // Кроме того, нет смысла рассматривать те узлы, у которых не единственный следующий.
            for (int n = 0; n < nodes.Count; n++)
            {
                DBNode node = nodes[n];
                //if (node.waschecked) continue; -- ОТМЕТКИ НЕ ИГРАЮТ РОЛИ 
                DBNode ndd = node;
                // Проверяем условие стартового узла
                if (ndd.prev == -1 ||
                    ndd.prev == -2 ||
                    ndd.prev >= 0 && nodes[ndd.prev].next == -2
                    ) { } // начальный
                else { continue; }

                // Зафиксируем новую цепочку
                List<DBNode> list = new List<DBNode>(new DBNode[] { ndd });
                // пробежимся вперед
                while (HasSingleNext(ndd) && HasSinglePrev(nodes[ndd.next])) { ndd = nodes[ndd.next]; list.Add(ndd); }
                nchains++;
                if (list.Count > maxchain) { maxchain = list.Count; maxlist = list.ToArray(); }
            }
            Console.WriteLine($"==== nchains: {nchains}  maxchain: {maxchain}");

            // Выдача максимальной цепочки
            Console.Write(DBNode.UnCombine(maxlist[0].word32x2, nsymbols));
            for (int i = 1; i < maxlist.Length; i++)
            {
                DBNode node = maxlist[i];
                var word = node.word32x2;
                string sword = DBNode.UnCombine(word, nsymbols);
                Console.Write(sword[sword.Length - 1]);
            }
            Console.WriteLine();
        }

    }
}
