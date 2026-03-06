// Process screen update selected records Graph Extension 
// Acumatica Graph Extension for APRetainageRelease. Includes multiple actions for selecting documents and overriding the main list.
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using PX.Objects.GL.Attributes;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AP.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects;
using PX.Objects.AP;
using PX.Common;
using static PX.Data.Reports.PXSettingProvider.ReportSettings;
using static PX.Data.BQL.BqlPlaceholder;
using static PX.Objects.TX.CSTaxCalcType;

namespace PX.Objects.AP
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class APRetainageRelease_Extension : PXGraphExtension<PX.Objects.AP.APRetainageRelease>
    {
        
        public List<APInvoiceExt> docList = new List<APInvoiceExt> ();
        public List<APInvoiceExt> selectedDocList = new List<APInvoiceExt> ();
        public String invNbr = null;
        protected APRetainageFilterExt commonFilterExt { get; set; }
        protected APInvoice orgInvoice { get; set; }

        #region Event Handlers
        protected void APRetainageFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                APRetainageFilter filter = e.Row as APRetainageFilter;
                if (filter == null) return;
                var filterExt = filter.GetExtension<APRetainageFilterExt>();
                commonFilterExt = filterExt;

                if (filterExt != null)
                {
                    invNbr = filterExt.UsrInvoiceNbr;
                    PXTrace.WriteInformation($"Update Invoice Nbr: {filterExt.UsrInvoiceNbr}");
                }
                ts.Complete();
            }
        }

        public delegate IEnumerable DocumentListDelegate();

        [PXOverride]
        public IEnumerable documentList(DocumentListDelegate baseMethod)
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                foreach (APInvoiceExt doc in baseMethod())
                {
                    docList.Add(doc);
                    yield return doc;
                }
                ts.Complete();
            }
        }

        public PXAction<APRetainageFilter> UpdateBtn;

        [PXButton]
        [PXUIField(DisplayName = "Update", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable updateBtn(PXAdapter adapter)
        {

            if(invNbr != null)
            {
                if (docList.Count > 0)
                {
                    foreach (APInvoiceExt doc in docList)
                    {
                        if (doc.Selected == true)
                        {
                            selectedDocList.Add(doc);
                            using (PXTransactionScope ts = new PXTransactionScope())
                            {
                                doc.DocDesc = invNbr;
                                Base.Caches<APInvoiceExt>().Update(doc);
                                Base.Caches<APInvoiceExt>().Persist(PXDBOperation.Update);
                                ts.Complete();
                            }

                            updateAPInvoice(doc, invNbr);
                            commonFilterExt.UsrInvoiceNbr = null;
                        }
                    }

                    if(selectedDocList.Count == 0)
                    {
                        var msg = "No Selected Records to Update!";
                        PXTrace.WriteInformation(msg);
                        throw new PXRowPersistingException(typeof(APRetainageFilterExt.usrInvoiceNbr).Name, commonFilterExt.UsrInvoiceNbr, msg);
                    }
                }
                else
                {
                    var msg = "No Records to Update!";
                    PXTrace.WriteInformation(msg);
                    throw new PXRowPersistingException(typeof(APRetainageFilterExt.usrInvoiceNbr).Name, commonFilterExt.UsrInvoiceNbr, msg);
                }
            }
            else
            {
                var msg = "Invoice Number is Empty!";
                PXTrace.WriteInformation(msg);
                throw new PXRowPersistingException(typeof(APRetainageFilterExt.usrInvoiceNbr).Name, commonFilterExt.UsrInvoiceNbr, msg);
            }
            PXTrace.WriteInformation("Update Clicked!");
            return adapter.Get();
        }

        public PXAction<APRetainageFilter> UpdateBtnAll;

        [PXButton]
        [PXUIField(DisplayName = "Update All", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable updateBtnAll(PXAdapter adapter)
        {

            if (invNbr != null)
            {
                if (docList.Count > 0)
                {
                    foreach (APInvoiceExt doc in docList)
                    {
                        using (PXTransactionScope ts = new PXTransactionScope())
                        {
                            doc.DocDesc = invNbr;
                            Base.Caches<APInvoiceExt>().Update(doc);
                            Base.Caches<APInvoiceExt>().Persist(PXDBOperation.Update);
                            ts.Complete();
                        }

                        updateAPInvoice(doc, invNbr);
                        commonFilterExt.UsrInvoiceNbr = null;
                    }
                }
                else
                {
                    var msg = "No Records to Update!";
                    PXTrace.WriteInformation(msg);
                    throw new PXRowPersistingException(typeof(APRetainageFilterExt.usrInvoiceNbr).Name, commonFilterExt.UsrInvoiceNbr, msg);
                }
            }
            else
            {
                var msg = "Invoice Number is Empty!";
                PXTrace.WriteInformation(msg);
                throw new PXRowPersistingException(typeof(APRetainageFilterExt.usrInvoiceNbr).Name, commonFilterExt.UsrInvoiceNbr, msg);
            }
            PXTrace.WriteInformation("Update Clicked!");
            return adapter.Get();
        }
        #endregion

        private void updateAPInvoice(APInvoiceExt invoiceExt, String desc)
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                APInvoice apInvoice = PXSelect<APInvoice,
                    Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
                    And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>
                    .Select(Base, invoiceExt.DocType, invoiceExt.RefNbr);

                if (apInvoice != null)
                {
                    apInvoice.DocDesc = desc;
                    Base.Caches<APInvoice>().Update(apInvoice);
                    Base.Caches<APInvoice>().Persist(PXDBOperation.Update);
                }
                ts.Complete();
            }
        }
    }
}
