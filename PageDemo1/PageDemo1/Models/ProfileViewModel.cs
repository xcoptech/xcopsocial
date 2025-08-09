namespace PageDemo1.Models
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }
        public string ProfileImage { get; set; }
        public string Bio { get; set; }
        public int Followers { get; set; }
        public int Friends { get; set; }
        public List<Post> Posts { get; set; }
    }


}
