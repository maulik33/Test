namespace Emailer.Entity
{
    public enum EmailMissionState
    {
        Queued = 1,
        SendFailed = 2,
        SendSuccess = 3,
        NothingToSend = 8
    }

    public enum EmailType
    {
        Student = -100,
        LocalAdmin = -101,
        TechAdmin = -102,
        Custom = 133
    }

    public enum EmailUserType
    {
        Student = 1,
        Admin = 0
    }

    public enum EmailSelectionLevel
    {
        Institution = 1,
        Cohort = 2,
        Group = 3,
        StudentUser = 4,
        AdminUser = 5,
        Unknown = 100
    }

    public struct EmailMission
    {
        public int MissionId;
        public EmailType EmailMissionType;
        public int EmailId;
        public int UserType;
        public string CreatorEmail;

        public EmailMission(int missionId, int emailId, int userType, string creatorEmail)
        {
            MissionId = missionId;
            EmailId = emailId;
            UserType = userType;
            CreatorEmail = creatorEmail;

            // set email type based on emailId
            if (emailId > 0)
                EmailMissionType = EmailType.Custom;
            else
                EmailMissionType = (EmailType) emailId;
        }
    }
}
