using System;
using System.Composition;

namespace Acuminator.Utilities;

[Export]
public class CodeMapSettings : IEquatable<CodeMapSettings>
{
	public const bool DefaultExpandRegularNodes   = true;
	public const bool DefaultExpandAttributeNodes = false;

	public static CodeMapSettings Default { get; } = new(DefaultExpandRegularNodes, DefaultExpandAttributeNodes);

	public virtual bool ExpandRegularNodes { get; }

	public virtual bool ExpandAttributeNodes { get; }

	public CodeMapSettings(bool expandRegularNodes, bool expandAttributeNodes)
	{
		ExpandRegularNodes   = expandRegularNodes;
		ExpandAttributeNodes = expandAttributeNodes;
	}

	protected CodeMapSettings()
	{
	}

	public CodeMapSettings WithExpandRegularNodesEnabled() => WithExpandRegularNodesValue(true);

	public CodeMapSettings WithExpandRegularNodesDisabled() => WithExpandRegularNodesValue(false);

	protected CodeMapSettings WithExpandRegularNodesValue(bool value) =>
		new(value, ExpandAttributeNodes);

	public CodeMapSettings WithExpandAttributeNodesEnabled() => WithExpandAttributeNodesValue(true);

	public CodeMapSettings WithExpandAttributeNodesDisabled() => WithExpandAttributeNodesValue(false);

	protected CodeMapSettings WithExpandAttributeNodesValue(bool value) =>
		new(ExpandRegularNodes, value);

	public override bool Equals(object obj) => Equals(obj as CodeMapSettings);

	public bool Equals(CodeMapSettings? other) => 
		other != null && ExpandRegularNodes == other.ExpandRegularNodes && ExpandAttributeNodes == other.ExpandAttributeNodes;

	public override int GetHashCode()
	{
		int hash = 17;

		unchecked
		{
			hash = 23 * hash + ExpandRegularNodes.GetHashCode();
			hash = 23 * hash + ExpandAttributeNodes.GetHashCode();
		}

		return hash;
	}
}
