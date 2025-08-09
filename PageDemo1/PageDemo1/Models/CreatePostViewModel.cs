namespace PageDemo1.Models
{
    public class CreatePostViewModel
    {
        public List<string> UserNames { get; set; }
        public string SelectedUser { get; set; }
        public string Content { get; set; }
        public string Prompt { get; set; }
    }
}
