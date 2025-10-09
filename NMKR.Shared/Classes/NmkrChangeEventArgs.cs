using System;

namespace NMKR.Shared.Classes
{

    public class NmkrChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        public object? Value { get; set; }
    }
}