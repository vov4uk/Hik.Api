using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public uint ErrorCode { get; }
        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get { return GetEnumDescription((HikError)ErrorCode); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="HikException"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="errorCode">The error code.</param>
        public HikException(string method, uint errorCode)
            : base(method)
        {
            ErrorCode = errorCode;
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

        private static string GetEnumDescription(HikError value)
        {
            string val = value.ToString();
            FieldInfo fi = value.GetType().GetField(val);

            if (fi != null && fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
            {
                return attributes.First().Description;
            }

            return val;
        }
    }
}
