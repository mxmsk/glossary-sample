using System;
using System.Runtime.Serialization;

namespace Glossary.Terms.Services
{
	/// <summary>
	/// The exception that is thrown by <see cref="ITermsService"/> when storage is invalid.
	/// </summary>
	[Serializable]
	public class InvalidTermsStorageException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidTermsStorageException"/> class.
		/// </summary>
		public InvalidTermsStorageException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidTermsStorageException"/> class
		/// with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public InvalidTermsStorageException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the System.Exception class with a specified
		/// error message and a reference to the inner exception that is the cause of
		/// this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a
		/// <langword>null</langword> reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public InvalidTermsStorageException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidTermsStorageException"/> class with
		/// serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
		/// about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
		/// about the source or destination.</param>
		protected InvalidTermsStorageException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
