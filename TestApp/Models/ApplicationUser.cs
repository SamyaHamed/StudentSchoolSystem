using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Identity;

namespace TestApp.Models;

public class ApplicationUser: IdentityUser
{
   public DateTime DateOfBirth{ get; set; }
   public string FullName { get; set; }
}