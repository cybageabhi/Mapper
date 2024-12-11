namespace Server.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }

        public string DisplayName { get; set; }
        public string Password { get; set; }
        public DateTime Created { get; set; }
        public bool Enabled { get; set; }
        public string EmailAddress { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
