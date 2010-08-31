using System.Data;
using System.Data.SqlClient;
using Emailer.Utilities;

namespace Emailer.DAO
{
    internal static class Core
    {
        #region Private Members
        public static string ConStr;
        #endregion

        #region Properties
        public static string ConnectionString
        {
            get
            {
                if(!string.IsNullOrEmpty(ConStr))
                    return ConStr;

                ConStr = ConfigMgr.GetConnectionStringValue("NursingEmailer");
                if(string.IsNullOrEmpty(ConStr))
                {
                    Logger.LogInfo("ConnectionString Property: Empty");
                    return null;
                }
                return ConStr;
            }
        }
        #endregion

        static Core()
        {
            ConStr = ConnectionString;
        }

        public static int ExecuteNonQuery(string procName, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConStr))
            {
                using (SqlCommand cmd = new SqlCommand(procName, conn))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);
                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                    catch (SqlException sqlException)
                    {
                        Logger.LogError("ExecuteNonQuery Error", sqlException);
                        Logger.LogDebug(sqlException.StackTrace);
                        throw;
                    }
                }
            }
        }

        public static int ExecuteStoredProcedure(string procName, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConStr))
            {
                using (SqlCommand cmd = new SqlCommand(procName, conn))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);
                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                    catch (SqlException sqlException)
                    {
                        Logger.LogError("ExecuteStoredProcedure Error", sqlException);
                        Logger.LogDebug(sqlException.StackTrace);
                        throw;
                    }
                }
            }
        }

        public static object ExecuteScalar(string procName, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConStr))
            {
                using (SqlCommand cmd = new SqlCommand(procName, conn))
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);
                        conn.Open();
                        return cmd.ExecuteScalar();
                    }
                    catch (SqlException sqlException)
                    {
                        Logger.LogError("ExecuteScalar Error", sqlException);
                        Logger.LogDebug(sqlException.StackTrace);
                        throw;
                    }
                }
            }
        }

        public static SqlDataReader GetDataReader(string procName, params SqlParameter[] parameters)
        {
            SqlConnection conn = new SqlConnection(ConStr);
            SqlCommand cmd = new SqlCommand(procName, conn);

            try
            {
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (SqlParameter param in parameters)
                    cmd.Parameters.Add(param);

                conn.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (SqlException sqlException)
            {
                conn.Close();
                Logger.LogError("GetDataReader Error", sqlException);
                Logger.LogDebug(sqlException.StackTrace);
                throw;
            }
        }
    }
}
