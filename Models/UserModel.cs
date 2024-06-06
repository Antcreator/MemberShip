using Microsoft.AspNetCore.Identity;

namespace MemberShip.Models
{
    public class UserModel : IdentityUser
    {
        public string? Name { get; set; }
        public List<Payment>? Payments { get; set; }
    }
}
