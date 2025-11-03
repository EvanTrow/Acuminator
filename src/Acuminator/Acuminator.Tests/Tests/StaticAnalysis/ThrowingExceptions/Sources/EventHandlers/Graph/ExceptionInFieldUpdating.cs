using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		public PXSelect<SOInvoice> Invoices;

		public int Mode { get; set; }

		protected virtual void _(Events.FieldUpdating<SOInvoice, SOInvoice.refNbr> e)
		{
			switch (Mode)
			{
				case 0:
					throw new PXRowPersistedException(typeof(SOInvoice.refNbr).Name, e.Row.RefNbr, "Persist error");               //Should report diagnostic
				case 1:
					throw new PXLockViolationException(typeof(SOInvoice), PXDBOperation.Insert, new object[] { e.Row.RefNbr });    //Should report diagnostic
				case 2:
					throw new PXException("Something bad happened");        //Should report diagnostic
				case 3:
					throw new ArgumentOutOfRangeException(nameof(Mode));    //No diagnostic
				case 4:
					throw new ArgumentNullException(nameof(Mode));			//No diagnostic
				case 5:
					throw new ArgumentException("Something bad happened");  //No diagnostic
				case 6:
					throw new NotImplementedException();                    //No diagnostic
				default:
					throw new NotSupportedException();                      //No diagnostic
			}	
		}

		protected virtual void _(Events.FieldUpdating<SOLine.refNbr> e)
		{
			switch (Mode)
			{
				case 0:
					throw new PXRowPersistedException(typeof(SOInvoice.refNbr).Name, (e.Row as SOLine).RefNbr, "Persist error");               //Should report diagnostic
				case 1:
					throw new PXLockViolationException(typeof(SOInvoice), PXDBOperation.Insert, new object[] { (e.Row as SOLine).RefNbr });    //Should report diagnostic
				case 2:
					throw new PXException("Something bad happened");        //Should report diagnostic
				case 3:
					throw new ArgumentOutOfRangeException(nameof(Mode));    //No diagnostic
				case 4:
					throw new ArgumentNullException(nameof(Mode));          //No diagnostic
				case 5:
					throw new ArgumentException("Something bad happened");  //No diagnostic
				case 6:
					throw new NotImplementedException();                    //No diagnostic
				default:
					throw new NotSupportedException();                      //No diagnostic
			}
		}
	}

	public class SOInvoice : PXBqlTable, IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }

		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion

		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntr
		{
			get;
			set;
		}
		#endregion
	}

	public class SOLine : PXBqlTable, IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }

		public abstract class refNbr : IBqlField { }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOInvoice.lineCntr))]
		public int? LineNbr { get; set; }

		public abstract class lineNbr : IBqlField { }
		#endregion
	}
}