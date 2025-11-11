using PX.Data;
using PX.SM;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidViewUsageInProcessingDelegate.Sources
{
	public class UsersProcess : PXGraph<UsersProcess>
	{
		public PXCancel<Users> Cancel = null!;

		public PXProcessing<Users, Where<Users.guest, Equal<False>>> OurUsers = null!;

		public PXSetup<BlobProviderSettings> BolbSettings = null!;

		public PXSelect<Users> AllUsers = null!;

		[SuppressMessage("Acuminator", "PX1050:Hardcoded strings are not allowed as parameters for localization methods and PXException constructors. " +
						 "You should use string constants from the appropriate localizable messages class.", Justification = "<Pending>")]
		public UsersProcess()
		{
			OurUsers.SetProcessAllCaption("Process users");
			OurUsers.SetProcessCaption("Process user");

			OurUsers.SetParametersDelegate(ProcessParameters);
			OurUsers.SetParametersDelegate(delegate (List<Users> users) {
				Console.WriteLine("Users parameters processing");

				var processingGraph = PXGraph.CreateInstance<UsersProcess>();
				var result = processingGraph.AllUsers.Select().Count > 0;

				if (!result)
				{
					throw new PXSetupNotEnteredException<Users>(null);
				}

				return result;
			});

			OurUsers.SetParametersDelegate(users =>
			{
				Console.WriteLine("Users parameters processing");

				var processingGraph = PXGraph.CreateInstance<UsersProcess>();
				var result = processingGraph.AllUsers.Select().Count > 0;

				if (!result)
				{
					throw new PXSetupNotEnteredException<Users>(null);
				}

				return result;
			});

			OurUsers.SetParametersDelegate(users => throw new PXSetupNotEnteredException<Users>(null));
			OurUsers.SetParametersDelegate(users => Helper.ThrowPXSetupNotEnteredException());
		}

		[SuppressMessage("Acuminator", "PX1050:Hardcoded strings are not allowed as parameters for localization methods and PXException constructors. " +
						 "You should use string constants from the appropriate localizable messages class.", Justification = "<Pending>")]
		private static bool ProcessParameters(List<Users> users)
		{
			Console.WriteLine("Users parameters processing");

			var processingGraph = PXGraph.CreateInstance<UsersProcess>();
			var result = processingGraph.AllUsers.Select().Count > 0;

			if (!result)
			{
				throw new PXSetupNotEnteredException<Users>(null);
			}

			return result;
		}
	}

	public static class Helper
	{
		// Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
		public static bool ThrowPXSetupNotEnteredException() => throw new PXSetupNotEnteredException<Users>(null);
	}
}
