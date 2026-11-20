using Microsoft.AspNetCore.Identity;
using TheGadgetZone.Business.Entities;

namespace TheGadgetZone.DataAccess.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
