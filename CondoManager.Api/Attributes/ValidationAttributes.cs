using CondoManager.Entity.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CondoManager.Api.Attributes
{
    public class EmailValidationAttribute : ValidationAttribute
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var email = value.ToString();
            if (!EmailRegex.IsMatch(email))
            {
                return new ValidationResult("Please enter a valid email address.");
            }

            return ValidationResult.Success;
        }
    }

    public class PasswordValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Password is required.");
            }

            var password = value.ToString();
            if (password.Length < 6)
            {
                return new ValidationResult("Password must be at least 6 characters long.");
            }

            if (!password.Any(char.IsDigit))
            {
                return new ValidationResult("Password must contain at least one digit.");
            }

            if (!password.Any(char.IsLetter))
            {
                return new ValidationResult("Password must contain at least one letter.");
            }

            return ValidationResult.Success;
        }
    }

    public class PhoneValidationAttribute : ValidationAttribute
    {
        private static readonly string ValidDddsPattern =
            string.Join("|", Enum.GetValues(typeof(BrazilianDDD)).Cast<int>());

        private static readonly Regex PhoneRegex = new Regex(
            $@"^(?:{ValidDddsPattern})(?:9\d{8}|[2-5]\d{7})$",
            RegexOptions.Compiled);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Optional field
            }

            // Normalize: remove all non-digits
            var phone = Regex.Replace(value.ToString(), @"[^\d]", "");

            // Must be 10 or 11 digits (2 DDD + 8 or 9 phone digits)
            if (phone.Length != 10 && phone.Length != 11)
            {
                return new ValidationResult("Phone number must contain a valid DDD and 8 or 9 digits.");
            }

            // Extract DDD (first 2 digits)
            var ddd = int.Parse(phone.Substring(0, 2));

            // Validate if DDD exists in enum
            if (!Enum.IsDefined(typeof(BrazilianDDD), ddd))
            {
                return new ValidationResult($"Invalid DDD: {ddd}.");
            }

            // Extract the local number (after DDD)
            var localNumber = phone.Substring(2);

            // Validate rules: mobile (9 digits starting with 9) or landline (8 digits starting with 2–5)
            if (localNumber.Length == 9 && localNumber.StartsWith("9"))
            {
                return ValidationResult.Success; // valid mobile
            }

            if (localNumber.Length == 8 && "2345".Contains(localNumber[0]))
            {
                return ValidationResult.Success; // valid landline
            }

            return new ValidationResult("Please enter a valid Brazilian phone number with DDD.");
        }
    }

    public class ApartmentNumberValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Apartment number is required.");
            }

            var apartmentNumber = value.ToString();
            if (apartmentNumber.Length > 10)
            {
                return new ValidationResult("Apartment number cannot exceed 10 characters.");
            }

            return ValidationResult.Success;
        }
    }
}