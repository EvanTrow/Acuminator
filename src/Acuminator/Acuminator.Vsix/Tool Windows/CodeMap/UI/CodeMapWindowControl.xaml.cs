#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interaction logic for CodeMapWindowControl.
	/// </summary>
	public partial class CodeMapWindowControl : UserControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CodeMapWindowControl"/> class.
		/// </summary>
		public CodeMapWindowControl()
		{
			this.InitializeComponent();
		}

		private void CodeMapImage_Initialized(object sender, EventArgs e) =>
			DpiUtils.SetReverseDpiTransformationForImage(sender as Image);

		private void CodeMapImage_DpiChanged(object sender, DpiChangedEventArgs e)
		{
			if (sender is Image image &&
				(e.OldDpi.DpiScaleX != e.NewDpi.DpiScaleX || e.OldDpi.DpiScaleY != e.NewDpi.DpiScaleY))
			{
				DpiUtils.SetReverseDpiTransformationForImage(image);
			}
		}
	}
}