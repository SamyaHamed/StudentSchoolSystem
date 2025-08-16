using System.Runtime.InteropServices.JavaScript;

namespace TestApp.Dto;

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }

    public DateTime DateOfBirth { get; set; }


}