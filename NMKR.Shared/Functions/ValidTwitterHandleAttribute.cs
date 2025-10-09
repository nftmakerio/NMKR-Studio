using NMKR.Shared.Functions;
using System.ComponentModel.DataAnnotations;

public class ValidTwitterHandleAttribute : ValidationAttribute
{
    RequiredAttribute _innerAttribute = new RequiredAttribute();

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        string twh=(string)value;
        if (GlobalFunctions.CheckTwitterHandle(ref twh))
            return ValidationResult.Success;

        string specificErrorMessage = ErrorMessage;
        return new ValidationResult(specificErrorMessage, new[] { validationContext.MemberName });
    }
}