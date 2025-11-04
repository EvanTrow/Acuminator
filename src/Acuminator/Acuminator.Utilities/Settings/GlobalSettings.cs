using System;
using System.Threading;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
	public class GlobalSettings
	{
		private const int NOT_INITIALIZED = 0, INITIALIZED = 1;
		private static int _isInitialized = NOT_INITIALIZED;

		private static CodeAnalysisSettings? _cachedCodeAnalysisSettings;
		private static BannedApiSettings? _cachedBannedApiSettings;
		private static CodeMapSettings? _cachedCodeMapSettings;

		public static CodeAnalysisSettings AnalysisSettings => _cachedCodeAnalysisSettings ?? CodeAnalysisSettings.Default;

		public static BannedApiSettings BannedApiSettings => _cachedBannedApiSettings ?? BannedApiSettings.Default;

		public static CodeMapSettings CodeMapSettings => _cachedCodeMapSettings ?? CodeMapSettings.Default;

		/// <summary>
		/// Initializes the global settings once. Must be called on package initialization.
		/// </summary>
		/// <param name="codeAnalysisSettings">The code analysis settings.</param>
		/// <param name="bannedApiSettings">The banned API settings.</param>
		/// <param name="codeMapSettings">The code map settings.</param>
		public static void InitializeGlobalSettingsOnce(CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings,
														CodeMapSettings codeMapSettings)
		{
			codeAnalysisSettings.ThrowOnNull();
			bannedApiSettings.ThrowOnNull();
			codeMapSettings.ThrowOnNull();

			if (Interlocked.CompareExchange(ref _isInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				_cachedCodeAnalysisSettings = codeAnalysisSettings;
				_cachedBannedApiSettings 	= bannedApiSettings;
				_cachedCodeMapSettings 		= codeMapSettings;
			}
		}

		/// <summary>
		/// Initializes the global settings in a thread unsafe way. For tests only.
		/// </summary>
		/// <param name="codeAnalysisSettings">The code analysis settings.</param>
		/// <param name="bannedApiSettings">The banned API settings.</param>
		/// <param name="codeMapSettings">The code map settings.</param>
		internal static void InitializeGlobalSettingsThreadUnsafeForTestsOnly(CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings,
																			  CodeMapSettings codeMapSettings)
		{
			_cachedCodeAnalysisSettings = codeAnalysisSettings.CheckIfNull();
			_cachedBannedApiSettings 	= bannedApiSettings.CheckIfNull();
			_cachedCodeMapSettings 		= codeMapSettings.CheckIfNull();
		}
	}
}