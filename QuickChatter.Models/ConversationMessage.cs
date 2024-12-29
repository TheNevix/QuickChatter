namespace QuickChatter.Models
{
    public class ConversationMessage
    {
        public DateTime SentOn { get; set; }
        public string Message { get; set; }
        public ConnectedClient SentBy { get; set; }
    }
}
