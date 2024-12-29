using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickChatter.Models
{
    public class Conversation
    {
        public ConnectedClient Inviter { get; set; }
        public ConnectedClient Accepter { get; set; }
        public List<ConversationMessage> Messages { get; set; }
        public bool IsAccepted { get; set; } = false;
    }
}
