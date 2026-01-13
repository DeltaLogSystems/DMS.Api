namespace DMS.Api.Shared
{
    #region Scheme Type Models

    /// <summary>
    /// Scheme type request model
    /// </summary>
    public class SchemeTypeRequest
    {
        public string SchemeTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Scheme type response model
    /// </summary>
    public class SchemeTypeResponse
    {
        public int SchemeTypeID { get; set; }
        public string SchemeTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    #endregion

    #region Role Models

    /// <summary>
    /// Role response model (for dropdown)
    /// </summary>
    public class RoleResponse
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    #endregion

    #region Company Models

    /// <summary>
    /// Company response model (for dropdown)
    /// </summary>
    public class CompanyResponse
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    #endregion

    #region Center Models

    /// <summary>
    /// Center response model (for dropdown)
    /// </summary>
    public class CenterResponse
    {
        public int CenterID { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string CenterAddress { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Company and Center combined response model
    /// </summary>
    public class CompanyCenterResponse
    {
        public CompanyInfo Company { get; set; } = new CompanyInfo();
        public CenterInfo Center { get; set; } = new CenterInfo();
    }

    /// <summary>
    /// Detailed company information
    /// </summary>
    public class CompanyInfo
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyLogo { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Detailed center information
    /// </summary>
    public class CenterInfo
    {
        public int CenterID { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string CenterAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    #endregion
}
