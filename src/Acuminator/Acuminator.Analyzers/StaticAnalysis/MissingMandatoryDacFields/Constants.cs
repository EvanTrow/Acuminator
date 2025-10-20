namespace Acuminator.Analyzers.StaticAnalysis.MissingMandatoryDacFields;

internal static class Constants
{
	public const string FieldCategoriesSeparator = ",";
	public static readonly char[] FieldCategoriesSeparatorArray = FieldCategoriesSeparator.ToCharArray();

	public const string FieldCategoryAndInsertModeSeparator = "-";
	public static readonly char[] FieldCategoryAndInsertModeSeparatorArray = FieldCategoryAndInsertModeSeparator.ToCharArray();
}