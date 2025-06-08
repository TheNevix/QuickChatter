namespace QuickChatter.Models
{
    public class UpdateInfo
    {
        public string version { get; set; }
        public string url { get; set; }
        public string changelog { get; set; }
        public List<string> changes { get; set; }
    }
}
