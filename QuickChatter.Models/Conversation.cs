namespace QuickChatter.Models
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public ConnectedClient Inviter { get; set; }
        public ConnectedClient Accepter { get; set; }
        public List<ConversationMessage> Messages { get; set; }
        public bool IsAccepted { get; set; } = false;
    }
}
