#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using PX.Common;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class PXCustomException : PXSetPropertyException
	{
		private const string ErrorFieldProcessing = "Error";
		private const string ErrorFieldProcessingWithDescriptor = "Error with descriptor";

		public readonly string FieldName;

		protected PXCustomException(string fieldName, Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, errorLevel, format, args)
		{
			FieldName = fieldName;
		}

		public PXCustomException(string fieldName, Exception inner, PXErrorLevel errorLevel, params object[] args)
			: this(fieldName, inner, errorLevel, ErrorMessages.ErrorFieldProcessing, args)
		{
		}

		public PXCustomException(string fieldName, bool isWithDescriptor, Exception inner, PXErrorLevel errorLevel,
										  string fieldNameInErrorMessage, string errorText)
			: this(fieldName, inner, errorLevel, format: GetExceptionMessageFormat(isWithDescriptor),
				   args: new[] { fieldNameInErrorMessage, errorText })
		{
		}

		public PXCustomException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

		private static string GetExceptionMessageFormat(bool isWithDescriptor) =>
			isWithDescriptor
				? ErrorFieldProcessingWithDescriptor
				: ErrorFieldProcessing;
	}
}