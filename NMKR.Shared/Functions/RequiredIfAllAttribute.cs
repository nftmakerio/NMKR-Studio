using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections;
using System.Linq;

public class RequiredIfAllAttribute : ValidationAttribute
{
    private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();
    public string[] _dependentProperties { get; set; }
    public List<List<object>> _targetValues { get; set; }

    public RequiredIfAllAttribute(params object[] dependentPropertiesAndValues)
    {
        if (dependentPropertiesAndValues.Length % 2 != 0)
        {
            throw new ArgumentException("The number of dependent properties and values should be even.");
        }

        _dependentProperties = new string[dependentPropertiesAndValues.Length / 2];
        _targetValues = new List<List<object>>();

        for (int i = 0; i < dependentPropertiesAndValues.Length; i += 2)
        {
            string dependentProperty = (string)dependentPropertiesAndValues[i];
            IEnumerable targetValues = (IEnumerable)dependentPropertiesAndValues[i + 1];
            _dependentProperties[i / 2] = dependentProperty;
            _targetValues.Add(targetValues.Cast<object>().ToList());
        }
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (_dependentProperties.Length != _targetValues.Count)
        {
            throw new ArgumentException("Number of dependent properties should match the number of target values lists.");
        }

        bool anyConditionMet = true;

        for (int i = 0; i < _dependentProperties.Length; i++)
        {
            var field = validationContext.ObjectType.GetProperty(_dependentProperties[i]);
            if (field != null)
            {
                var dependentValue = field.GetValue(validationContext.ObjectInstance, null);
                if (!_targetValues[i].Contains(dependentValue))
                {
                    anyConditionMet = false;
                    break;
                }
             
            }
            else
            {
                return new ValidationResult(FormatErrorMessage(_dependentProperties[i]));
            }
        }

        if (!anyConditionMet)
        {
            return ValidationResult.Success;
        }

        if (!_innerAttribute.IsValid(value))
        {
            string name = validationContext.DisplayName;
            string specificErrorMessage = ErrorMessage;
            if (specificErrorMessage.Length < 1)
                specificErrorMessage = $"{name} is required.";

            return new ValidationResult(specificErrorMessage, new[] { validationContext.MemberName });
        }

        return ValidationResult.Success;
    }
}