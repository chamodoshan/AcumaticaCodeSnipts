// Example of Graph extension with different events 
// This extension demonstrates hooking into various shipment row events (Updating, Updated, Persisting, Deleted) to manage freight estimates and tracking.
public class SOShipmentEntry_Extension : PXGraphExtension<PX.Objects.SO.SOShipmentEntry>
{
        protected bool TrackingActive = true;
        protected SOShipment CurrentShipment {  get; set; }
        protected SOShipmentExt CurrentShipmentExt { get; set; }
        protected SOOrderExt CurrentSOOrderExt { get; set; }
        protected SOOrder CurrentSOOrder { get; set; }
 
        protected bool isInternalUpdate = false;
        protected bool fetchedSO = false;
        protected bool mrkPriceUpdated = false;
        protected decimal? balance = 0m;
 
        SOFreightTrackingMaint sOFreightTrackingMaint = PXGraph.CreateInstance<SOFreightTrackingMaint>();
 
        private SOShipment shipmenttoDelete { get; set; }
 
        #region Event Handlers
 
        //Load the SO freight balance from SO to the Shipment 
        protected void SOShipment_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
 
            if(fetchedSO) return; // Prevent recursive updates
 
            SOShipment shipment = (SOShipment)e.Row;
            if (shipment == null) return;
 
            if (shipment.Status != SOShipmentStatus.Open && shipment.Status != SOShipmentStatus.Hold) return; 
            
            SOShipmentExt shipmentExt = shipment.GetExtension<SOShipmentExt>();
 
            SOOrderShipment soOrderShipment = PXSelect<SOOrderShipment,
                            Where<SOOrderShipment.shipmentNbr,
                            Equal<Required<SOOrderShipment.shipmentNbr>>>>
                            .Select(this.Base, shipment.ShipmentNbr)
                            .FirstOrDefault();
 
            if (soOrderShipment == null) return;
 
            SOOrder soOrder = PXSelect<SOOrder,
                            Where<SOOrder.orderNbr,
                            Equal<Required<SOOrder.orderNbr>>>>
                            .Select(this.Base, soOrderShipment.OrderNbr)
                            .FirstOrDefault();
 
            CurrentShipment = shipment;
            CurrentShipmentExt = shipmentExt;
 
            if (soOrder != null)
            {
                CurrentSOOrder = soOrder;
                SOOrderExt sOOrderExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(soOrder);
                CurrentSOOrderExt = sOOrderExt;
 
                SetFreighBalance(shipment, shipmentExt, sOOrderExt);
 
                PXTrace.WriteInformation("SO freight balance updated in Shipment.");
            }
    }
 
        //Calculate the Freight price and update when enter the freight cost in Shipping 
        protected void SOShipment_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if(mrkPriceUpdated) return; // Prevent recursive updates
           
                var cShipment = (SOShipment)e.Row;
                var oldShipment = (SOShipment)e.OldRow; 
                if (cShipment == null || oldShipment == null) return;
                CurrentShipment = cShipment;
 
                if (CurrentShipment.Status != SOShipmentStatus.Open && CurrentShipment.Status != SOShipmentStatus.Hold) return;
 
                var cShipmentExt = cShipment.GetExtension<SOShipmentExt>();
                CurrentShipmentExt = cShipmentExt;
 
                if (CurrentSOOrderExt != null && CurrentShipmentExt != null && CurrentShipment != null)
                {
                    //Get balance using SO freight
                    var blc = (CurrentSOOrderExt.UsrCuryFreightBlc ?? 0m) - (CurrentShipment.CuryFreightAmt ?? 0m);
                    if (blc == 0) return;
 
                    CurrentShipmentExt.UsrCurySoFreightBlc = CurrentSOOrderExt.UsrCuryFreightBlc;
 
                    //Only run when there is a change in Freight cost 
                    if (!Equals(cShipment.CuryFreightCost, oldShipment.CuryFreightCost))
                    {
                        CurrentShipmentExt.UsrCuryFreightMrkPrice = CurrentShipment.CuryFreightAmt;
                    }
 
                    mrkPriceUpdated = true; // Reset the flag after execution
 
                    UpdateFreightPriceTemp();
                
                }
           
    }
 
        //Save changes to DB when click Save
        protected void SOShipment_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            var cShipment = (SOShipment)e.Row;
            if (cShipment == null) return;
            CurrentShipment = cShipment;
 
            if (CurrentShipment.Status != SOShipmentStatus.Open && CurrentShipment.Status != SOShipmentStatus.Hold) return;
            PXTrace.WriteInformation($"SAVE - Ship staus continues - {CurrentShipment.Status}");
 
            var cShipmentExt = cShipment.GetExtension<SOShipmentExt>();
            CurrentShipmentExt = cShipmentExt;
 
            if (cShipment == null && cShipmentExt == null) return;
            if (CurrentShipment.CuryFreightCost <= 0) return; // prevent updating during save if there is no freight 
 
            if (isInternalUpdate) return; // Prevent recursive updates
 
            try
            {
                isInternalUpdate = true;
                
                UpdateFreightPrice();
 
                SaveToSOFreightTracker();
            }
            finally
            {
                isInternalUpdate = false; // Reset the flag after execution
            }
        }
 
        protected virtual void SOShipment_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            SOShipment cShipment = (SOShipment)e.Row;
            if (cShipment != null)
            {
                shipmenttoDelete = cShipment;
 
                SOOrderShipment soOrderShipment = PXSelect<SOOrderShipment,
                            Where<SOOrderShipment.shipmentNbr,
                            Equal<Required<SOOrderShipment.shipmentNbr>>>>
                            .Select(this.Base, cShipment.ShipmentNbr)
                            .FirstOrDefault();
 
                if (soOrderShipment != null)
                {
 
                    SOOrder soOrder = PXSelect<SOOrder,
                                Where<SOOrder.orderNbr,
                                Equal<Required<SOOrder.orderNbr>>>>
                                .Select(this.Base, soOrderShipment.OrderNbr)
                                .FirstOrDefault();
 
                    CurrentSOOrder = soOrder;
                }
            }
 
        }
 
        //SO order save - Save to SOFReightTRacking
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (shipmenttoDelete != null && CurrentSOOrder != null)
            {
                sOFreightTrackingMaint.deleteRecordSH(CurrentSOOrder, shipmenttoDelete);
                shipmenttoDelete = null;
            }
 
            // Execute the base persist method to save all changes
            baseMethod();
        }
 
        #endregion
 
        #region Override
        // Override the ConfirmShipment method
        public delegate IEnumerable ConfirmShipmentActionDelegate(PXAdapter adapter);
        [PXOverride]
        public IEnumerable ConfirmShipmentAction(PXAdapter adapter, ConfirmShipmentActionDelegate baseMethod)
        {
            // Get the current shipment
            SOShipment shipment = Base.Document.Current;
            if (shipment != null)
            {
                if (shipment.Status == SOShipmentStatus.Open || shipment.Status == SOShipmentStatus.Hold)
                {
                    PXTrace.WriteInformation($"CONFIRM - Ship staus continues - {shipment.Status}");
                    // Retrieve the related SOOrder using SOOrderShipment
                    SOOrderShipment orderShipment = PXSelect<SOOrderShipment,
                        Where<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>
                        .Select(Base, shipment.ShipmentNbr);
 
                    if (orderShipment != null)
                    {
                        // Get the SOOrder associated with the shipment
                        SOOrder order = PXSelect<SOOrder,
                            Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                            And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                            .Select(Base, orderShipment.OrderType, orderShipment.OrderNbr);
 
                        if (order != null)
                        {
                            // Access extended fields or make updates to the SOOrder
                            SOOrderExt orderExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(order);
 
                            // Access extended fields or make updates to the SOShipment
                            SOShipmentExt shipmentExt = PXCache<SOShipment>.GetExtension<SOShipmentExt>(shipment);
 
                            if (shipment.ShipmentQty == shipment.ControlQty)
                            {
 
                                if (shipment.CuryFreightAmt > orderExt.UsrCuryFreightBlc)
                                {
 
                                    // Log the error using PXTrace to create a side panel notification
                                    PXTrace.WriteError(FreightCustomization.Messages.NotAvialableFreightBlc);
                                    
                                    //Throw an exception to prevent confirmation and show an error message
                                    throw new PXException(FreightCustomization.Messages.NotAvialableFreightBlc);
 
 
                                }
                                else
                                {
                                    //Update SO freight balance
                                    if (shipmentExt != null && orderExt != null)
                                    {
                                        CurrentSOOrder = order;
                                        CurrentSOOrderExt = orderExt;
                                        CurrentShipment = shipment;
                                        CurrentShipmentExt = shipmentExt;
 
                                        decimal? previousFreightBalance = orderExt.UsrCuryFreightBlc;
                                        decimal? currentFreightBalance = shipmentExt.UsrCurySoFreightBlc;
 
                                        if (previousFreightBalance != currentFreightBalance)
                                        {
                                            PXTrace.WriteInformation($"FROMSO  - freight balance - {orderExt.UsrCuryFreightBlc}");
                                            PXTrace.WriteInformation($"SAVEtoSO  - freight balance - {shipmentExt.UsrCurySoFreightBlc}");
                                         
                                            SaveFreightBalanceToSO(); // Only update if there’s a difference
                                            fetchedSO = true;
                                            mrkPriceUpdated = true;
                                            isInternalUpdate = true;
                                        }
                                    }
 
                                    // Update SO freight tracker 
                                    
                                    SaveToSOFreightTracker();
                                }
 
                                PXTrace.WriteInformation($"SOOrder {order.OrderNbr} updated during shipment confirmation.");
                            }
                            else
                            {
                                PXTrace.WriteInformation($"SOOrder {order.OrderNbr} not updated due to contral qty error");
                            }
 
 
 
                        }
                    }
                }
            }
 
            // Continue with the base ConfirmShipment process
            return baseMethod(adapter);
        }
        #endregion
 
        #region Extra Methods
        //setting freight balance logic from 
        public void SetFreighBalance(SOShipment shipment, SOShipmentExt shipmentExt,SOOrderExt sOOrderExt)
        {
            if (shipment.CuryFreightCost > 0)
            {
                if (sOOrderExt.UsrCuryFreightBlc != null)
                {
                    shipmentExt.UsrCurySoFreightBlc = sOOrderExt.UsrCuryFreightBlc - shipment.CuryFreightAmt;
                    fetchedSO = true;
                }
                else
                {
                    shipmentExt.UsrCurySoFreightBlc = (decimal?)0.00 + shipment.CuryFreightAmt;
                    fetchedSO = true;
                }
                
            }
            else
            {
                if (sOOrderExt.UsrCuryFreightBlc != null)
                {
                    shipmentExt.UsrCurySoFreightBlc = sOOrderExt.UsrCuryFreightBlc;
                    fetchedSO = true;
                }
                else
                {
                    shipmentExt.UsrCurySoFreightBlc = (decimal?)0.00;
                    fetchedSO = true;
                }
            }
        }
 
        //Temp freight calculation and update
        public void UpdateFreightPriceTemp()
        {
            //update the existing freight price with the logic
            if (CurrentShipmentExt != null && CurrentShipment != null)
            {
                balance = (CurrentShipmentExt.UsrCurySoFreightBlc ?? 0m) - (CurrentShipment.CuryFreightAmt ?? 0m);
                if (balance == 0) return;
                if (balance <= 0)
                {
                    CurrentShipment.CuryFreightAmt = (CurrentShipment.CuryFreightAmt ?? 0m) + balance;
                    CurrentShipmentExt.UsrCurySoFreightBlc = 0.0000m;
                    CurrentShipmentExt.UsrCuryFreightMrkPrice = (CurrentShipment.CuryFreightAmt ?? 0m) - balance;
                }
                else
                {
                    CurrentShipmentExt.UsrCurySoFreightBlc = balance;
                }
            }
        }
 
        //Freight calculation and update 
        public void UpdateFreightPrice()
        {
            //update the existing freight price with the logic
            if (CurrentShipmentExt != null && CurrentShipment != null)
            {
                balance = (CurrentShipmentExt.UsrCurySoFreightBlc ?? 0m) - (CurrentShipment.CuryFreightAmt ?? 0m);
                if(balance == 0) return;
                if (balance < 0)
                {
                    CurrentShipment.CuryFreightAmt = (CurrentShipment.CuryFreightAmt ?? 0m); 
                    CurrentShipmentExt.UsrCurySoFreightBlc = 0.0000m;
                }
                else
                {
                    CurrentShipmentExt.UsrCurySoFreightBlc = balance;
                }
 
                // Update caches to ensure changes are recognized by Acumatica
                PXCache shipmentCache = Base.Caches<SOShipment>();
 
                // Mark the record as updated so Acumatica knows it was changed
                shipmentCache.MarkUpdated(CurrentShipment);
                shipmentCache.Update(CurrentShipment);
 
                PXTrace.WriteInformation("Updated SO Freight Price");
            }
        }
 
        // Save to SO
        public void SaveFreightBalanceToSO()
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                // Reload SOOrder to ensure we have the latest version before updating
                SOOrder refreshedOrder = PXSelect<SOOrder,
                Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                .Select(Base, CurrentSOOrder.OrderType, CurrentSOOrder.OrderNbr)
                .FirstOrDefault();
 
                if (refreshedOrder == null) return;
 
                CurrentSOOrder = refreshedOrder; // Update with the latest version
                CurrentSOOrderExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(CurrentSOOrder);
 
                if (CurrentSOOrder != null && CurrentSOOrderExt != null)
                {
                    CurrentSOOrderExt.UsrCuryFreightBlc = CurrentShipmentExt.UsrCurySoFreightBlc;
 
                    // Update the cache to reflect changes in Acumatica
                    this.Base.Caches<SOOrder>().Update(CurrentSOOrder);
 
                    // Persist if not in a transactional save
                    this.Base.Caches<SOOrder>().Persist(PXDBOperation.Update);
 
                    // Clears the cache after persisting
                    //this.Base.Caches<SOOrder>().Persisted(false);
 
 
                    PXTrace.WriteInformation("Updated SO Freight Balance to the latest: " + CurrentSOOrderExt.UsrCuryFreightBlc);
                }
                ts.Complete();
            }
 
        }
 
        //Save to SOFReightTRacking
        public void SaveToSOFreightTracker()
        {
            if (sOFreightTrackingMaint != null && CurrentSOOrder != null && CurrentSOOrderExt !=null && CurrentShipment != null && CurrentShipmentExt != null && TrackingActive == true)
            {
                sOFreightTrackingMaint.updateSOFreightTrackerSH(CurrentSOOrder, CurrentSOOrderExt, CurrentShipment,CurrentShipmentExt);
 
                PXTrace.WriteInformation("Updated SO Freight Tracker");
            }
          
        }
 
        #endregion
}
