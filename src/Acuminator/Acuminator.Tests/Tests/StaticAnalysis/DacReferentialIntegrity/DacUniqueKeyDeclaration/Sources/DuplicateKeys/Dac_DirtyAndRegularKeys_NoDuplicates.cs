#nullable disable
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.Sources
{
	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	/// <exclude/>
	[PXCacheName("SO Order")]
	public class SOOrder : PXBqlTable, IBqlTable
	{
		public class PK : PrimaryKeyOf<SOOrder>.By<orderNbr>
		{
			public static SOOrder Find(PXGraph graph, string orderNbr) => FindBy(graph, orderNbr);
		}

		public class PK_Dirty : PrimaryKeyOf<SOOrder>.By<orderNbr>.Dirty
		{
			public static SOOrder Find(PXGraph graph, string orderNbr) => FindBy(graph, orderNbr);
		}

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		public abstract class orderNbr : IBqlField { }

		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXDBString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		public abstract class status : IBqlField { }

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		public abstract class Tstamp : IBqlField { }
	}
}
