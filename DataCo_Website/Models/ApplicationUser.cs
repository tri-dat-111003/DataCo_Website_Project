using Microsoft.AspNetCore.Identity;

namespace DataCo_Website.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? CustomerId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual Customer? Customer { get; set; }
    }
}
