using System.Linq;

public class OpenApiHeaderIgnoreAttribute : System.Attribute
{
}
public class OpenApiHeaderIgnoreFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
   
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        
      /*  if (GeneralConfigurationClass.UseTestnet)
            return;*/

        if (operation == null || context == null || context.ApiDescription?.ParameterDescriptions == null)
            return;

        var parametersToHide = context.ApiDescription.ParameterDescriptions
            .Where(parameterDescription => ParameterHasIgnoreAttribute(parameterDescription))
            .ToList();

        if (parametersToHide.Count == 0)
            return;



        foreach (var parameterToHide in parametersToHide)
        {
            var parameter = operation.Parameters.FirstOrDefault(parameter => string.Equals(parameter.Name, parameterToHide.Name, System.StringComparison.Ordinal));
            if (parameter != null)
                operation.Parameters.Remove(parameter);
        }
    }

    private static bool ParameterHasIgnoreAttribute(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription parameterDescription)
    {
        if (parameterDescription.ModelMetadata is Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata metadata)
        {
            return metadata.Attributes.ParameterAttributes.Any(attribute => attribute.GetType() == typeof(OpenApiHeaderIgnoreAttribute));
        }

        return false;
    }
}