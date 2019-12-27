using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace DeBruijn
{
    public interface INodePart
    {
        void Init(bool firsttime);
        void RestoreWNodes();
        void RestoreInitLNodes();
        void RestoreDeactivateWNodes();
        void RestoreLNodes();
        int Count();
        void Save();
        void Close();
        void DropDictionary();
        LNode GetLNodeLocal(int localcode);

        int GetSetNode(BWord bword);
        IEnumerable<int> GetSetNodes(IEnumerable<BWord> bwords);
        void MakePrototype();

        void SetNodePrev(int localcode, NCode prevlink);
        void SetNodeNext(int localcode, NCode nextlink);

        IEnumerable<LNode> GetNodes(IEnumerable<int> localcodes);
        IEnumerable<BWord> GetWNodes(IEnumerable<int> localcodes);
    }
}
