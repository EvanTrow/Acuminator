namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;

internal static class Constants
{
	public const string FieldKindsSeparator = ",";
	public static readonly char[] FieldKindsSeparatorArray = FieldKindsSeparator.ToCharArray();

	public const string FieldKindAndInsertModeSeparator = "-";
	public static readonly char[] FieldKindAndInsertModeSeparatorArray = FieldKindAndInsertModeSeparator.ToCharArray();
}