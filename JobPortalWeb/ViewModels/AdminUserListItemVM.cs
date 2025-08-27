namespace JobPortalWeb.ViewModels
{
    public class AdminUserListItemVM
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = "";
        public bool IsEmployer { get; set; }
        public bool IsSeeker { get; set; }
        public bool IsBanned { get; set; }
        public bool IsAdmin { get; set; }
    }
}
