using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrtoMatrix
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start OrtoMatrix");
            Scanning();
        }
        static int ncmax = 20, nrmax = 10;
        static async void Scanning()
        {
            Task t0 = ScanRow(0);
            Task t1 = ScanRow(1);
            Task t2 = ScanRow(2);
            Task t3 = ScanRow(3);
            await Task.WhenAll(new Task[] { t0, t1, t2, t3 });
        }
        static async Task ScanRow(int row)
        {
            for (int i=0; i<ncmax; i++)
            {
                await DoStep(row, i);
            }
        } 
        static async Task DoStep(int ir, int ic)
        {
            Console.WriteLine($"{ir} {ic}");
            Task.
        }
        static async Task SendReceive(int ir, int ic)
        {

        } 
    }
}
