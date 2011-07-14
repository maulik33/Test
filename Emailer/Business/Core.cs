using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using Emailer.Entity;
using Emailer.Utilities;

namespace Emailer.Business
{
    public static class Core
    {
        public static bool SendEmail(EmailType mailType, EmailMessage mailMessage, string recipient)
        {
            SmtpClient emailClient = new SmtpClient(EmailSender.ServerName);
            try
            {
                mailMessage.To = recipient;
                emailClient.Send(mailMessage.GetMailMessage());
                Logger.LogInfo("Message sent successfully.");
            }
            catch (SmtpException smtpException)
            {
                Logger.LogError("SendEmail Error", smtpException);
                Logger.LogDebug(smtpException.StackTrace);
                return false;
            }
            return true;
        }

        public static EmailMissionState SendEmail(EmailMission emailMission)
        {
            // get email recipients
            string[] recipients = GetEmailRecipients(emailMission);

            // get mission details
            List<EmailMessage> missionDetails = new List<EmailMessage>();

            #region Sql Parameters
            SqlParameter[] sqlParameters = new SqlParameter[1];
            SqlParameter parameterEmailMissionId = new SqlParameter("@emailMissionId", SqlDbType.Int, 4);
            parameterEmailMissionId.Value = emailMission.MissionId;
            sqlParameters[0] = parameterEmailMissionId;
            #endregion

            using (IDataReader reader = DAO.Core.GetDataReader("uspGetEmailMissionDetails", sqlParameters))
            {
                if (reader == null)
                    return EmailMissionState.NothingToSend;

                switch(emailMission.EmailMissionType)
                {
                    case EmailType.Student:
                        while(reader.Read())
                            missionDetails.Add(new StudentEmailMessage(reader[0] as string, reader[1] as string));
                        break;
                    case EmailType.LocalAdmin:
                    case EmailType.TechAdmin:
                        Dictionary<string, string> adminStudentInfo = new Dictionary<string, string>();
                        while(reader.Read())
                            adminStudentInfo.Add(reader[0].ToString(), reader[1].ToString());
                        missionDetails.Add(new AdminEmailMessage(adminStudentInfo));
                        break;
                    case EmailType.Custom:
                        Dictionary<string, string> customStudentInfo = new Dictionary<string, string>();
                        //while (reader.Read())
                        //    customStudentInfo.Add(reader[0].ToString(), reader[1].ToString());
                        missionDetails.Add(new CustomEmailMessage(customStudentInfo, emailMission.EmailId));
                        break;
                    default:
                        throw new ApplicationException("Invalid EmailType passed");
                }
            }

            // send the emails to the users
            for(int i = 0; i < recipients.Length; i++)
            {
                for (int j = 0; j < missionDetails.Count; j++)
                {
                    if (SendEmail(emailMission.EmailMissionType, missionDetails[j], recipients[i]))
                        Logger.LogInfo(string.Format("Email message send successfully to {0}.", recipients[i]));
                    else
                        Logger.LogError(string.Format("Email message failed to send to {0}.", recipients[i]));
                }
            }

            UpdateEmailStatus(emailMission.MissionId, 4);
            Logger.LogInfo(string.Format("Email mission {0} completed.", emailMission.MissionId));
            return EmailMissionState.SendSuccess;
        }

        public static void UpdateEmailStatus(int emailId, int emailStatus)
        {
            #region Parameters
            SqlParameter[] sqlParameters = new SqlParameter[2];
            SqlParameter parameterId = new SqlParameter("@emailMissionId", SqlDbType.Int, 4);
            parameterId.Value = emailId;
            sqlParameters[0] = parameterId;

            SqlParameter parameterStatus = new SqlParameter("@emailStatus", SqlDbType.Int, 4);
            parameterStatus.Value = emailStatus;
            sqlParameters[1] = parameterStatus;
            #endregion

            DAO.Core.ExecuteNonQuery("uspUpdateEmailStatus", sqlParameters);
        }

        public static EmailMission[] GetQueuedMissions()
        {
            List<EmailMission> missions = new List<EmailMission>();

            using (IDataReader reader = DAO.Core.GetDataReader("uspGetQueuedMissions"))
            {
                if (reader == null)
                    return null;

                while(reader.Read())
                {
                    missions.Add(new EmailMission((int)reader[0], (int)reader[1], (int)reader[2]));
                }
                return missions.ToArray();
            }
        }

        public static string[] GetEmailRecipients(EmailMission emailMission)
        {
            switch(emailMission.UserType)
            {
                case 0:
                    return GetAdminRecipients(emailMission.MissionId);
                case 1:
                    return GetStudentRecipients(emailMission.MissionId);
                default:
                    return null;
            }
        }

        private static string[] GetStudentRecipients(int missionId)
        {
            List<string> recipients = new List<string>();

            #region Sql Parameters
            SqlParameter[] sqlParameters = new SqlParameter[1];
            SqlParameter parameterMissionId = new SqlParameter("@emailMissionId", SqlDbType.Int, 4);
            parameterMissionId.Value = missionId;
            sqlParameters[0] = parameterMissionId;
            #endregion

            using (IDataReader reader = DAO.Core.GetDataReader("uspGetStudentMissionRecipients", sqlParameters))
            {
                if (reader == null)
                    return null;

                while (reader.Read())
                {
                    recipients.Add(reader[0].ToString());
                }
                return recipients.ToArray();
            }
        }

        private static string[] GetAdminRecipients(int missionId)
        {
            List<string> recipients = new List<string>();

            #region Sql Parameters
            SqlParameter[] sqlParameters = new SqlParameter[1];
            SqlParameter parameterMissionId = new SqlParameter("@emailMissionId", SqlDbType.Int, 4);
            parameterMissionId.Value = missionId;
            sqlParameters[0] = parameterMissionId;
            #endregion

            using (IDataReader reader = DAO.Core.GetDataReader("uspGetAdminMissionRecipients", sqlParameters))
            {
                if (reader == null)
                    return null;

                while (reader.Read())
                {
                    recipients.Add(reader[0].ToString());
                }
                return recipients.ToArray();
            }
        }

        public static string[] GetCustomEmailDefinition(int emailId)
        {
            #region Sql Parameters
            SqlParameter[] sqlParameters = new SqlParameter[1];
            SqlParameter parameterEmailId = new SqlParameter("@emailId", SqlDbType.Int, 4);
            parameterEmailId.Value = emailId;
            sqlParameters[0] = parameterEmailId;
            #endregion

            using(IDataReader reader = DAO.Core.GetDataReader("uspGetCustomEmailDefinition", sqlParameters))
            {
                if(reader == null)
                    return null;

                reader.Read();
                return new[]{reader[0].ToString(), reader[1].ToString()};
            }
        }
    }
}
