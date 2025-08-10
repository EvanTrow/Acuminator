
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal readonly struct XmlCommentTagsInfo
	{
		public List<XmlNodeSyntax>? SummaryTags { get; }

		public List<InheritdocTagInfo>? InheritdocTagInfos { get; }

		public List<XmlNodeSyntax>? ExcludeTags { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(SummaryTags))]
		public bool HasSummaryTag => SummaryTags?.Count > 0;

		[MemberNotNullWhen(returnValue: true, member: nameof(InheritdocTagInfos))]
		public bool HasInheritdocTag => InheritdocTagInfos?.Count > 0;

		[MemberNotNullWhen(returnValue: true, member: nameof(ExcludeTags))]
		public bool HasExcludeTag => ExcludeTags?.Count > 0;

		public bool NoXmlComments => !HasExcludeTag && !HasSummaryTag && !HasInheritdocTag;

		public XmlCommentTagsInfo(List<XmlNodeSyntax>? summaryTags, List<XmlNodeSyntax>? inheritdocTags, List<XmlNodeSyntax>? excludeTags)
		{
			SummaryTags 	   = summaryTags;
			InheritdocTagInfos = inheritdocTags?.Select(inheritdocTag => new InheritdocTagInfo(inheritdocTag)).ToList(capacity: inheritdocTags.Count);
			ExcludeTags 	   = excludeTags;
		}

		public IEnumerable<XmlNodeSyntax> GetTagNodes(bool includeSummaryTag, bool includeInheritdocTag, bool includeExcludeTag)
		{
			if (NoXmlComments || (!includeSummaryTag && !includeInheritdocTag && !includeExcludeTag))
				return [];

			IEnumerable<XmlNodeSyntax> tags = HasSummaryTag && includeSummaryTag
				? SummaryTags
				: [];

			if (HasInheritdocTag && includeInheritdocTag)
			{
				var inheritdocTags = InheritdocTagInfos.Where(info => info.HasInheritdocTag)
													   .Select(info => info.Tag!);
				tags = tags.Concat(inheritdocTags);
			}

			if (HasExcludeTag && includeExcludeTag)
				tags = tags.Concat(ExcludeTags);

			return tags;
		}
	}
}