using System;
using System.Reflection;
using log4net.Config;

namespace Emailer.Utilities
{
    public static class Logger
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Logger()
        {
            XmlConfigurator.Configure();
        }

        #region LogInfo
        public static void LogInfo(string message)
        {
            LogInfo(message, null);
        }

        public static void LogInfo(string message, Exception ex)
        {
            if(Log.IsInfoEnabled)
                Log.Info(message, ex);
        }
        #endregion

        #region LogWarn
        public static void LogWarning(string message)
        {
            LogWarning(message, null);
        }

        public static void LogWarning(string message, Exception ex)
        {
            if(Log.IsWarnEnabled)
                Log.Warn(message, ex);
        }
        #endregion

        #region LogDebug
        public static void LogDebug(string message)
        {
            LogDebug(message, null);
        }

        public static void LogDebug(string message, Exception ex)
        {
            if(Log.IsDebugEnabled)
                Log.Debug(message, ex);
        }
        #endregion

        #region LogError
        public static void LogError(string message)
        {
            LogError(message, null);
        }

        public static void LogError(string message, Exception ex)
        {
            if(Log.IsErrorEnabled)
                Log.Error(message, ex);
        }
        #endregion

        #region LogFatal
        public static void LogFatal(string message)
        {
            LogFatal(message, null);
        }

        public static void LogFatal(string message, Exception ex)
        {
            if(Log.IsFatalEnabled)
                Log.Fatal(message, ex);
        }
        #endregion
    }
}
