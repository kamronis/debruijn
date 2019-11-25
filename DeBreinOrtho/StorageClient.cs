using System;
using System.Collections.Generic;
using System.Text;

namespace DeBreinOrtho
{
    public enum Command
    {
        none, getsetnode, setnodeprev, setnodenext
    };
    public abstract class StorageClient
    {
        protected bool ismaster = true;
        protected int nsymbols;

        public abstract int NodesCount();
        public abstract int GetSetNode(UInt64 w);


        /// <summary>
        /// Посылка команды, ее аргументов, получение результата, использование результата
        /// </summary>
        public abstract void SendActionUseResult(Command comm, object[] args, Action<object> action);
        public abstract void SetNodeNextOrtho(int node, int nextlink, Action action);

        public abstract DBNode GetNode(int code);

        public abstract void SetNodePrev(int node, int prevlink);
        public abstract void SetNodeNext(int node, int nextlink);


    }
}
