// DB updates/delete and insert
// Shows creating, updating, isolating and persisting custom objects manually in a graph.
public class SOFreightTrackingMaint : PXGraph<SOFreightTrackingMaint>
{
 
  protected SOFreightTracking CurrentSOFreightTracking { get; set; }
  protected SOFreightTracking MainSOFreightTracking { get; set; }
  
  //fetching the existing record - SO
  public void updateSOFreightTrackerSO(SOOrder order,SOOrderExt orderExt) {
 
          if (string.IsNullOrEmpty(order.OrderNbr) || order.OrderNbr.Contains("NEW"))
          {
              PXTrace.WriteInformation("OrderNbr is NEW or OrderNbr is null, skipping update.");
              return;
          }
 
          var record = PXSelect<SOFreightTracking,
                  Where<SOFreightTracking.sOOrderNbr,
                  Equal<Required<SOFreightTracking.sOOrderNbr>>>>
                  .Select(this, order.OrderNbr)
                  .FirstOrDefault();
 
 
          if (record != null)
          {
              CurrentSOFreightTracking = record;
              if (orderExt != null)
              {
                  updateExistingRecordSO(order, orderExt);
              }
                
          }
          else
          {
              if (orderExt != null)
              {
                  addRecordSO(order, orderExt);
              }
          }
      }
 
  //update the existing record - SO
  public void updateExistingRecordSO(SOOrder order,SOOrderExt orderExt) {
          if (CurrentSOFreightTracking != null)
          {
              CurrentSOFreightTracking.ProjectID = order.ProjectID;
              //CurrentSOFreightTracking.SOOrderNbr = order.OrderNbr;
              CurrentSOFreightTracking.Ponbr = order.CustomerOrderNbr;
              CurrentSOFreightTracking.SOFreightEst = orderExt.UsrCuryFreightEst;
              CurrentSOFreightTracking.SOFreightBlc = orderExt.UsrCuryFreightBlc;
 
              PXTrace.WriteInformation("Updating SOFreightTracking record for OrderNbr: " + CurrentSOFreightTracking.SOOrderNbr);
 
              // Update the cache to reflect changes in Acumatica
              this.Caches<SOFreightTracking>().Update(CurrentSOFreightTracking);
 
              // Persist if not in a transactional save
              this.Caches<SOFreightTracking>().Persist(PXDBOperation.Update);
          }
      }
 
  //add a new record - SO
  public  void addRecordSO(SOOrder order, SOOrderExt orderExt) {
 
          // Initialize a new instance for a new record
          CurrentSOFreightTracking = new SOFreightTracking();
 
          CurrentSOFreightTracking.ProjectID = order.ProjectID;
          CurrentSOFreightTracking.SOOrderNbr = order.OrderNbr;
          CurrentSOFreightTracking.Ponbr = order.CustomerOrderNbr;
          CurrentSOFreightTracking.SOFreightEst = orderExt.UsrCuryFreightEst;
          CurrentSOFreightTracking.SOFreightBlc = orderExt.UsrCuryFreightBlc;
 
          //insert new rec into the cache
          this.Caches<SOFreightTracking>().Insert(CurrentSOFreightTracking);
 
          //presists changes to save to the db
          this.Caches<SOFreightTracking>().Persist(PXDBOperation.Insert);
 
          PXTrace.WriteInformation("Adding SOFreightTracking record for OrderNbr: " + order.OrderNbr);
      }
 
  //Delete record - SO
  public  void deleteRecordSO(SOOrder order) {
 
      using (PXTransactionScope ts = new PXTransactionScope())
      {
          PXTrace.WriteInformation("SO Order Delete - Removing SOFreightTracking record for OrderNbr: " + order.OrderNbr);
 
          PXDatabase.Delete<SOFreightTracking>(new PXDataFieldRestrict<SOFreightTracking.sOOrderNbr>(order.OrderNbr));
 
          ts.Complete();
      }
 
  }
}

// Get list of records 
// Demonstrates querying multiple records and converting to list.
/*
var record = PXSelect<SOFreightTracking,
         Where<SOFreightTracking.sOOrderNbr,
         Equal<Required<SOFreightTracking.sOOrderNbr>>>>
         .Select(this, order.OrderNbr)
         .RowCast<SOFreightTracking>().ToList();
*/
