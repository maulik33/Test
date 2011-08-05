using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using Emailer.Utilities;

namespace Emailer
{
    public enum ServiceCommands
    {
        StopWorker = 128,
        RestartWorker,
        CheckWorker
    }

    public enum ServiceState
    {
        ServiceStopped = 0x00000001,
        ServiceStartPending = 0x00000002,
        ServiceStopPending = 0x00000003,
        ServiceRunning = 0x00000004,
        ServiceContinuePending = 0x00000005,
        ServicePausePending = 0x00000006,
        ServicePaused = 0x00000007
    }

    public struct ServiceStatus
    {
        public int ServiceType;
        public int CurrentState;
        public int ControlsAccepted;
        public int Win32ExitCode;
        public int ServiceSpecificExitCode;
        public int CheckPoint;
        public int WaitHint;
    }

    public partial class EmailService : ServiceBase
    {
        #region Private Members
        private static ManualResetEvent _pause = new ManualResetEvent(false);
        private ServiceStatus _serviceStatus;
        private Thread _workerThread;

        private EmailManager _emailManager;
        #endregion

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int SetServiceStatus(IntPtr hServiceStatus, ref ServiceStatus lpServiceStatus);

        //public static void Main()
        //{
        //    Run(new EmailService());
        //}

        public EmailService()
        {
            InitializeComponent();
        }

        public void ServiceWorker()
        {
            try
            {
                _emailManager.ProcessEmails();
            }
            catch(Exception ex)
            {
                Logger.LogError("Service Worker Error", ex);
                Logger.LogDebug(ex.StackTrace);
            }
        }

        protected override void OnStart(string[] args)
        {
            IntPtr handle = ServiceHandle;
            _serviceStatus.CurrentState = (int) ServiceState.ServiceStartPending;
            SetServiceStatus(handle, ref _serviceStatus);

            try
            {
                // get a handle the email manager
                _emailManager = EmailManager.GetEmailManager();

                if (_emailManager == null)
                    throw new ApplicationException("Email manager object could not be created!");

                // start worker thread to perform the work
                if ((_workerThread == null) ||
                    ((_workerThread.ThreadState & ThreadState.Unstarted | ThreadState.Stopped)) != 0)
                {
                    _workerThread = new Thread(ServiceWorker);
                    _workerThread.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("OnStart Error", ex);
                Logger.LogDebug(ex.StackTrace);
            }

            _serviceStatus.CurrentState = (int) ServiceState.ServiceRunning;
            SetServiceStatus(handle, ref _serviceStatus);
            Logger.LogInfo("Service Started Successfully");
        }

        protected override void OnStop()
        {
            // give the email process time to shutdown (30 seconds by default) 
            _emailManager.StopEmailProcessing();

            // give the service a few more seconds to stop
            RequestAdditionalTime(4000);

            // signal worker thread to exit
            if((_workerThread != null) && (_workerThread.IsAlive))
            {
                _pause.Reset();
                Thread.Sleep(5000);
                _workerThread.Abort();
            }

            // successful exit
            ExitCode = 0;
            Logger.LogInfo("Service Stopped Successfully");
        }

        protected override void OnPause()
        {
            if((_workerThread != null) && (_workerThread.IsAlive) && ((_workerThread.ThreadState & (ThreadState.Suspended | ThreadState.SuspendRequested)) == 0))
            {
                _pause.Reset();
                Logger.LogInfo("Worker Thread Paused Successfully");
                Thread.Sleep(5000);
            }
        }

        protected override void OnContinue()
        {
            // signal worker thread to continue
            if((_workerThread != null) && ((_workerThread.ThreadState & (ThreadState.Suspended | ThreadState.SuspendRequested)) != 0))
            {
                _pause.Set();
                Logger.LogInfo("Worker Thread Continued Succesfully (from pause)");
            }
        }

        protected override void OnCustomCommand(int command)
        {
            switch(command)
            {
                case (int)ServiceCommands.StopWorker:
                    OnStop();
                    break;
                case (int)ServiceCommands.RestartWorker:
                    OnStart(null);
                    break;
                case (int)ServiceCommands.CheckWorker:
                    break;
                default:
                    break;
            }
        }
    }
}
