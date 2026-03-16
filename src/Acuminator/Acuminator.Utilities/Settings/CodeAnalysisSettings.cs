using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace Acuminator.Utilities
{
	[Export]
	public class CodeAnalysisSettings : IEquatable<CodeAnalysisSettings>
	{
		public const bool DefaultRecursiveAnalysisEnabled 			  = true;
		public const bool DefaultISVSpecificAnalyzersEnabled 		  = false;
		public const bool DefaultSuppressionMechanismEnabled 		  = true;
		public const bool DefaultStaticAnalysisEnabled 				  = true;
		public const bool DefaultPX1007DocumentationDiagnosticEnabled = false;
		public const bool DefaultInfoDiagnosticsEnabled 			  = true;

		public static CodeAnalysisSettings Default => 
			new CodeAnalysisSettings(
				DefaultRecursiveAnalysisEnabled,
				DefaultISVSpecificAnalyzersEnabled,
				DefaultStaticAnalysisEnabled,
				DefaultSuppressionMechanismEnabled,
				DefaultPX1007DocumentationDiagnosticEnabled,
				DefaultInfoDiagnosticsEnabled);

		public virtual bool RecursiveAnalysisEnabled { get; }

		public virtual bool IsvSpecificAnalyzersEnabled { get; }

		public virtual bool StaticAnalysisEnabled { get; }

		public virtual bool SuppressionMechanismEnabled { get; }

		public virtual bool PX1007DocumentationDiagnosticEnabled { get; }

		public virtual bool InfoDiagnosticsEnabled { get; }

		protected CodeAnalysisSettings()
		{
			InfoDiagnosticsEnabled = DefaultInfoDiagnosticsEnabled;
		}

		public CodeAnalysisSettings(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled, bool staticAnalysisEnabled, 
									bool suppressionMechanismEnabled, bool px1007DocumentationDiagnosticEnabled, bool infoDiagnosticsEnabled)
		{
			RecursiveAnalysisEnabled 			 = recursiveAnalysisEnabled;
			IsvSpecificAnalyzersEnabled 		 = isvSpecificAnalyzersEnabled;
			StaticAnalysisEnabled 				 = staticAnalysisEnabled;
			SuppressionMechanismEnabled 		 = suppressionMechanismEnabled;
			PX1007DocumentationDiagnosticEnabled = px1007DocumentationDiagnosticEnabled;
			InfoDiagnosticsEnabled 				 = infoDiagnosticsEnabled;
		}

		public CodeAnalysisSettings WithRecursiveAnalysisEnabled() => WithRecursiveAnalysisEnabledValue(true);

		public CodeAnalysisSettings WithRecursiveAnalysisDisabled() => WithRecursiveAnalysisEnabledValue(false);

		protected virtual CodeAnalysisSettings WithRecursiveAnalysisEnabledValue(bool value) =>
			new CodeAnalysisSettings(value, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, SuppressionMechanismEnabled, 
									 PX1007DocumentationDiagnosticEnabled, InfoDiagnosticsEnabled);

		public CodeAnalysisSettings WithIsvSpecificAnalyzersEnabled() => WithIsvSpecificAnalyzersEnabledValue(true);

		public CodeAnalysisSettings WithIsvSpecificAnalyzersDisabled() => WithIsvSpecificAnalyzersEnabledValue(false);

		protected virtual CodeAnalysisSettings WithIsvSpecificAnalyzersEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, value, StaticAnalysisEnabled, SuppressionMechanismEnabled, 
									 PX1007DocumentationDiagnosticEnabled, InfoDiagnosticsEnabled);


		public CodeAnalysisSettings WithStaticAnalysisEnabled() => WithStaticAnalysisEnabledValue(true);

		public CodeAnalysisSettings WithStaticAnalysisDisabled() => WithStaticAnalysisEnabledValue(false);

		protected virtual CodeAnalysisSettings WithStaticAnalysisEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, value, SuppressionMechanismEnabled, 
									 PX1007DocumentationDiagnosticEnabled, InfoDiagnosticsEnabled);


		public CodeAnalysisSettings WithSuppressionMechanismEnabled() => WithSuppressionMechanismEnabledValue(true);

		public CodeAnalysisSettings WithSuppressionMechanismDisabled() => WithSuppressionMechanismEnabledValue(false);

		protected virtual CodeAnalysisSettings WithSuppressionMechanismEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, value, 
									 PX1007DocumentationDiagnosticEnabled, InfoDiagnosticsEnabled);


		public CodeAnalysisSettings WithPX1007DocumentationDiagnosticEnabled() => WithPX1007DocumentationDiagnosticEnabledValue(true);

		public CodeAnalysisSettings WithPX1007DocumentationDiagnosticDisabled() => WithPX1007DocumentationDiagnosticEnabledValue(false);

		protected virtual CodeAnalysisSettings WithPX1007DocumentationDiagnosticEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, SuppressionMechanismEnabled,
									 value, InfoDiagnosticsEnabled);


		public CodeAnalysisSettings WithInfoDiagnosticsEnabled() =>
			WithInfoDiagnosticsEnabledValue(true);

		public CodeAnalysisSettings WithInfoDiagnosticsDisabled() => WithInfoDiagnosticsEnabledValue(false);

		protected virtual CodeAnalysisSettings WithInfoDiagnosticsEnabledValue(bool value) =>
			new CodeAnalysisSettings(RecursiveAnalysisEnabled, IsvSpecificAnalyzersEnabled, StaticAnalysisEnabled, SuppressionMechanismEnabled,
									 PX1007DocumentationDiagnosticEnabled, value);

		public override bool Equals(object obj) =>
			obj is CodeAnalysisSettings other && Equals(other);

		public bool Equals(CodeAnalysisSettings other) =>
			RecursiveAnalysisEnabled             == other?.RecursiveAnalysisEnabled &&
			IsvSpecificAnalyzersEnabled          == other.IsvSpecificAnalyzersEnabled &&
			StaticAnalysisEnabled                == other.StaticAnalysisEnabled &&
			SuppressionMechanismEnabled          == other.SuppressionMechanismEnabled &&
			PX1007DocumentationDiagnosticEnabled == other.PX1007DocumentationDiagnosticEnabled &&
			InfoDiagnosticsEnabled               == other.InfoDiagnosticsEnabled;

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + RecursiveAnalysisEnabled.GetHashCode();
				hash = 23 * hash + IsvSpecificAnalyzersEnabled.GetHashCode();
				hash = 23 * hash + StaticAnalysisEnabled.GetHashCode();
				hash = 23 * hash + SuppressionMechanismEnabled.GetHashCode();
				hash = 23 * hash + PX1007DocumentationDiagnosticEnabled.GetHashCode();
				hash = 23 * hash + InfoDiagnosticsEnabled.GetHashCode();
			}

			return hash;
		}
	}
}
