namespace Emailer.Entity
{
    public class Email : EmailBase
    {
        public Email(EmailTemplate template) : base(template)
        {
        }

        public string[] GetEmailRecipients(EmailPersonType personType, EmailGroupType groupType)
        {
            return GetRecipients(0, personType, groupType);
        }

        public override string[] GetRecipients(int groupId, EmailPersonType personType, EmailGroupType groupType)
        {
            return Business.Core.GetRecipients(groupId, personType, groupType);
        }
    }
}
