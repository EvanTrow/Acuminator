#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Settings
{
	[Export(typeof(CodeMapSettings))]
	internal class CodeMapSettingsFromOptionsPage : CodeMapSettings
	{
		private readonly CodeMapOptionsPage _optionsPage;

		[ImportingConstructor]
		public CodeMapSettingsFromOptionsPage(CodeMapOptionsPage optionsPage)
		{
			_optionsPage = optionsPage.CheckIfNull();
		}

		public override bool ExpandRegularNodes => _optionsPage.ExpandRegularNodes;

		public override bool ExpandAttributeNodes => _optionsPage.ExpandAttributeNodes;
	}
}
