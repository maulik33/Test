using System;
using System.Configuration;

namespace Emailer.Utilities
{
    public static class ConfigMgr
    {
        public static string GetConfigValue(string index)
        {
            try
            {
                return ConfigurationManager.AppSettings[index];
            }
            catch (Exception ex)
            {
                Logger.LogError("GetConfigValue Error", ex);
                Logger.LogDebug(ex.StackTrace);
            }
            return null;
        }

        public static string GetConnectionStringValue(string index)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[index].ToString();
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
