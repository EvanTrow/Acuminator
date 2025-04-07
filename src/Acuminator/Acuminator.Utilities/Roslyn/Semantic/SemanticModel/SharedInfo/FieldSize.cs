namespace Acuminator.Utilities.Roslyn.Semantic.SharedInfo
{
	/// <summary>
	/// Information about the DAC field's size
	/// </summary>
	public record struct DacFieldSize(int? Value)
	{
		public static DacFieldSize NotDefined { get; } = new(-1);

		public static DacFieldSize MultipleSizesDeclared { get; } = new(null);
	}
}