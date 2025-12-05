using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.Shared.Extensions;

public static class ExtensionUtils
{
	private const int MaxIterationCount = 10_000;

	public static IEnumerable<TExtensionInfo> GetAllBaseExtensionInfosAndThisBFS<TExtensionInfo>(this TExtensionInfo extensionInfo)
	where TExtensionInfo : class, IExtensionInfo<TExtensionInfo> =>
		GetBaseExtensionInfosAndThisBFS(extensionInfo.CheckIfNull(), includeSelf: true);


	public static IEnumerable<TExtensionInfo> GetAllBaseExtensionInfosBFS<TExtensionInfo>(this TExtensionInfo extensionInfo)
	where TExtensionInfo : class, IExtensionInfo<TExtensionInfo> =>
		GetBaseExtensionInfosAndThisBFS(extensionInfo.CheckIfNull(), includeSelf: false);

	private static IEnumerable<TExtensionInfo> GetBaseExtensionInfosAndThisBFS<TExtensionInfo>(TExtensionInfo extensionInfo,
																								bool includeSelf)
	where TExtensionInfo : class, IExtensionInfo<TExtensionInfo>
	{
		if (includeSelf)
			yield return extensionInfo;

		var baseExtensions = extensionInfo.BaseExtensions;

		if (baseExtensions.IsDefaultOrEmpty)
			yield break;

		// Use breadth first traversal to get level by level extensions + add hard guard on iterations count against infinite loops
		int iterationCount = 0;
		var queue = new Queue<TExtensionInfo>(baseExtensions);

		while (queue.Count > 0 && iterationCount < MaxIterationCount)
		{
			iterationCount++;
			var baseOrChainedGraphExtension = queue.Dequeue();
			yield return baseOrChainedGraphExtension;

			var baseExtensionsOfBaseExtension = baseOrChainedGraphExtension.BaseExtensions;

			if (baseExtensionsOfBaseExtension.IsDefaultOrEmpty)
				continue;

			foreach (var descendantGraphExtension in baseExtensionsOfBaseExtension)
			{
				queue.Enqueue(descendantGraphExtension);
			}
		}
	}

	public static IEnumerable<TExtensionInfo> GetAllBaseExtensionInfosAndThisDFS<TExtensionInfo>(this TExtensionInfo extensionInfo)
	where TExtensionInfo : class, IExtensionInfo<TExtensionInfo> =>
		GetBaseExtensionInfosAndThisDFS(extensionInfo.CheckIfNull(), includeSelf: true);


	public static IEnumerable<TExtensionInfo> GetAllBaseExtensionInfosDFS<TExtensionInfo>(this TExtensionInfo extensionInfo)
	where TExtensionInfo : class, IExtensionInfo<TExtensionInfo> =>
		GetBaseExtensionInfosAndThisDFS(extensionInfo.CheckIfNull(), includeSelf: false);

	private static IEnumerable<TExtensionInfo> GetBaseExtensionInfosAndThisDFS<TExtensionInfo>(TExtensionInfo extensionInfo,
																								bool includeSelf)
	where TExtensionInfo : class, IExtensionInfo<TExtensionInfo>
	{
		if (includeSelf)
			yield return extensionInfo;

		var baseExtensions = extensionInfo.BaseExtensions;

		if (baseExtensions.IsDefaultOrEmpty)
			yield break;

		// Use depth first traversal to get level by level extensions + add hard guard on iterations count against infinite loops
		int iterationCount = 0;
		var stack = new Stack<TExtensionInfo>(baseExtensions);

		while (stack.Count > 0 && iterationCount < MaxIterationCount)
		{
			iterationCount++;
			var baseOrChainedGraphExtension = stack.Pop();
			yield return baseOrChainedGraphExtension;

			var baseExtensionsOfBaseExtension = baseOrChainedGraphExtension.BaseExtensions;

			if (baseExtensionsOfBaseExtension.IsDefaultOrEmpty)
				continue;

			foreach (var descendantGraphExtension in baseExtensionsOfBaseExtension)
			{
				stack.Push(descendantGraphExtension);
			}
		}
	}
}