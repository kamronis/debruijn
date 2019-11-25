using System;
using System.Collections.Generic;
using System.Text;

namespace DeBreinOrtho
{
    class StorageClientMaster : StorageClient
    {
        // Если мастер, то есть хранилище
        private StorageSection storageofmaster = null;

        public StorageClientMaster(int nsymbols, bool ismaster)
        {
            this.nsymbols = nsymbols;
            this.ismaster = ismaster;
            if (ismaster)
            {
                storageofmaster = new StorageSection(nsymbols);
            }
        }

        public override int NodesCount()
        {
            return storageofmaster.NodesCount;
        }
        public override int GetSetNode(UInt64 w)
        {
            return storageofmaster.GetSetNode(w);
        }
        public override DBNode GetNode(int code) { return storageofmaster.GetNode(code); }

        public override void SendActionUseResult(Command comm, object[] args, Action<object> action)
        {
            object res = null;
            switch (comm)
            {
                case Command.none: throw new Exception("I dont know how to implement empty command");
                case Command.getsetnode:
                    {
                        res = GetSetNode((UInt64)args[0]);
                        break;
                    }
                case Command.setnodeprev:
                    {
                        SetNodePrev((int)args[0], (int)args[1]);
                        break;
                    }
                case Command.setnodenext:
                    {
                        SetNodeNext((int)args[0], (int)args[1]);
                        break;
                    }
                default: throw new Exception("Unimplemented 289734");
            }
            action(res);
        }
        public override void SetNodeNextOrtho(int node, int nextlink, Action action)
        {
            SetNodeNext(node, nextlink);
            action();
        }

        public override void SetNodePrev(int node, int prevlink)
        {
            storageofmaster.SetNodePrev(node, prevlink);
        }
        public override void SetNodeNext(int node, int nextlink)
        {
            storageofmaster.SetNodeNext(node, nextlink);
        }


        /// <summary>
        /// Извлечение цепочек
        /// </summary>
        public void ExtractChains()
        {
            storageofmaster.ExtractChains();
        }

        public void Statistics()
        {
            // Теперь в хранилище есть узлы. По построению они от 0 до NodesCount-1. 
            // Берем последовательно узлы и собираем статистику
            int nisolated = 0, nleftonly = 0, nrightonly = 0, nfull = 0;
            int nfollow = 0, nnotfollow = 0;
            for (int nd = 0; nd < this.NodesCount(); nd++)
            {
                DBNode node = this.GetNode(nd);
                int prev = node.prev;
                int next = node.next;
                if (prev < 0 && next < 0) nisolated++;
                else if (prev >= 0 && next < 0) nleftonly++;
                else if (prev < 0 && next >= 0) nrightonly++;
                else if (prev >= 0 && next >= 0) nfull++;

                if (next >= 0)
                {
                    if (next == nd + 1) nfollow++;
                    else nnotfollow++;
                }
            }
            Console.WriteLine($"{nisolated} {nleftonly} {nrightonly} {nfull}");
            Console.WriteLine($"follow: {nfollow} not follow: {nnotfollow}");
        }

    }
}
