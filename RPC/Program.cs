using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RPC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            double time = 3;
            if (args.Length > 1 && args[0] == "-t" && args[1].All(char.IsDigit)) time = Convert.ToDouble(args[1]);
            Console.WriteLine($"Время: {time} сек");

            bool isEnabled = false;
            RpcCore rpc = null;

            while (true)
            {
                Process[] p = Process.GetProcessesByName("dreamseeker");
                if (p.Length > 0 && !string.IsNullOrEmpty(p[0].MainWindowTitle) && isEnabled == false)
                {
                    isEnabled = true;
                    rpc = new RpcCore();
                    rpc.Loop().GetAwaiter();
                }
                else if (p.Length == 0 && rpc != null && isEnabled == true)
                {
                    isEnabled = false;
                    rpc.Close();
                }
                await Task.Delay(TimeSpan.FromSeconds(time));
            }
        }
    }
}
