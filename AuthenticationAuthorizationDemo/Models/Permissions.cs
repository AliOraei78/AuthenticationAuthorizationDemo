namespace AuthenticationAuthorizationDemo.Models
{
    public static class Permissions
    {
        public const string CanViewUsers = "users:view";
        public const string CanEditUsers = "users:edit";
        public const string CanDeleteUsers = "users:delete";
        public const string CanManageRoles = "roles:manage";
        public const string CanManageUsers = "users:manage";   // or any name you prefer (e.g. "users:full")
    }
}