using System;
using System.Threading.Tasks;

namespace TCP
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start TCPServer");
            string host = "127.0.0.1";
            int port = 8888;

            ServerConnection sconnection = new ServerConnection(host, port);
            sconnection.Start();
        }
    }
}
