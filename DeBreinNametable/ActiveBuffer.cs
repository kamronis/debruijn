using System;
using System.Collections.Generic;
using System.Text;

namespace DeBruijnNametable
{
    public class ActionBuffer
    {
        private int size;
        private List<object[]> list;
        private Action<IEnumerable<object[]>> handler;
        public ActionBuffer(int size, Action<IEnumerable<object[]>> handler) 
        {
            this.size = size;
            list = new List<object[]>();
            this.handler = handler;
        }
        public void Add(object[] el) 
        { 
            list.Add(el);
            if (list.Count >= size)
            {
                Flush();
            } 
        }
        public void Flush()
        {
            handler(list.ToArray());
            list = new List<object[]>();
        }
    }

    public class ActiveBuffer<T>
    {
        private int size;
        private List<T> list;
        private Action<IEnumerable<T>> handler;
        public ActiveBuffer(int size, Action<IEnumerable<T>> handler)
        {
            this.size = size;
            list = new List<T>();
            this.handler = handler;
        }
        public void Add(T el)
        {
            list.Add(el);
            if (list.Count >= size)
            {
                handler(list);
                list = new List<T>();
            }
        }
        public void Flush()
        {
            if (list.Count > 0)
            {
                handler(list);
                list = new List<T>();
            }
        }
    }

}
