using System;
using System.Diagnostics.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.SharedInfo
{
	/// <summary>
	/// Information about the DAC field's size
	/// </summary>
	public readonly record struct DacFieldSize(int? Value)
	{
		private const int NotDefinedValue = -1;

		public static DacFieldSize NotDefined { get; } = new(NotDefinedValue);

		public static DacFieldSize MultipleSizesDeclared { get; } = new(null);

		[MemberNotNullWhen(returnValue: true, nameof(Value))]
		public bool IsNotDefined => Value == NotDefinedValue;

		[MemberNotNullWhen(returnValue: false, nameof(Value))]
		public bool IsInconsistent => Value is null;

		public override string ToString() => Value switch
		{
			NotDefinedValue => "not defined",
			null 			=> "inconsistent",
			_ 				=> Value.ToString()
		};
	}
}