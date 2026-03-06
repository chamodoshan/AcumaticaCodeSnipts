// UI example with all kind of UI elements 
// Complete DAC mapped from UI showing varied data fields: int, string, bool, string lists, timestamps and audit fields.
[PXCacheName("CSSmartTags")]
public class CSSmartTags : PXBqlTable, IBqlTable
{
  #region Tagid
  [PXDBIdentity(IsKey = true)]
  public virtual int? Tagid { get; set; }
  public abstract class tagid : PX.Data.BQL.BqlInt.Field<tagid> { }
  #endregion
 
  #region Tagcd
  [PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
  [PXUIField(DisplayName = "Tag Name")]
  public virtual string Tagcd { get; set; }
  public abstract class tagcd : PX.Data.BQL.BqlString.Field<tagcd> { }
  #endregion
 
  #region ModuleCD
  [PXDBString(15, IsUnicode = true, InputMask = "")]
  [PXUIField(DisplayName = "Module")]
  public virtual string ModuleCD { get; set; }
  public abstract class moduleCD : PX.Data.BQL.BqlString.Field<moduleCD> { }
  #endregion
 
  #region Active
  [PXDBBool()]
  [PXDefault(true)]
  [PXUIField(DisplayName = "Active")]
  public virtual bool? Active { get; set; }
  public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
  #endregion
 
  #region Color
  [PXDBString(10, IsUnicode = true, InputMask = "")]
  [PXUIField(DisplayName = "Color")]
  public virtual string Color { get; set; }
  public abstract class color : PX.Data.BQL.BqlString.Field<color> { }
  #endregion
 
  #region Type
  [PXDBString(2, IsFixed = true, InputMask = "")]
  [PXStringList(
          new string[]
          {
              SmartTags.TagType.Default,
              SmartTags.TagType.Custom
          },
          new string[]
          {
              SmartTags.Messages.Default,
              SmartTags.Messages.Custom
          })]
  [PXUIField(DisplayName = "Type")]
  public virtual string Type { get; set; }
  public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
  #endregion
 
  #region CreatedDateTime
  [PXDBCreatedDateTime()]
  public virtual DateTime? CreatedDateTime { get; set; }
  public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
  #endregion
 
  #region CreatedByID
  [PXDBCreatedByID()]
  public virtual Guid? CreatedByID { get; set; }
  public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
  #endregion
 
  #region CreatedByScreenID
  [PXDBCreatedByScreenID()]
  public virtual string CreatedByScreenID { get; set; }
  public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
  #endregion
 
  #region LastModifiedDateTime
  [PXDBLastModifiedDateTime()]
  public virtual DateTime? LastModifiedDateTime { get; set; }
  public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
  #endregion
 
  #region LastModifiedByID
  [PXDBLastModifiedByID()]
  public virtual Guid? LastModifiedByID { get; set; }
  public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
  #endregion
 
  #region LastModifiedByScreenID
  [PXDBLastModifiedByScreenID()]
  public virtual string LastModifiedByScreenID { get; set; }
  public abstract class lastModifiedByScreenID :          PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
  #endregion
 
  #region Tstamp
  [PXDBTimestamp()]
  [PXUIField(DisplayName = "Tstamp")]
  public virtual byte[] Tstamp { get; set; }
  public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
  #endregion
 
  #region Noteid
  [PXNote()]
  public virtual Guid? Noteid { get; set; }
  public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
  #endregion
}
