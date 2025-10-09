using System.ComponentModel.DataAnnotations;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;

public class ValidCardanoAddressAttribute : ValidationAttribute
{
    RequiredAttribute _innerAttribute = new RequiredAttribute();

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (ConsoleCommand.CheckIfAddressIsValid(null, (string) value, GlobalFunctions.IsMainnet(), out string outaddress,
                out Blockchain blockchain, false, false))
            return ValidationResult.Success;

        string specificErrorMessage = ErrorMessage;
        return new ValidationResult(specificErrorMessage, new[] {validationContext.MemberName});
    }
}

public class ValidCardanoAddressOrAdaHandleAttribute : ValidationAttribute
{
    RequiredAttribute _innerAttribute = new RequiredAttribute();

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (object.ReferenceEquals(value.GetType(), typeof(string)))
        {
            if (ConsoleCommand.CheckIfAddressIsValid(null, (string) value, GlobalFunctions.IsMainnet(),
                                   out string outaddress,
                                   out Blockchain blockchain, false, false))
                return ValidationResult.Success;
        }

        /*if (ConsoleCommand.CheckIfAddressIsValid(null, (string)value, GlobalFunctions.IsMainnet(), out string outaddress,
                true, false))
            return ValidationResult.Success;
        */
        string specificErrorMessage = ErrorMessage;
        return new ValidationResult(specificErrorMessage, new[] { validationContext.MemberName });
    }
}