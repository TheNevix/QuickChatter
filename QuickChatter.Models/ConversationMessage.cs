using System.Text.Json.Serialization;

namespace QuickChatter.Models
{
    public partial class ConversationMessage
    {
        public DateTime SentOn { get; set; }
        public string Message { get; set; }
        public User SentBy { get; set; }
        [JsonIgnore]
        public object Image { get; set; }
    }
}
