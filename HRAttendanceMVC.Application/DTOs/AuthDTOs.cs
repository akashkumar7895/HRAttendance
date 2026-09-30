//using System.ComponentModel.DataAnnotations;

//namespace HRAttendanceMVC.Application.DTOs
//{
//    public class LoginDto
//    {
//        [Required(ErrorMessage = "Email is required")]
//        [EmailAddress]
//        public string Email { get; set; } = string.Empty;

//        [Required(ErrorMessage = "password is required")]
//        [DataType(DataType.Password)]
//        public string Password { get; set; } = string.Empty;
//    }

//    public class SignupDto
//    {
//        [Required(ErrorMessage = "The name is important")]
//        public string Name { get; set; } = string.Empty;

//        [Required(ErrorMessage = "Email is required")]
//        [EmailAddress]
//        public string Email { get; set; } = string.Empty;

//        [Required(ErrorMessage = "password is required")]
//        [DataType(DataType.Password)]
//        public string Password { get; set; } = string.Empty;

//        [Required(ErrorMessage = "Select role")]
//        public string Role { get; set; } = "HR";
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace HRAttendanceMVC.Application.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }


    public class SignupDto
    {
        [Required(ErrorMessage = "The name is important")]
        public string Name { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Select role")]
        [RegularExpression(
            "^(HR|Employee)$",
            ErrorMessage = "Please select a valid role."
        )]
        public string Role { get; set; } = "HR";
    }
}