namespace QuickChatter.Models
{
    public class ConversationMessage
    {
        public DateTime SentOn { get; set; }
        public string Message { get; set; }
        public User SentBy { get; set; }
    }
}
