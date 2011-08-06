using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emailer.Entity;
using System.Diagnostics;

namespace Emailer
{
    class program
    {
#if DEBUG
        public static void Main(string[] args)
        {
            try
            {
                exec();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error Occurred : " + ex.Message);
            }
            Debug.WriteLine("Email Engine completed its execution.");
        }
#endif
        private static void exec()
        {
            //EmailMission[] result = Business.Core.GetQueuedMissions().Skip(20).Take(20).ToArray();
            EmailMission[] result = Business.Core.GetQueuedMissions();
            foreach (var email in result)
            {
                Business.Core.SendEmail(email);
            }
        }
    }
}
