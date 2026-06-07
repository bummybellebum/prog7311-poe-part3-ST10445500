//ST10445500 - PROG7311 - GLMS POE
//AdminUserListItemViewModel

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.ViewModels.Account
{
    public class AdminUserListItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DisplayName
        {
            get
            {
                var name = $"{FirstName} {LastName}".Trim();
                return string.IsNullOrWhiteSpace(name) ? Email : name;
            }
        }
    }
}

//.....................................o0oEND OF FILEo0o........................................//
