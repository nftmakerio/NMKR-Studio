using System;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ExtendedApiExplorerSettingsAttribute :
    Attribute,
    IApiDescriptionGroupNameProvider,
    IApiDescriptionVisibilityProvider
{
    /// <inheritdoc />
    public string? GroupName { get; set; }

    private bool _ignoreApi;

    /// <inheritdoc />
    public bool IgnoreApi
    {
        get { return _ignoreApi; }
        set { _ignoreApi = !GeneralConfigurationClass.UseTestnet && value; }
    }
}