using Microsoft.AspNet.Identity.EntityFramework;

namespace Core.Entities
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MobileNo { get; set; }
        public string? GovIdType { get; set; }
        public string? GovId { get; set; }

        public Address? Address { get; set; }
    }
}