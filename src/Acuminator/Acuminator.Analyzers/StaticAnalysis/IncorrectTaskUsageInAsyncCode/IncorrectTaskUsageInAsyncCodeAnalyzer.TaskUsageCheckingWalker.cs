using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.IncorrectTaskUsageInAsyncCode
{
	public partial class IncorrectTaskUsageInAsyncCodeAnalyzer : PXDiagnosticAnalyzer
	{
		private class TaskUsageCheckingWalker : CSharpSyntaxWalker 
		{
			private readonly SyntaxNodeAnalysisContext _syntaxContext;
			private readonly PXContext _pxContext;

			public TaskUsageCheckingWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
			{
				_syntaxContext = syntaxContext;
				_pxContext	   = pxContext;
			}
		}
	}
}