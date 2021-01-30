using System;

namespace LogBroadcasterReciever.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            LogBroadcasterReciever log = new LogBroadcasterReciever();
            log.LogReceived += Log_LogReceived;
            log.LogReceivedComplete += Log_LogReceivedComplete;
            log.Start(20080);

            Console.WriteLine("Press any key.");
            Console.ReadKey();

            log.Cancel();

            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }

        private static void Log_LogReceived(object sender, LogMessage e)
        {
            Console.WriteLine(e.type);
            Console.WriteLine(e.condition);
            Console.WriteLine(e.stackTrace);
        }

        private static void Log_LogReceivedComplete(object sender, EventArgs e)
        {
            Console.WriteLine("End recieve");
        }
    }
}
