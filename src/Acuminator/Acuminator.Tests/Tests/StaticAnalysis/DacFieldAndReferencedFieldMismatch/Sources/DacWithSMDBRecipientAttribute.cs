using System;
using System.ComponentModel;
using PX.Api;
using PX.Data;
using PX.SM.Email;


namespace PX.SM.Email
{
	// Added attribute for tests since current Acumatica DLLs don't have it
	public class SMDBRecipientAttribute : PXDBStringAttribute
	{
		/// <summary>
		/// The length of the field that contains a single recipient.
		/// </summary>
		public const int SingleRecipientFieldLength = 500;

		/// <summary>
		/// The length of the field that contains multiple recipients.
		/// </summary>
		public const int MultipleRecipientsFieldLength = 3000;

		/// <summary>
		/// Initialize a new instance of the <see cref="SMDBRecipientAttribute"/> class.
		/// </summary>
		/// <param name="isMultiple">
		/// <see langword="true"/> if used for field that contains multiple recipients, such as To, Cc, Bcc.
		/// In that case <see cref="SingleRecipientFieldLength"/> is used as field length.
		/// Otherwise <see cref="MultipleRecipientsFieldLength"/>.
		/// </param>
		public SMDBRecipientAttribute(bool isMultiple = false) : base(isMultiple ? MultipleRecipientsFieldLength : SingleRecipientFieldLength)
		{
			IsUnicode = true;
		}
	}
}


namespace PX.Objects.HackathonDemo.DAC.InconsistentTypesOfDeclaredFieldAndReferencedDacFields
{
	[Serializable]
	[PXCacheName("Foreign Keys Container")]
	public class DacWithForeignKeys : PXBqlTable, IBqlTable
	{
		#region PaymentTermsListID
		[SMDBRecipient]
		[PXUIField(DisplayName = "Payment Terms")]
		[PXSelector(typeof(ForeignDac.paymentTermsListID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID { get; set; }

		public abstract class paymentTermsListID : PX.Data.BQL.BqlString.Field<paymentTermsListID> { }
		#endregion

		#region PaymentTermsListID2
		[SMDBRecipient(true)]
		[PXUIField(DisplayName = "Payment Terms Altn")]
		[PXSelector(typeof(ForeignDac.paymentTermsListID2))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID2 { get; set; }

		public abstract class paymentTermsListID2 : PX.Data.BQL.BqlString.Field<paymentTermsListID2> { }
		#endregion

		#region PaymentTermsListID4
		[SMDBRecipient(false)]
		[PXUIField(DisplayName = "Payment Terms Str Key")]
		[PXSelector(typeof(ForeignDac.paymentTermsListID4))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID4 { get; set; }

		public abstract class paymentTermsListID4 : PX.Data.BQL.BqlString.Field<paymentTermsListID4> { }
		#endregion

		#region Correct DAC fields
		#region PaymentTermsListID
		[SMDBRecipient(true)]
		[PXUIField(DisplayName = "Payment Terms")]
		[PXSelector(typeof(ForeignDac.paymentTermsListID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListIDCorrect { get; set; }

		public abstract class paymentTermsListIDCorrect : PX.Data.BQL.BqlString.Field<paymentTermsListIDCorrect> { }
		#endregion

		#region PaymentTermsListID2
		[SMDBRecipient]
		[PXUIField(DisplayName = "Payment Terms Altn")]
		[PXSelector(typeof(ForeignDac.paymentTermsListID2))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID2Correct { get; set; }

		public abstract class paymentTermsListID2Correct : PX.Data.BQL.BqlString.Field<paymentTermsListID2Correct> { }
		#endregion

		#region PaymentTermsListID4
		[SMDBRecipient(true)]
		[PXUIField(DisplayName = "Payment Terms Str Key")]
		[PXSelector(typeof(ForeignDac.paymentTermsListID4))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID4Correct { get; set; }

		public abstract class paymentTermsListID4Correct : PX.Data.BQL.BqlString.Field<paymentTermsListID4Correct> { }
		#endregion
		#endregion
	}


	[Serializable]
	[PXCacheName("Foreign Dac")]
	public class ForeignDac : PXBqlTable, IBqlTable
	{
		#region PaymentTermsListID
		/// <summary>
		/// Example initially found by commerce team. Field width should be 25.
		/// </summary>
		[SMDBRecipient(true)]
		[PXUIField(DisplayName = "Payment Terms")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID { get; set; }
		///<inheritdoc cref="PaymentTermsListID" />
		public abstract class paymentTermsListID : PX.Data.BQL.BqlString.Field<paymentTermsListID> { }
		#endregion

		#region PaymentTermsListID2
		/// <summary>
		/// Same as previous example, but with PXString. Should also trigger an error.
		/// </summary>
		[SMDBRecipient(false)]
		[PXUIField(DisplayName = "Payment Terms Altn")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID2 { get; set; }
		///<inheritdoc cref="PaymentTermsListID2" />
		public abstract class paymentTermsListID2 : PX.Data.BQL.BqlString.Field<paymentTermsListID2> { }
		#endregion

		#region PaymentTermsListID4
		/// <summary>
		/// Derived type with correct length, should trigger an error - analysis is complicated
		/// </summary>
		[SMDBRecipient(true)]
		[PXUIField(DisplayName = "Payment Terms Str Key")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID4 { get; set; }
		///<inheritdoc cref="PaymentTermsListID4" />
		public abstract class paymentTermsListID4 : PX.Data.BQL.BqlString.Field<paymentTermsListID4> { }
		#endregion
	}
}
