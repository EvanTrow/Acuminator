namespace Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

/// <summary>
/// Type of the graph event handler override.
/// </summary>
public enum GraphEventHandlerOverrideType : byte
{
	/// <summary>
	/// Handler is not an override.
	/// </summary>
	None,

	/// <summary>
	/// Handler is a C# override.
	/// </summary>
	CSharp,

	/// <summary>
	/// Handler is an override implemented with Acumatica events override mechanism with interceptors.
	/// See <![CDATA[https://help.acumatica.com/Help?ScreenId=ShowWiki&pageid=d316d773-1548-4a23-b542-a4bf7aa4ecc6]]>
	/// </summary>
	OverrideWithInterceptor,

	/// <summary>
	/// Handler is an override implemented with Acumatica PXOverride mechanism. 
	/// See <![CDATA[https://help.acumatica.com/Help?ScreenId=ShowWiki&pageid=635c830e-4617-4d5c-9fa5-035952311aa9]]>
	/// </summary>
	OverrrideWithPXOverrideAttribute
}
