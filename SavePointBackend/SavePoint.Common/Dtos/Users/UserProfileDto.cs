namespace SavePoint.Common.Dtos.Users
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public int ListCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}