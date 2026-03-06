// Update or modify a Selector 
// With attributes – can be used in multiple classes / screens if needed
public class SOLineExt : PXCacheExtension<PX.Objects.SO.SOLine>
{
    #region InventoryID
    [PXMergeAttributes(Method = MergeMethod.Append)]
    [AlternateIdAttribute]
    [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
    public virtual int? InventoryID { get; set; }
    #endregion  
}

// AlternateIdAttribute custom selector with complex joins and returned columns
public class AlternateIdAttribute : PXCustomSelectorAttribute
{
    public AlternateIdAttribute()
:       base(typeof(Search2<InventoryItem.inventoryID,
             LeftJoin<INItemXRef,
                 On<INItemXRef.inventoryID, Equal<InventoryItem.inventoryID>>>,
 
             Where<InventoryItem.stkItem, Equal<True>>>),
      typeof(InventoryItem.inventoryID),   // Inventory Item ID
      typeof(InventoryItem.inventoryCD),   // Inventory Item ID
      typeof(InventoryItem.descr),         // Inventory Description
      typeof(InventoryItem.itemClassID),         
      typeof(InventoryItem.itemStatus),         
      typeof(InventoryItem.itemType),         
      typeof(InventoryItem.baseUnit),      // Base Unit
      typeof(InventoryItem.salesUnit),      
      typeof(InventoryItem.purchaseUnit),      
      typeof(InventoryItem.basePrice),      
      typeof(InventoryItem.commodityCodeType),      
      typeof(InventoryItem.exportToExternal),      
      typeof(INItemXRef.alternateID))      // Alternate ID
          
    {
        SubstituteKey = typeof(InventoryItem.inventoryCD); // Specify the key field for the selector
        DescriptionField = typeof(InventoryItem.descr);    // Specify the description field for the selector
    }

    protected virtual IEnumerable GetRecords()
    {
        PXGraph graph = new PXGraph();
 
        foreach (PXResult<InventoryItem, INItemXRef> result in PXSelectJoin<InventoryItem,
            LeftJoin<INItemXRef,
                On<INItemXRef.inventoryID, Equal<InventoryItem.inventoryID>>>,
            Where<InventoryItem.stkItem, Equal<True>>>.Select(graph))
        {
            InventoryItem item = result;
            INItemXRef xRef = result;
 
            // Combine data as needed or return directly
            PXTrace.WriteInformation($"Item: {item.InventoryCD}, Alternate ID: {xRef.AlternateID}");
            yield return new PXResult<InventoryItem, INItemXRef>(item, xRef);
        }
    }
}

// Without Attributer and having restrictions 
// DAC Extension to add PXRestrictor and Inventory Attribute directly
public class SOLineExt2 : PXCacheExtension<PX.Objects.SO.SOLine>
{
     #region InventoryID
     [Inventory(Visibility = PXUIVisibility.SelectorVisible,
         DisplayName = "Inventory ID",
         BqlField = typeof(InventoryItem.inventoryID))]
     [PXRestrictor(typeof(Where<InventoryItem.stkItem, Equal<True>,
         Or<InventoryItem.nonStockReceipt, Equal<True>,
         Or<InventoryItem.nonStockShip, Equal<True>>>>),
         PX.Objects.IN.Messages.InventoryItemIsInStatus,
         typeof(InventoryItem.inventoryCD),
         typeof(Search<INItemXRef.alternateID,
              Where<INItemXRef.inventoryID, Equal<InventoryItem.inventoryID>>>)
         )]
     public virtual int? InventoryID { get; set; }
     public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
     #endregion
}

// Internet example with custom columns – didn’t check
/*
[PXCustomizeSelectorColumns( typeof(INLotSerialStatus.lotSerialNbr), typeof(INLotSerialStatus.siteID), typeof(INLotSerialStatus.locationID), typeof(INLotSerialStatus.qtyOnHand), typeof(INLotSerialStatus.qtyAvail), typeof(INLotSerialStatus.expireDate))] [PXSelector( typeof(Search2<INLotSerialStatus.lotSerialNbr, LeftJoin<INLocation, On<INLotSerialStatus.locationID, Equal<INLocation.locationID>>, LeftJoin<INSiteLotSerial, On<INLotSerialStatus.inventoryID, Equal<INSiteLotSerial.inventoryID>, And<INLotSerialStatus.siteID, Equal<INSiteLotSerial.siteID>, And<INLotSerialStatus.lotSerialNbr, Equal<INSiteLotSerial.lotSerialNbr>>>>>>, Where<INLotSerialStatus.inventoryID, Equal<Current<SOLineSplit.inventoryID>>, And<INLotSerialStatus.siteID, Equal<Current<SOLineSplit.siteID>>, And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>, OrderBy<Asc<INLotSerialStatus.lotSerialNbr, Asc<INLotSerialStatus.siteID, Asc<INLotSerialStatus.locationID>>>>>), typeof(INLotSerialStatus.lotSerialNbr), typeof(INLotSerialStatus.siteID), typeof(INLotSerialStatus.locationID), typeof(INLotSerialStatus.qtyOnHand), typeof(INLotSerialStatus.qtyAvail), typeof(INLotSerialStatus.expireDate))] 
*/
