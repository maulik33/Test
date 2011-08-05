using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emailer.Entity
{
    public class EmailRecipient
    {
        public string EmailId { get; set; }

        public EmailSelectionLevel Type { get; set; }

        public string Name { get; set; }
    }
}
