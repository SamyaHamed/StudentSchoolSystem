using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TestApp.Dto;
using TestApp.Models;
using TestApp.utilities;

namespace TestApp.Controllers;
[ApiController]
[Route("/api/account")]
public class AccountController:Controller
{

    private readonly UserManager<ApplicationUser> _userManager; 
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly GenerateJwtToken _jwtToken;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        GenerateJwtToken jwtToken )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtToken = jwtToken;

    }

    [HttpPost("register")] // sign up
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var user = new ApplicationUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
            FullName = registerDto.FullName,
            DateOfBirth = registerDto.DateOfBirth
        };
        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e=>e.Description).ToList();
            return new BadRequestObjectResult(errors);
        }
        
        var roleResult = await _userManager.AddToRoleAsync(user, "User");

        if (!roleResult.Succeeded)
        {
            var roleErrors = roleResult.Errors.Select(e => e.Description).ToList();
            return new BadRequestObjectResult(roleErrors);
        }

        return Ok(new { message = "User registered and role assigned successfully." });

    }

    [HttpPost("login")] //login
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if ( user== null)
            return Unauthorized(new { message ="Invalid email or password"});
        
        var result = await _signInManager.CheckPasswordSignInAsync(user,loginDto.Password,lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var roles = await _userManager.GetRolesAsync(user); 
        var token = _jwtToken.GenerateToken(user, roles); 

        return Ok(new
        {
            message = "Login Successful",
            token = token,
            user = new UserDto
            {
                Email = user.Email,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth
            }
        });
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost("logout")] //logout 
    public async Task<IActionResult> logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }
   
   [Authorize(Roles = "Admin, User")]
   [HttpPut("profile")] // update profile 
   public async Task<IActionResult> UpdateProfile(UserDto userdto)
   {
       var user = await _userManager.GetUserAsync(User);
       if (user == null)
       {
           return Unauthorized(new {message = "User not found"});
       }

       user.DateOfBirth = userdto.DateOfBirth ?? user.DateOfBirth;
       user.FullName = userdto.FullName ?? user.FullName;
       user.Email = userdto.Email ?? user.Email;
       var result = await _userManager.UpdateAsync(user);
       if (!result.Succeeded)
       {
           return new BadRequestObjectResult(result.Errors.Select(e=>e.Description).ToList());
       }
       return Ok(new {message = "Profile updated successfully"});
       


   }
    
    
   [Authorize (Roles = "Admin, User")]
   [HttpGet("profile")] // get profile  
   public async Task<IActionResult> GetProfile()
   {
       var user = await _userManager.GetUserAsync(User);
       if (user ==  null)
       {
           return Unauthorized(new { message = "UserNotFound" });
       }
       return Ok(
        new
       {
           user.Id,
           user.Email,
           user.UserName,
           user.FullName,
           user.DateOfBirth
       });


   }
   
   [Authorize(Roles = "Admin, User")]
   [HttpPost("change-password")] //change password
   public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
   {
       var user = await _userManager.GetUserAsync(User);
       if (user == null)
       {
           return Unauthorized (new{message = "User not found"});
       }
       var result = await _userManager.ChangePasswordAsync(user,changePasswordDto.CurrentPassword,changePasswordDto.NewPassword);
       if (!result.Succeeded)
       {
           return BadRequest(new{message = result.Errors.Select(e=>e.Description)});
       }
       return Ok(new {message = "Password changed successfully"});
   }
    
}