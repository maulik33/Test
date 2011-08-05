using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using Emailer.Entity;
using Emailer.Utilities;
using System.Text;
using System.Linq;
using System.IO;

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
                //emailClient.Send(mailMessage.GetMailMessage());
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

        public static bool SendEmail(EmailMessage mailMessage, string recipient)
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
            IList<EmailRecipient> recipients = GetEmailRecipients(emailMission);

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

                switch (emailMission.EmailMissionType)
                {
                    case EmailType.Student:
                        while (reader.Read())
                            missionDetails.Add(new StudentEmailMessage(reader[0] as string, reader[1] as string, reader[2] as string));
                        break;
                    case EmailType.LocalAdmin:
                    case EmailType.TechAdmin:
                        Dictionary<string, string> adminStudentInfo = new Dictionary<string, string>();
                        while (reader.Read())
                            adminStudentInfo.Add(reader[0].ToString(), reader[1].ToString());
                        missionDetails.Add(new AdminEmailMessage(adminStudentInfo));
                        break;
                    case EmailType.Custom:

                        if (emailMission.UserType == 1)
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, string> customStudentInfo = new Dictionary<string, string>();
                                //customStudentInfo.Add(reader[0].ToString(), reader[1].ToString());
                                missionDetails.Add(new CustomEmailMessage(customStudentInfo, emailMission.EmailId, reader[2] as string));
                            }
                        }
                        else
                        {
                            Dictionary<string, string> customStudentInfo = new Dictionary<string, string>();
                            //while (reader.Read())
                            //   customStudentInfo.Add(reader[0].ToString(), reader[1].ToString());
                            missionDetails.Add(new CustomEmailMessage(customStudentInfo, emailMission.EmailId));
                        }
                        break;
                    default:
                        throw new ApplicationException("Invalid EmailType passed");
                }
            }

            Dictionary<string, bool> status = new Dictionary<string, bool>();

            if ((emailMission.EmailMissionType == EmailType.Custom && emailMission.UserType == (int)EmailUserType.Student) || emailMission.EmailMissionType == EmailType.Student)
            {
                //send email students
                for (int j = 0; j < missionDetails.Count; j++)
                {
                    bool isSendSuccessful = SendEmail(emailMission.EmailMissionType, missionDetails[j], missionDetails[j].RecipientEmailId);
                    if (isSendSuccessful)
                    {
                        Logger.LogInfo(string.Format("Email message sent successfully to {0}.", missionDetails[j].RecipientEmailId));
                    }
                    else
                    {
                        Logger.LogError(string.Format("Email message failed to send to {0}.", missionDetails[j].RecipientEmailId));
                    }
                    status.Add(string.Format("0.{0}", j), isSendSuccessful);
                }
            }
            else
            {
                // send the emails to other users
                for (int i = 0; i < recipients.Count; i++)
                {
                    for (int j = 0; j < missionDetails.Count; j++) // Gokul: wont the count be always 1 here?
                    {
                        bool isSendSuccessful = SendEmail(emailMission.EmailMissionType, missionDetails[j], recipients[i].EmailId);
                        if (isSendSuccessful)
                        {
                            Logger.LogInfo(string.Format("Email message send successfully to {0}.", recipients[i]));
                        }
                        else
                        {
                            Logger.LogError(string.Format("Email message failed to send to {0}.", recipients[i]));
                        }
                        status.Add(string.Format("{1}.{0}", j, i), isSendSuccessful);
                    }
                }
            }

            UpdateEmailStatus(emailMission.MissionId, 4);

            SendConfirmationEmail(emailMission, missionDetails, recipients, status);

            Logger.LogInfo(string.Format("Email mission {0} completed.", emailMission.MissionId));
            return EmailMissionState.SendSuccess;
        }

        private static void SendConfirmationEmail(EmailMission emailMission, List<EmailMessage> missionDetails
            , IList<EmailRecipient> recipients, Dictionary<string, bool> status)
        {
            StringBuilder confirmationEmailText = new StringBuilder();
            var statusBreakup = from s in status
                                group s by s.Value into g
                                select new { StatusText = g.Key, Count = g.Count() };
            confirmationEmailText.AppendLine("Status");
            foreach (var item in statusBreakup)
            {
                confirmationEmailText.AppendLine(string.Format("{0} : {1}", item.StatusText, item.Count));
            }
            confirmationEmailText.AppendLine("This Email has been sent to: " + GetSenderText(emailMission, missionDetails, recipients));
            string subject = "";

            EmailMessage firstEmail = missionDetails.FirstOrDefault();
            if (firstEmail != null)
            {
                subject = firstEmail.Subject;
            }

            foreach (var email in missionDetails)
            {
                confirmationEmailText.AppendLine(string.Format("To : {0}", email.To));
                confirmationEmailText.AppendLine(string.Format("Status : {0}", GetStatusText(emailMission
                    , email, status, missionDetails.IndexOf(email), recipients)));
                confirmationEmailText.AppendLine(email.Body);
                confirmationEmailText.AppendLine(GetSeperatorLine());
            }

            EmailMessage confirmationEmail = new ConfirmationEmailMessage(string.Format("Nursing(RN): Confirmation Email #{0} [{1}]"
                , emailMission.MissionId, subject)
                , emailMission.CreatorEmail, confirmationEmailText.ToString());
            SendEmail(confirmationEmail, emailMission.CreatorEmail);

            string filename = Path.Combine(@"D:\Temp\Temporary\SendConfirmation2", String.Format("{0}.txt", emailMission.MissionId));

            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine(confirmationEmailText.ToString());
            }
        }

        private static string GetStatusText(EmailMission mission, EmailMessage email, Dictionary<string, bool> status, int index, IList<EmailRecipient> recipients)
        {
            string key = "";
            string emailStatus = "Send Failed";
            if ((mission.EmailMissionType == EmailType.Custom && mission.UserType == (int)EmailUserType.Student) || mission.EmailMissionType == EmailType.Student)
            {
                key = string.Format("0.{0}", index);
            }
            else
            {
                EmailRecipient recipient = recipients.Where(p => p.EmailId == email.To).FirstOrDefault();
                if (recipient != null)
                {
                    key = string.Format("{1}.{0}", index, recipients.IndexOf(recipient));
                }
            }

            if (status.ContainsKey(key)
                && status[key] == true)
            {
                emailStatus = "Sent Successfully";
            }

            return emailStatus;
        }

        private static string GetSenderText(EmailMission emailMission, List<EmailMessage> missionDetails, IList<EmailRecipient> recipients)
        {
            StringBuilder senderText = new StringBuilder();
            try
            {
                EmailSelectionLevel selectionLevel = recipients.FirstOrDefault().Type;
                string selectionLevelText = EmailUtilities.ToAcronymProper(selectionLevel.ToString());

                switch (selectionLevel)
                {
                    case EmailSelectionLevel.StudentUser:
                    case EmailSelectionLevel.AdminUser:
                        senderText.AppendLine(EmailUtilities.ToAcronymProper(emailMission.EmailMissionType.ToString()));
                        break;
                    default:
                        senderText.AppendLine(string.Format("All {0} in {1}"
                            , EmailUtilities.ToAcronymProper(emailMission.EmailMissionType.ToString())
                            , selectionLevelText));
                        break;
                }

                foreach (var recipient in recipients.GroupBy(p => p.Name))
                {
                    senderText.AppendLine(recipient.Key);
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex.Message);
                senderText.AppendLine("Error occurred when constructing Recipient list");
            }

            senderText.AppendLine(GetSeperatorLine());

            return senderText.ToString();
        }

        public static string GetSeperatorLine()
        {
            return "".PadRight(150, '-');
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

                while (reader.Read())
                {
                    missions.Add(new EmailMission((int)reader[0], (int)reader[1], (int)reader[2], reader["CreatorEmail"].ToString()));
                }
                return missions.ToArray();
            }
        }

        public static IList<EmailRecipient> GetEmailRecipients(EmailMission emailMission)
        {
            switch (emailMission.UserType)
            {
                case 0:
                    return GetAdminRecipients(emailMission.MissionId);
                case 1:
                    return GetStudentRecipients(emailMission.MissionId);
                default:
                    return null;
            }
        }

        private static IList<EmailRecipient> GetStudentRecipients(int missionId)
        {
            return GetRecipients(missionId, "uspGetStudentMissionRecipients");
        }

        private static IList<EmailRecipient> GetAdminRecipients(int missionId)
        {
            return GetRecipients(missionId, "uspGetAdminMissionRecipients");
        }

        private static IList<EmailRecipient> GetRecipients(int missionId, string spName)
        {
            List<EmailRecipient> recipients = new List<EmailRecipient>();

            #region Sql Parameters
            SqlParameter[] sqlParameters = new SqlParameter[1];
            SqlParameter parameterMissionId = new SqlParameter("@emailMissionId", SqlDbType.Int, 4);
            parameterMissionId.Value = missionId;
            sqlParameters[0] = parameterMissionId;
            #endregion

            using (IDataReader reader = DAO.Core.GetDataReader(spName, sqlParameters))
            {
                if (reader == null)
                    return null;

                while (reader.Read())
                {
                    recipients.Add(new EmailRecipient()
                    {
                        EmailId = (reader["Email"] as string) ?? "",
                        Name = (reader["Name"] as string) ?? "",
                        Type = (EmailSelectionLevel)Enum.Parse(typeof(EmailSelectionLevel), reader["SelectionLevel"].ToString() ?? "100")
                    });
                }
            }
            return recipients;
        }

        public static string[] GetCustomEmailDefinition(int emailId)
        {
            #region Sql Parameters
            SqlParameter[] sqlParameters = new SqlParameter[1];
            SqlParameter parameterEmailId = new SqlParameter("@emailId", SqlDbType.Int, 4);
            parameterEmailId.Value = emailId;
            sqlParameters[0] = parameterEmailId;
            #endregion

            using (IDataReader reader = DAO.Core.GetDataReader("uspGetCustomEmailDefinition", sqlParameters))
            {
                if (reader == null)
                    return null;

                reader.Read();
                return new[] { reader[0].ToString(), reader[1].ToString() };
            }
        }
    }
}
