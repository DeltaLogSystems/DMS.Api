using System.Data;

namespace DMS.Api.DL
{
    public static class PatientsDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region Patient Code Generation

        /// <summary>
        /// Generate next patient code for company
        /// </summary>
        public static async Task<string> GeneratePatientCodeAsync(int companyId)
        {
            // Get company code
            var companyCode = await _sqlHelper.ExecScalarAsync(
                "SELECT CompanyCode FROM M_Companies WHERE CompanyID = @companyId",
                "@companyId", companyId
            );

            if (companyCode == null)
            {
                throw new Exception("Company not found");
            }

            // Get last patient number for this company
            var lastPatientCode = await _sqlHelper.ExecScalarAsync(
                @"SELECT PatientCode FROM M_Patients 
                  WHERE CompanyID = @companyId 
                  ORDER BY PatientID DESC 
                  LIMIT 1",
                "@companyId", companyId
            );

            int nextNumber = 1;

            if (lastPatientCode != null)
            {
                string lastCode = lastPatientCode.ToString()!;
                string prefix = companyCode.ToString()!;

                // Extract number from last code (e.g., MHS0001 -> 0001)
                if (lastCode.StartsWith(prefix))
                {
                    string numberPart = lastCode.Substring(prefix.Length);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            // Format: CompanyCode + 4-digit number (e.g., MHS0001)
            string newPatientCode = $"{companyCode}{nextNumber:D4}";
            return newPatientCode;
        }

        /// <summary>
        /// Calculate age from date of birth
        /// </summary>
        public static int CalculateAge(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;

            // Adjust if birthday hasn't occurred this year
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Get all patients with related information
        /// </summary>
        public static async Task<DataTable> GetAllPatientsAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  ORDER BY p.PatientID DESC"
            );
            return dt;
        }

        /// <summary>
        /// Get active patients only
        /// </summary>
        public static async Task<DataTable> GetActivePatientsAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  WHERE p.IsActive = 1
                  ORDER BY p.PatientID DESC"
            );
            return dt;
        }

        /// <summary>
        /// Get patient by ID
        /// </summary>
        public static async Task<DataTable> GetPatientByIdAsync(int patientId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  WHERE p.PatientID = @patientId",
                "@patientId", patientId
            );
            return dt;
        }

        /// <summary>
        /// Get patient by patient code
        /// </summary>
        public static async Task<DataTable> GetPatientByCodeAsync(string patientCode)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  WHERE p.PatientCode = @patientCode",
                "@patientCode", patientCode
            );
            return dt;
        }

        /// <summary>
        /// Get patient by mobile number
        /// </summary>
        public static async Task<DataTable> GetPatientByMobileAsync(string mobileNo)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  WHERE p.MobileNo = @mobileNo",
                "@mobileNo", mobileNo
            );
            return dt;
        }

        /// <summary>
        /// Get patients by company ID
        /// </summary>
        public static async Task<DataTable> GetPatientsByCompanyIdAsync(int companyId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  WHERE p.CompanyID = @companyId
                  ORDER BY p.PatientID DESC",
                "@companyId", companyId
            );
            return dt;
        }

        /// <summary>
        /// Get patients by center ID
        /// </summary>
        public static async Task<DataTable> GetPatientsByCenterIdAsync(int centerId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  WHERE p.CenterID = @centerId
                  ORDER BY p.PatientID DESC",
                "@centerId", centerId
            );
            return dt;
        }

        /// <summary>
        /// Get patients by scheme type
        /// </summary>
        public static async Task<DataTable> GetPatientsBySchemeTypeAsync(int schemeType)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  WHERE p.SchemeType = @schemeType
                  ORDER BY p.PatientID DESC",
                "@schemeType", schemeType
            );
            return dt;
        }

        /// <summary>
        /// Search patients by name (partial match)
        /// </summary>
        public static async Task<DataTable> SearchPatientsByNameAsync(string searchTerm)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT p.*, 
                         comp.CompanyName, comp.CompanyCode,
                         c.CenterName,
                         st.SchemeTypeName
                  FROM M_Patients p
                  INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                  LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                  WHERE p.PatientName LIKE @searchTerm 
                     OR p.PatientCode LIKE @searchTerm
                     OR p.MobileNo LIKE @searchTerm
                  ORDER BY p.PatientID DESC",
                "@searchTerm", $"%{searchTerm}%"
            );
            return dt;
        }

        /// <summary>
        /// Advanced search with multiple filters
        /// </summary>
        public static async Task<DataTable> SearchPatientsAsync(
            string? searchTerm = null,
            int? companyId = null,
            int? centerId = null,
            int? schemeType = null,
            bool? isActive = null)
        {
            var query = @"SELECT p.*, 
                                 comp.CompanyName, comp.CompanyCode,
                                 c.CenterName,
                                 st.SchemeTypeName
                          FROM M_Patients p
                          INNER JOIN M_Companies comp ON p.CompanyID = comp.CompanyID
                          INNER JOIN M_Centers c ON p.CenterID = c.CenterID
                          LEFT JOIN M_SchemeTypes st ON p.SchemeType = st.SchemeTypeID
                          WHERE 1=1";

            var parameters = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query += " AND (p.PatientName LIKE @searchTerm OR p.PatientCode LIKE @searchTerm OR p.MobileNo LIKE @searchTerm)";
                parameters.Add("@searchTerm");
                parameters.Add($"%{searchTerm}%");
            }

            if (companyId.HasValue)
            {
                query += " AND p.CompanyID = @companyId";
                parameters.Add("@companyId");
                parameters.Add(companyId.Value);
            }

            if (centerId.HasValue)
            {
                query += " AND p.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            if (schemeType.HasValue)
            {
                query += " AND p.SchemeType = @schemeType";
                parameters.Add("@schemeType");
                parameters.Add(schemeType.Value);
            }

            if (isActive.HasValue)
            {
                query += " AND p.IsActive = @isActive";
                parameters.Add("@isActive");
                parameters.Add(isActive.Value);
            }

            query += " ORDER BY p.PatientID DESC";

            var dt = await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
            return dt;
        }

        /// <summary>
        /// Check if mobile number exists
        /// </summary>
        public static async Task<bool> MobileNumberExistsAsync(string mobileNo, int? excludePatientId = null)
        {
            string query = excludePatientId.HasValue
                ? "SELECT COUNT(*) FROM M_Patients WHERE MobileNo = @mobileNo AND PatientID != @patientId"
                : "SELECT COUNT(*) FROM M_Patients WHERE MobileNo = @mobileNo";

            object?[] parameters = excludePatientId.HasValue
                ? new object[] { "@mobileNo", mobileNo, "@patientId", excludePatientId.Value }
                : new object[] { "@mobileNo", mobileNo };

            var result = await _sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Check if patient code exists
        /// </summary>
        public static async Task<bool> PatientCodeExistsAsync(string patientCode)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM M_Patients WHERE PatientCode = @patientCode",
                "@patientCode", patientCode
            );
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Get patient count by company
        /// </summary>
        public static async Task<int> GetPatientCountByCompanyAsync(int companyId, bool activeOnly = false)
        {
            string query = activeOnly
                ? "SELECT COUNT(*) FROM M_Patients WHERE CompanyID = @companyId AND IsActive = 1"
                : "SELECT COUNT(*) FROM M_Patients WHERE CompanyID = @companyId";

            var result = await _sqlHelper.ExecScalarAsync(query, "@companyId", companyId);
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Get patient count by center
        /// </summary>
        public static async Task<int> GetPatientCountByCenterAsync(int centerId, bool activeOnly = false)
        {
            string query = activeOnly
                ? "SELECT COUNT(*) FROM M_Patients WHERE CenterID = @centerId AND IsActive = 1"
                : "SELECT COUNT(*) FROM M_Patients WHERE CenterID = @centerId";

            var result = await _sqlHelper.ExecScalarAsync(query, "@centerId", centerId);
            return Convert.ToInt32(result);
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Insert new patient with auto-generated code
        /// </summary>
        public static async Task<int> InsertPatientAsync(
            string patientName,
            int companyId,
            int centerId,
            string mobileNo,
            DateTime dateOfBirth,
            int? schemeType,
            int createdBy)
        {
            // Generate patient code
            string patientCode = await GeneratePatientCodeAsync(companyId);

            // Calculate age
            int age = CalculateAge(dateOfBirth);

            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_Patients 
                  (PatientCode, PatientName, CompanyID, CenterID, MobileNo, 
                   DateOfBirth, Age, SchemeType, DialysisCycles, IsActive,
                   CreatedBy, CreatedDate)
                  VALUES 
                  (@patientCode, @patientName, @companyId, @centerId, @mobileNo,
                   @dateOfBirth, @age, @schemeType, 0, 1,
                   @createdBy, CURDATE());
                  SELECT LAST_INSERT_ID();",
                "@patientCode", patientCode,
                "@patientName", patientName,
                "@companyId", companyId,
                "@centerId", centerId,
                "@mobileNo", mobileNo,
                "@dateOfBirth", dateOfBirth,
                "@age", age,
                "@schemeType", schemeType.HasValue ? (object)schemeType.Value : DBNull.Value,
                "@createdBy", createdBy
            );
            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update patient details
        /// </summary>
        public static async Task<int> UpdatePatientAsync(
            int patientId,
            string patientName,
            int companyId,
            int centerId,
            string mobileNo,
            DateTime dateOfBirth,
            int? schemeType,
            bool isActive,
            int modifiedBy)
        {
            // Recalculate age
            int age = CalculateAge(dateOfBirth);

            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Patients 
                  SET PatientName = @patientName,
                      CompanyID = @companyId,
                      CenterID = @centerId,
                      MobileNo = @mobileNo,
                      DateOfBirth = @dateOfBirth,
                      Age = @age,
                      SchemeType = @schemeType,
                      IsActive = @isActive,
                      ModifiedBy = @modifiedBy,
                      ModifiedDate = CURDATE()
                  WHERE PatientID = @patientId",
                "@patientId", patientId,
                "@patientName", patientName,
                "@companyId", companyId,
                "@centerId", centerId,
                "@mobileNo", mobileNo,
                "@dateOfBirth", dateOfBirth,
                "@age", age,
                "@schemeType", schemeType.HasValue ? (object)schemeType.Value : DBNull.Value,
                "@isActive", isActive,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        /// <summary>
        /// Update dialysis cycles count
        /// </summary>
        public static async Task<int> UpdateDialysisCyclesAsync(int patientId, int cycles)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Patients 
                  SET DialysisCycles = @cycles,
                      ModifiedDate = CURDATE()
                  WHERE PatientID = @patientId",
                "@patientId", patientId,
                "@cycles", cycles
            );
            return result;
        }

        /// <summary>
        /// Increment dialysis cycles
        /// </summary>
        public static async Task<int> IncrementDialysisCyclesAsync(int patientId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Patients 
                  SET DialysisCycles = DialysisCycles + 1,
                      ModifiedDate = CURDATE()
                  WHERE PatientID = @patientId",
                "@patientId", patientId
            );
            return result;
        }

        /// <summary>
        /// Toggle patient active status
        /// </summary>
        public static async Task<int> TogglePatientStatusAsync(int patientId, int modifiedBy)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Patients 
                  SET IsActive = NOT IsActive,
                      ModifiedBy = @modifiedBy,
                      ModifiedDate = CURDATE()
                  WHERE PatientID = @patientId",
                "@patientId", patientId,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        /// <summary>
        /// Update all patient ages (can be run as scheduled job)
        /// </summary>
        public static async Task<int> UpdateAllPatientAgesAsync()
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Patients 
                  SET Age = TIMESTAMPDIFF(YEAR, DateOfBirth, CURDATE()) - 
                           (DATE_FORMAT(CURDATE(), '%m%d') < DATE_FORMAT(DateOfBirth, '%m%d')),
                      ModifiedDate = CURDATE()
                  WHERE IsActive = 1"
            );
            return result;
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Soft delete patient
        /// </summary>
        public static async Task<int> SoftDeletePatientAsync(int patientId, int modifiedBy)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Patients 
                  SET IsActive = 0,
                      ModifiedBy = @modifiedBy,
                      ModifiedDate = CURDATE()
                  WHERE PatientID = @patientId",
                "@patientId", patientId,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        /// <summary>
        /// Permanently delete patient
        /// </summary>
        public static async Task<int> DeletePatientAsync(int patientId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Patients WHERE PatientID = @patientId",
                "@patientId", patientId
            );
            return result;
        }

        #endregion
    }
}
