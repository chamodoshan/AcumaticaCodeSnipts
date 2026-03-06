using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.PO;
using PX.Objects.SO;

public class MiscSnippets
{
// Copy Orde from  QT to SO 
// Extends the standard CopyOrder action to clear specific fields when creating a new copied order.

public delegate IEnumerable CopyOrderDelegate(PXAdapter adapter);
 
[PXOverride]
public virtual IEnumerable CopyOrder(PXAdapter adapter, CopyOrderDelegate baseMethod)
{
    SOOrder sourceOrder = Base.Document.Current;
    if (sourceOrder == null) return baseMethod(adapter);
 
    // Run the default Copy Order logic
    IEnumerable result = baseMethod(adapter);
 
    // Retrieve the newly copied order
    SOOrder copiedOrder = Base.Document.Current;
    if (copiedOrder != null)
    {
        // Clear a specific field (Example: Customer PO Number)
        copiedOrder.CustomerOrderNbr = null;
 
        // Clear a custom field (Example: UsrCustomField from SOOrderExt)
        copiedOrder.GetExtension<SOOrderExt>().UsrFreightConfirmed = false;
 
        // Ensure the changes are saved
        Base.Document.Update(copiedOrder);
    }
 
    return result;
}


// UI visibility update
// Demonstrates how to programmatically set the visibility of a strongly-typed databound field using PXUIFieldAttribute.

PXUIFieldAttribute.SetVisible<PX.Objects.CR.BAccountExt.usrCreditLimit>(cache, customer, true);


// DB operation check
// Example of checking during persisting events whether the current operation is Insert or Update to execute conditional logic.

if (e.Operation == PXDBOperation.Update || e.Operation == PXDBOperation.Insert) //&& e.TranStatus == PXTranStatus.Completed


// POP up
// Example of prompting user confirmation on string parameters before committing changes in a graph event.

if (Base.Document.Ask("Reminder",
    "Freight amount is 0. Do you want to proceed without adding freight?",
    MessageButtons.YesNo) == WebDialogResult.No)
{
    orderExt.UsrFreightConfirmed = true;
    e.Cache.Update(order);
    return; // Stop saving if the user selects "No"
}


// Set UI warnings 
// Shows setting a UI Warning icon with a description on a specific datafield.

protected void _(Events.RowSelected<ARInvoice> e)
{
 
    ARInvoice invoice = (ARInvoice)e.Row;
 
    if (invoice == null) return;
 
    Customer customer = PXSelect<Customer,
    Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
    .Select(Base, invoice.CustomerID)
    .FirstOrDefault();
 
    if (customer == null) return;
 
    var customerExt = customer.GetExtension<PX.Objects.CR.BAccountExt>();
 
    if (customerExt == null) return;
 
    if (customer.CreditRule == CreditRuleTypes.CS_NO_CHECKING)
    {
        if (invoice.CuryDocBal > 0)
        {
            if (remainingCreditLimit >= 0m && remainingCreditLimit >= invoice.CuryDocBal)
            {
                       
                PXUIFieldAttribute.SetWarning<ARInvoice.customerID>(e.Cache, invoice, null);
                        
            }
            else
            {
                PXUIFieldAttribute.SetWarning<ARInvoice.customerID>(e.Cache, invoice, "The customer's credit limit has been exceeded.");
                       
            }
        }
    }
}


// Save method 
// Allows interception of universal 'Save' click using Persist override mapped over the base persist.

//Override the method that handles the removal of the hold status
public delegate void PersistDelegate();
[PXOverride]
public void Persist(PersistDelegate baseMethod)
{
    // Get the current Purchase Order record
    POOrder currentOrder = Base.Document.Current;
    if (currentOrder != null)
    {
        // Check if the budget is exceeded
        bool isBudgetExceeded = CheckProjectBudgetExceeded(currentOrder);
 
        // If the budget is exceeded, display an error and stop the process
        if (isBudgetExceeded)
        {
            String msg = "The project budget has been exceeded. You cannot proceed.";
            throw new PXException(msg);
        }
    }
 
    // Call the base method to proceed with saving the order
    baseMethod();
}


// Release and Hold method 
// How to inject validations strictly during action override, like when someone triggers ReleaseFromHold.

public delegate IEnumerable ReleaseFromHoldDelegate(PXAdapter adapter);
 
[PXOverride]
public virtual IEnumerable ReleaseFromHold(PXAdapter adapter, ReleaseFromHoldDelegate baseMethod)
{
    if (Base.Document.Current != null)
    {
        // Check If there is any Allocation Rule in progress
        //bool isAllocationTaskPrgress = IsAllocationTaskInProgress();
        //PXTrace.WriteInformation("isAllocationTaskPrgress:" + isAllocationTaskPrgress);
        //if (isAllocationTaskPrgress)
        //{
        // Check If Budget is exceeded
        bool isBudgetExceeded = CheckBudgetisExceeded();
        PXTrace.WriteInformation("isBudgetExceeded:" + isBudgetExceeded);
        if (isBudgetExceeded)
        {
            Base.Document.Cache.RaiseExceptionHandling<POOrder.curyOrderTotal>(Base.Document.Current, Base.Document.Current.CuryOrderTotal,
                                  new PXSetPropertyException(PX.Objects.PM.Messages.BudgetControlDocumentWarning, PXErrorLevel.Warning));
            throw new PXException(PX.Objects.PM.Messages.BudgetControlDocumentWarning);
        }
        // }
    }
    return adapter.Get<POOrder>();
}


// Error msg at the top PO and bill screens 
// Retrieving an already-set UI field warning. Very useful to capture global document state from fields without repeating logic.

string warning = PXUIFieldAttribute.GetWarning<POOrder.orderQty>(sender, e.Row);
 
if (!string.IsNullOrEmpty(warning)
    && warning.Contains(PX.Objects.PM.Messages.BudgetControlDocumentWarning))
{
    IsBudgetExceeded = true;
}
else
{
    IsBudgetExceeded = false;
}


// Allocation check
// Selects using `Where` across PMTasks in graph to ascertain whether `ApplicableAllocationRules` intersect the list.

private bool IsAllocationTaskInProgress()
{
    bool isAllocationTaskPrgress;
    var projectId = Base.Document.Current.ProjectID;
    PXTrace.WriteInformation("projectId:" + projectId);
    PXSelectBase<PMTask> selectTasks = new PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.allocationID, IsNotNull>>>(Base);
 
    List<PMTask> tasks = new List<PMTask>();
    foreach (PMTask pmTask in selectTasks.Select(projectId))
    {
        tasks.Add(pmTask);
    }
    isAllocationTaskPrgress = tasks.Where(s => ApplicableAllocationRules.Contains(s.AllocationID.ToUpper())).Any();
    return isAllocationTaskPrgress;
}

}
