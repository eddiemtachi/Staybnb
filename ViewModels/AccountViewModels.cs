using System.ComponentModel.DataAnnotations;
namespace Staybnb.ViewModels;
public class RegisterViewModel
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), MinLength(8)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
}
public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}
