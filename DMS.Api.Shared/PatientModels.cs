namespace DMS.Api.Shared
{
    /// <summary>
    /// Patient registration request model
    /// </summary>
    public class PatientRequest
    {
        public string PatientName { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public int CenterID { get; set; }
        public string MobileNo { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int? SchemeType { get; set; }
        public int CreatedBy { get; set; }
    }

    /// <summary>
    /// Patient update request model
    /// </summary>
    public class PatientUpdateRequest
    {
        public int PatientID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public int CenterID { get; set; }
        public string MobileNo { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int? SchemeType { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
    }

    /// <summary>
    /// Patient response model
    /// </summary>
    public class PatientResponse
    {
        public int PatientID { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public int CenterID { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public int? SchemeType { get; set; }
        public string SchemeTypeName { get; set; } = string.Empty;
        public int DialysisCycles { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Patient search/filter request
    /// </summary>
    public class PatientSearchRequest
    {
        public string? SearchTerm { get; set; }
        public int? CompanyID { get; set; }
        public int? CenterID { get; set; }
        public int? SchemeType { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
