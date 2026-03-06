// Pop up screen and buttons 
// Demonstrates how to create a popup view in SOOrderEntry asking the user for input and a nested DAC for that popup.
public class SOOrderEntry_Extension : PXGraphExtension<PX.Objects.SO.SOOrderEntry>
{
    public PXFilter<addTagsPopUpViewDAC> addTagsPopUpView;
 
    public PXAction<SOOrder> TagAdd;
 
    [PXButton]
    [PXUIField(DisplayName = "Add Tags", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
    protected virtual IEnumerable tagAdd(PXAdapter adapter)
    {
        PXTrace.WriteInformation("Add Tags Clicked!");
 
        if(addTagsPopUpView.AskExt() == WebDialogResult.OK)
        {
            PXTrace.WriteInformation("Ok clikced !");
        }
            
        return adapter.Get();
    }
 
    public class addTagsPopUpViewDAC : PXBqlTable, IBqlTable
    {
        #region DTagName
        [PXString]
        [PXUIField(DisplayName = "Default Tag")]
        [DFCustomDropdownSelector]
        public virtual string DTagName { get; set; }
        public abstract class dtagName : PX.Data.BQL.BqlString.Field<dtagName> { }
        #endregion
 
        #region CTagName
        [PXString]
        [PXUIField(DisplayName = "Custom Tag")]
        [CSCustomDropdownSelector]
        public virtual string CTagName { get; set; }
        public abstract class ctagName : PX.Data.BQL.BqlString.Field<ctagName> { }
        #endregion
    }
}

// One of the selectors loaded for the values in pop up
// Provides drop-down lookup records of type 'CS' matching from CSSmartTags graph.
public class CSCustomDropdownSelectorAttribute : PXCustomSelectorAttribute
{
    public CSCustomDropdownSelectorAttribute()
    : base(typeof(Search<CSSmartTags.tagcd,
        Where<CSSmartTags.type, Equal<CS>>>))
    {
    }
 
    protected virtual IEnumerable GetRecords()
    {
        foreach (CSSmartTags record in PXSelect<CSSmartTags,
        Where<CSSmartTags.type, Equal<CS>>>.Select(new PXGraph()))
        {
            yield return record;
        }
    }
}
 
public class CS : PX.Data.BQL.BqlString.Constant<CS>
{
    public CS() : base("CS") { }
}
