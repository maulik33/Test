using Emailer.Utilities;

namespace Emailer.Entity
{
    public static class EmailSender
    {
        public static string ServerName;

        #region Properties
        public static string Server
        {
            get
            {
                if(!string.IsNullOrEmpty(ServerName))
                    return ServerName;

                ServerName = ConfigMgr.GetConfigValue("mailServer");
                if(string.IsNullOrEmpty(ServerName))
                {
                    Logger.LogInfo("Server Property: Empty");
                    return null;
                }
                return ServerName;
            }
        }
        #endregion

        static EmailSender()
        {
            ServerName = Server;
        }
    }
}
