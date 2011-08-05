using System.Collections.Generic;
using System.Net.Mail;
using Emailer.Utilities;

namespace Emailer.Entity
{
    public abstract class EmailMessage
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        public string RecipientEmailId { get; set; }

        protected string StudentAppLink;
        protected string AdminAppLink;

        protected EmailMessage()
        {
            StudentAppLink = ConfigMgr.GetConfigValue("studentAppLink");
            AdminAppLink = ConfigMgr.GetConfigValue("adminAppLink");

            if (string.IsNullOrEmpty(StudentAppLink) || string.IsNullOrEmpty(AdminAppLink))
            {
                Logger.LogError("Student application link configuration value missing!");
                Logger.LogDebug("EmailMessage constructor");
            }
        }

        public MailMessage GetMailMessage()
        {
            return new MailMessage(From, To, Subject, Body);
        }
    }

    public class ConfirmationEmailMessage : EmailMessage
    {
        public ConfirmationEmailMessage(string subject, string to, string body)
        {
            Subject = subject;
            From = "integrated.support@kaplan.com";
            Body = body;
            To = to;
        }
    }

    public class StudentEmailMessage : EmailMessage
    {
        public StudentEmailMessage(string username, string password, string emailid)
        {
            Subject = "Your Kaplan Nursing account details";
            From = "integrated.support@kaplan.com";
            Body =
                "Dear Nursing Student:\r\n\r\nWe are delighted to be working with you and your nursing school faculty to provide Kaplan/LWW testing and remediation materials to reinforce your classwork.  Below you will find your online access to the materials.  Your faculty will instruct you as to which secured or integrated tests you are taking and when, and you will be able to access remediation materials and additional unsecured tests from home at your convenience. This remediation will help you solidify your nursing content foundation, especially in your weaker areas.\r\n\r\nJust click on the link below, or cut and paste it to your browser to log in with the user name and password provided.  We suggest you start by clicking on the icon, Watch Me First to get an audio overview of the materials and best use.\r\n\r\nWe wish you all the best and hope you will get in touch if you have any questions.\r\n\r\n" +
                "Link:\r\n" + StudentAppLink + "\r\n\r\nUser Name: " + username + "      Password: " + password +
                "\r\n\r\n\r\n\r\nBest wishes from your Service Team,\r\n\r\nintegrated.support@kaplan.com<mailto:integrated.support@kaplan.com>";
            this.RecipientEmailId = emailid;
        }
    }

    public class AdminEmailMessage : EmailMessage
    {
        public AdminEmailMessage(Dictionary<string, string> users)
        {
            Subject = "Kaplan Nursing students account info list";
            From = "integrated.support@kaplan.com";
            Body = "Dear Nursing School Administrator:\r\n\r\nThank you for selecting the Kaplan and Lippincott Williams & Wilkins Integrated Testing program.  We look forward to working with you and your students to benchmark their progress and remediate their knowledge deficit.\r\n\r\nAttached are the user names and passwords for your students.  Each student has received an individual email with his/her username and password. Please keep this list of all the usernames and passwords available in a secure location in the lab in case someone comes to test without their username and password.  Please note: This list of usernames and passwords should not be posted to protect the security of the testing system.\r\n\r\nThe link to your administrator account where you can view individual and aggregate reports is:\r\n\r\n" + AdminAppLink + "\r\n\r\nYou can access all reports through your user name and password:\r\n";

            foreach (KeyValuePair<string, string> user in users)
            {
                Body += "User Name: " + user.Key + "      Password: " + user.Value + "\r\n";
            }

            Body += "\r\n\r\nPlease don't hesitate to contact us with questions, service requests or feedback.  The success of your students is very important to us, and we look forward to working with your faculty to help ensure that each student has the necessary tools to maximize their performance.";
            Body += "\r\n\r\n\r\n\r\nBest wishes from your Service Team,\r\n\r\nintegrated.support@kaplan.com<mailto:integrated.support@kaplan.com>";
        }
    }

    public class CustomEmailMessage : EmailMessage
    {
        public CustomEmailMessage(Dictionary<string, string> users, int emailId)
        {
            string[] customEmailData = Business.Core.GetCustomEmailDefinition(emailId);

            Subject = customEmailData[0];
            From = "integrated.support@kaplan.com";
            Body = customEmailData[1] + "\r\n\r\n";

            foreach (KeyValuePair<string, string> user in users)
            {
                Body += "User Name: " + user.Key + "      Password: " + user.Value + "\r\n";
            }
        }

        public CustomEmailMessage(Dictionary<string, string> users, int emailId, string recipientEmailId)
        {
            string[] customEmailData = Business.Core.GetCustomEmailDefinition(emailId);

            Subject = customEmailData[0];
            From = "integrated.support@kaplan.com";
            Body = customEmailData[1] + "\r\n\r\n";

            foreach (KeyValuePair<string, string> user in users)
            {
                Body += "User Name: " + user.Key + "      Password: " + user.Value + "\r\n";
            }

            this.RecipientEmailId = recipientEmailId;
        }
    }
}
