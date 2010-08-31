using System;
using System.Configuration;
using System.Threading;
using Emailer.Entity;

namespace Emailer.Utilities
{
    public enum ProcessStatus
    {
        Alive,
        Dying,
        Dead
    }

    public class EmailManager
    {
        private static EmailManager _emailManager;

        private Thread _workerThread;
        private readonly SafeQueue _missionQueue;
        private object _processing;
        private readonly int _pollingInterval;

        private EmailManager()
        {
            int result;
            if (Int32.TryParse(ConfigurationManager.AppSettings.Get("runtimeInterval"), out result))
                _pollingInterval = result;
            else
                throw new ApplicationException("runtimeInterval applicaton configuration missing");

            _processing = false;
            _missionQueue = new SafeQueue();
        }

        public static EmailManager GetEmailManager()
        {
            if (_emailManager == null)
                _emailManager = new EmailManager();
            return _emailManager;
        }

        public void ProcessEmails()
        {
            while (true)
            {
                // shortcut optimization
                bool optOut;
                lock (_processing)
                {
                    optOut = (bool)_processing;
                }

                // check for new emails
                if (!optOut)
                {
                    Logger.LogInfo("Checking for new missions");
                    EmailMission[] result = CheckForNewEmailMissions();

                    if (result != null && result.Length > 0)
                    {
                        // add missions to the queue
                        for (int i = 0; i < result.Length; i++)
                        {
                            _missionQueue.Enqueue(result[i]);
                        }

                        // start the worker to process these
                        _workerThread = new Thread(SendEmailMissions);
                        _workerThread.Start();
                        _processing = true;
                    }
                }

                // main thread can go to sleep for an interval
                Thread.Sleep(_pollingInterval);
            }
        }

        public void StopEmailProcessing()
        {

        }

        private static EmailMission[] CheckForNewEmailMissions()
        {
            return Business.Core.GetQueuedMissions();
        }

        private void SendEmailMissions()
        {
            try
            {
                // process the email missions in the queue
                while (_missionQueue.Count > 0)
                {
                    EmailMission result = (EmailMission)_missionQueue.Dequeue();
                    Business.Core.SendEmail(result);
                }
            }
            catch(Exception ex)
            {
                Logger.LogError("SendEmailMissions Error", ex);
                Logger.LogDebug(ex.StackTrace);
            }
            finally
            {
                // ensure the queue is empty (no exceptions)
                _missionQueue.Clear();

                // set process flag back to normal "ready" state
                lock (_processing)
                {
                    _processing = false;
                }
            }
        }
    }
}