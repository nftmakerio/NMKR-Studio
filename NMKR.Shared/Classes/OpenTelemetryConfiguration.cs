using System.Diagnostics.CodeAnalysis;

namespace NMKR.Shared.Classes
{
    [ExcludeFromCodeCoverage]
    public sealed class OpenTelemetryConfiguration
    {
        /// <summary>
        /// Whether OpenTelemetry is enabled or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Jaeger endpoint being used
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Service name to use within jaeger
        /// </summary>
        public string ServiceName { get; set; }
    }
}