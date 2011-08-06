using System;
using System.Configuration;

namespace Emailer.Utilities
{
    public static class ConfigMgr
    {
        public static string GetConfigValue(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch (Exception ex)
            {
                Logger.LogError("GetConfigValue Error", ex);
                Logger.LogDebug(ex.StackTrace);
            }
            return null;
        }

        public static string GetConnectionStringValue(string key)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[key].ToString();
            }
            catch (Exception ex)
            {
                Logger.LogError("GetConnectionStringValue Error", ex);
                Logger.LogDebug(ex.StackTrace);
            }
            return null;
        }
    }
}
