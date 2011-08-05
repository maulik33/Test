using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emailer.Entity;

namespace Emailer
{
    class program
    {
        public static void Main(string[] args)
        {
            int i = 0;
            do
            {
                Console.WriteLine(string.Format("************* Executing ({0}) *************", ++i));
                try
                {
                    exec();
                }
                catch (Exception ex)
                {
                    Console.Beep();
                    ConsoleColor fr = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = fr;
                }
                Console.WriteLine("Execution completed. Press 'r' to re-execute.");
            } while (Console.ReadKey(true).KeyChar == 'r');
        }

        private static void exec()
        {
            EmailMission[] result = Business.Core.GetQueuedMissions().Take(10).ToArray();
            foreach (var email in result)
            {
                Business.Core.SendEmail(email);
            }


        }
    }
}
