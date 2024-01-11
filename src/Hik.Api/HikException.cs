using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Hik.Api
{
    /// <summary>
    /// Hik Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class HikException : Exception
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="HikException"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="error">The error.</param>
        public HikException(string method, string error)
            : base(method)
        {
            ErrorMessage = error;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{ErrorMessage}{Environment.NewLine}{base.ToString()}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HikException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected HikException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
