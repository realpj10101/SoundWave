using System.ComponentModel.DataAnnotations;
using api.DTOs.Account;

namespace api.DTOs;

public record LoginDto(
    [MaxLength(50)] string Email,
    [DataType(DataType.Password)]
    [Length(PropLength.PasswordMinLength, PropLength.PasswordMaxLength)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Needs: 8 to 50 characters. An uppercase character(ABC). A number(123)")]
    string Password
);