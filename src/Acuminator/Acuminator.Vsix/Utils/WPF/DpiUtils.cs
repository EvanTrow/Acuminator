#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using Microsoft.VisualStudio.Utilities;

namespace Acuminator.Vsix.Utilities;

/// <summary>
/// A static helper for the work with DPI related things.
/// </summary>
internal static class DpiUtils
{
	/// <summary>
	/// Sets reverse DPI transformation for image.
	/// </summary>
	/// <param name="image">The image.</param>
	/// <remarks>
	/// This helper is required to support screens with high DPI that can apply DPI transformation to the entire WPF application.
	/// Without such transformation the images can appear blurry or badly scaled.
	/// </remarks>
	public static void SetReverseDpiTransformationForImage(Image? image)
	{
		if (image == null)
			return;

		var dpiScaleX = DpiAwareness.GetDpiXScale(image);
		var dpiScaleY = DpiAwareness.GetDpiYScale(image);

		if (dpiScaleX == 0 || dpiScaleY == 0)
			return;

		BitmapScalingMode scalingMode;

		if (dpiScaleX == 1.0 && dpiScaleY == 1.0)
		{
			image.RenderTransform = null;
			image.RenderTransformOrigin = default;
			scalingMode = BitmapScalingMode.HighQuality;
		}
		else
		{
			var scaleTransform = new ScaleTransform(1.0 / dpiScaleX, 1.0 / dpiScaleY, 0.5, 0.5);
			scalingMode = dpiScaleX >= 2.0
				? BitmapScalingMode.NearestNeighbor
				: BitmapScalingMode.HighQuality;

			RenderOptions.SetBitmapScalingMode(scaleTransform, scalingMode);

			image.RenderTransform = scaleTransform;
			image.RenderTransformOrigin = new Point(0.5, 0.5); // The (0.5, 0.5) parameters center the transformation origin.
		}

		RenderOptions.SetBitmapScalingMode(image, scalingMode);
	}
}
