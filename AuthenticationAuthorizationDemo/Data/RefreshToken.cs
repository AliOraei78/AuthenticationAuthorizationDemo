namespace AuthenticationAuthorizationDemo.Data
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TokenHash { get; set; } = string.Empty;       // hashed value
        public string TokenSalt { get; set; } = string.Empty;       // optional salt if using PBKDF2
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string? RevokedByIp { get; set; }
        public string? ReplacedByTokenHash { get; set; }         
    }
}