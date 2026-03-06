// Message/ Constants class 
// Standard format for Acumatica localizable messages to be used across the customizations.
[PXLocalizable("Freight Error")]
public static class Messages
{
     //Exceptions
     public const string NotAvialableFreightBlc = "Freight Amount is greater than availble SO freight balance. Please add 0 as the freight cost to see the available freight balance!";
     public const string FreightCostError = "Set Freight Cost to 0.00 to see available Freight Balance.";
     public const string FreightBlcErrorInvoice = "Freight balance for the project and added customer PO is not enough to fulfil the freight total!";
     public const string NOOrdersWithFreightBalance = "No Orders with Freight balance for the project anf the PO!";
     public const string NOProjectId = "Data Error - Project id not found for the Sales order under the entered Shipment ID !";
     public const string NOOrdersForShipment = "No Orders for the mentioned shipment id in the description!";
     public const string NORequiredData = "Required information is not updated. Please check Invoice total, Description or the Customer PO Number!";
     public const string CustomerPORequired = "Customer PO is required to create a Freight Invoice.";
     public const string DescriptionRequired = "Shipment number as the Description is required to create a Freight Invoice.";
     public const string TotalRequired = "Total is required to create a Freight Invoice.";
}
