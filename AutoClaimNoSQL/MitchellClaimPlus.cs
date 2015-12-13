// Updated from xsd generated file MitchellCliams.cs to better fit with VelocityDB use
// Code mostly the same. Made classes based on OptimizedPersistable and added call to Update() in each property setter.
using System.Xml.Serialization;
using VelocityDb;
using VelocityDb.Session;

namespace AutoClaimUsingNoSQL
{
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.mitchell.com/examples/claim")]
  [System.Xml.Serialization.XmlRootAttribute("MitchellClaim", Namespace = "http://www.mitchell.com/examples/claim", IsNullable = false)]
  public class MitchellClaimType : OptimizedPersistable
  {
    private string m_claimNumber;

    private string m_claimantFirstNameField;

    private string m_claimantLastNameField;

    private StatusCode? m_statusField;

    private bool m_statusFieldSpecified;

    private System.DateTime? m_lossDateField;

    private bool m_lossDateFieldSpecified;

    private LossInfoType m_lossInfo;

    private long? m_assignedAdjusterIDField;

    private bool m_assignedAdjusterIDFieldSpecified;

    private VehicleInfoType[] m_vehicles;

    /// <remarks/>
    public string ClaimNumber
    {
      get
      {
        return this.m_claimNumber;
      }
      set
      {
        this.m_claimNumber = value;
      }
    }

    /// <remarks/>
    public string ClaimantFirstName
    {
      get
      {
        return this.m_claimantFirstNameField;
      }
      set
      {
        this.m_claimantFirstNameField = value;
      }
    }

    /// <remarks/>
    public string ClaimantLastName
    {
      get
      {
        return this.m_claimantLastNameField;
      }
      set
      {
        this.m_claimantLastNameField = value;
      }
    }

    /// <remarks/>
    public StatusCode? Status
    {
      get
      {
        return this.m_statusField;
      }
      set
      {
        this.m_statusField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool StatusSpecified
    {
      get
      {
        return this.m_statusFieldSpecified;
      }
      set
      {
        this.m_statusFieldSpecified = value;
      }
    }

    /// <remarks/>
    public System.DateTime? LossDate
    {
      get
      {
        return this.m_lossDateField;
      }
      set
      {
        this.m_lossDateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool LossDateSpecified
    {
      get
      {
        return this.m_lossDateFieldSpecified;
      }
      set
      {
        this.m_lossDateFieldSpecified = value;
      }
    }

    /// <remarks/>
    public LossInfoType LossInfo
    {
      get
      {
        return this.m_lossInfo;
      }
      set
      {
        this.m_lossInfo = value;
      }
    }

    /// <remarks/>
    public long? AssignedAdjusterID
    {
      get
      {
        return this.m_assignedAdjusterIDField;
      }
      set
      {
        this.m_assignedAdjusterIDField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool AssignedAdjusterIDSpecified
    {
      get
      {
        return this.m_assignedAdjusterIDFieldSpecified;
      }
      set
      {
        this.m_assignedAdjusterIDFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("VehicleDetails", IsNullable = false)]
    public VehicleInfoType[] Vehicles
    {
      get
      {
        return this.m_vehicles;
      }
      set
      {
        this.m_vehicles = value;
      }
    }

    public override void Unpersist(SessionBase session, bool disableFlush = true)
    {
      if (IsPersistent == false)
        return;
      foreach (var v in m_vehicles)
        v.Unpersist(session, disableFlush);
      if (m_lossInfo != null)
        m_lossInfo.Unpersist(session, disableFlush);
      base.Unpersist(session, disableFlush);
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
  [System.SerializableAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.mitchell.com/examples/claim")]
  public enum StatusCode
  {

    /// <remarks/>
    OPEN,

    /// <remarks/>
    CLOSED,
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.mitchell.com/examples/claim")]
  public partial class LossInfoType : OptimizedPersistable
  {
    private CauseOfLossCode causeOfLossField;

    private bool causeOfLossFieldSpecified;

    private System.DateTime? reportedDateField;

    private bool reportedDateFieldSpecified;

    private string lossDescriptionField;

    /// <remarks/>
    public CauseOfLossCode CauseOfLoss
    {
      get
      {
        return this.causeOfLossField;
      }
      set
      {
        Update();
        this.causeOfLossField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool CauseOfLossSpecified
    {
      get
      {
        return this.causeOfLossFieldSpecified;
      }
      set
      {
        Update();
        this.causeOfLossFieldSpecified = value;
      }
    }

    /// <remarks/>
    public System.DateTime? ReportedDate
    {
      get
      {
        return this.reportedDateField;
      }
      set
      {
        Update();
        this.reportedDateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool ReportedDateSpecified
    {
      get
      {
        return this.reportedDateFieldSpecified;
      }
      set
      {
        Update();
        this.reportedDateFieldSpecified = value;
      }
    }

    /// <remarks/>
    public string LossDescription
    {
      get
      {
        return this.lossDescriptionField;
      }
      set
      {
        Update();
        this.lossDescriptionField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
  [System.SerializableAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.mitchell.com/examples/claim")]
  public enum CauseOfLossCode
  {

    /// <remarks/>
    Collision,

    /// <remarks/>
    Explosion,

    /// <remarks/>
    Fire,

    /// <remarks/>
    Hail,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("Mechanical Breakdown")]
    MechanicalBreakdown,

    /// <remarks/>
    Other,
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.mitchell.com/examples/claim")]
  public partial class VehicleInfoType : OptimizedPersistable
  {

    private int? modelYearField;

    private string makeDescriptionField;

    private string modelDescriptionField;

    private string engineDescriptionField;

    private string exteriorColorField;

    private string vinField;

    private string licPlateField;

    private string licPlateStateField;

    private System.DateTime? licPlateExpDateField;

    private bool licPlateExpDateFieldSpecified;

    private string damageDescriptionField;

    private int? mileageField;

    private bool mileageFieldSpecified;

    /// <remarks/>
    public int? ModelYear
    {
      get
      {
        return this.modelYearField;
      }
      set
      {
        Update();
        this.modelYearField = value;
      }
    }

    /// <remarks/>
    public string MakeDescription
    {
      get
      {
        return this.makeDescriptionField;
      }
      set
      {
        Update();
        this.makeDescriptionField = value;
      }
    }

    /// <remarks/>
    public string ModelDescription
    {
      get
      {
        return this.modelDescriptionField;
      }
      set
      {
        Update();
        this.modelDescriptionField = value;
      }
    }

    /// <remarks/>
    public string EngineDescription
    {
      get
      {
        return this.engineDescriptionField;
      }
      set
      {
        Update();
        this.engineDescriptionField = value;
      }
    }

    /// <remarks/>
    public string ExteriorColor
    {
      get
      {
        return this.exteriorColorField;
      }
      set
      {
        Update();
        this.exteriorColorField = value;
      }
    }

    /// <remarks/>
    public string Vin
    {
      get
      {
        return this.vinField;
      }
      set
      {
        Update();
        this.vinField = value;
      }
    }

    /// <remarks/>
    public string LicPlate
    {
      get
      {
        return this.licPlateField;
      }
      set
      {
        Update();
        this.licPlateField = value;
      }
    }

    /// <remarks/>
    public string LicPlateState
    {
      get
      {
        return this.licPlateStateField;
      }
      set
      {
        Update();
        this.licPlateStateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
    public System.DateTime? LicPlateExpDate
    {
      get
      {
        return this.licPlateExpDateField;
      }
      set
      {
        Update();
        this.licPlateExpDateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool LicPlateExpDateSpecified
    {
      get
      {
        return this.licPlateExpDateFieldSpecified;
      }
      set
      {
        Update();
        this.licPlateExpDateFieldSpecified = value;
      }
    }

    /// <remarks/>
    public string DamageDescription
    {
      get
      {
        return this.damageDescriptionField;
      }
      set
      {
        Update();
        this.damageDescriptionField = value;
      }
    }

    /// <remarks/>
    public int? Mileage
    {
      get
      {
        return this.mileageField;
      }
      set
      {
        Update();
        this.mileageField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool MileageSpecified
    {
      get
      {
        return this.mileageFieldSpecified;
      }
      set
      {
        Update();
        this.mileageFieldSpecified = value;
      }
    }
  }
}
