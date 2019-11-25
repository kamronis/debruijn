using System;
using System.Collections.Generic;
using System.Text;

namespace DeBreinData
{
    public class Reed
    {
        public int command = 0;
        public object[] args = null;
        public Action<object> Useresult;

        public static int nwords = 0; 
        private string line;
        private int portion;
        //private ModelStorage storage;
        private int len;
        private int nom = 0;
        private int prevCode = -1, currCode = -1;

        public bool ToProcess { get { return nom < len - portion + 1; } }

        public Reed(string line, int portion)
        {
            this.line = line;
            this.portion = portion;
            //this.storage = storage;
            len = line.Length;
        }

       /* public void Process0()
        {
            for (nom = 0; nom < len - portion + 1; nom++)
            {
                // state0
                string word = line.Substring(nom, portion);
                //nom++;
                nwords++;
                // Теперь надо запросить узел
                currCode = storage.GetNodeId(word);
                // state1
                if (prevCode != -1)
                {
                    // state2
                    storage.SetNodeNext(prevCode, currCode);
                    // state3
                    storage.SetNodePrev(currCode, prevCode);
                }
                // state4
                prevCode = currCode;
            }
        }*/

        public void Process()
        {

        }
        private int state = 0;
        /// <summary>
        /// Один шаг метода Process
        /// </summary>
        public void Step()
        {
            // nom < len - portion + 1;
            if (state == 0)
            {
                command = 1;
                string word = line.Substring(nom, portion);
                nom++;
                nwords++;
                // Теперь надо запросить узел - это внешняя операция, будет буферизоваться
                //currCode = storage.GetNodeId(word);
                args = new object[] { word };
                Useresult = (object res) => { currCode = (int)res; };
                
                state = 1;
            }
            else if (state == 1) // можно втянуть в предыдущую обработку
            {
                command = 0;
                if (prevCode != -1) state = 2;
                else state = 4;
            }
            else if (state == 2)
            {
                command = 2;
                //storage.SetNodeNext(prevCode, currCode);
                args = new object[] { prevCode, currCode };
                Useresult = (object res) => { };
                state = 3;
            }
            else if (state == 3)
            {
                command = 3;
                //storage.SetNodePrev(currCode, prevCode);
                args = new object[] { currCode, prevCode };
                Useresult = (object res) => { };
                state = 4;
            }
            else if (state == 4)
            {
                command = 0;
                prevCode = currCode;
                state = 0;
            }
        }
    }
}
