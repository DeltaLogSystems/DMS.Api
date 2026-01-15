using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all appointments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var dt = await AppointmentsDL.GetAllAppointmentsAsync();
                var appointments = await ConvertDataTableToAppointmentListAsync(dt);

                return Ok(ApiResponse<List<AppointmentResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {appointments.Count} appointment(s)",
                    appointments
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving appointments: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get appointment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            try
            {
                var dt = await AppointmentsDL.GetAppointmentByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<AppointmentResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Appointment not found"
                    ));
                }

                var appointment = await ConvertRowToAppointmentAsync(dt.Rows[0]);

                return Ok(ApiResponse<AppointmentResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Appointment retrieved successfully",
                    appointment
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving appointment: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get appointments by patient ID
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetAppointmentsByPatient(int patientId)
        {
            try
            {
                var dt = await AppointmentsDL.GetAppointmentsByPatientIdAsync(patientId);
                var appointments = await ConvertDataTableToAppointmentListAsync(dt);

                return Ok(ApiResponse<List<AppointmentResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {appointments.Count} appointment(s)",
                    appointments
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving appointments: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get appointments by center and date
        /// </summary>
        [HttpGet("center/{centerId}/date/{date}")]
        public async Task<IActionResult> GetAppointmentsByCenterAndDate(int centerId, DateTime date)
        {
            try
            {
                var dt = await AppointmentsDL.GetAppointmentsByCenterAndDateAsync(centerId, date);
                var appointments = await ConvertDataTableToAppointmentListAsync(dt);

                return Ok(ApiResponse<List<AppointmentResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {appointments.Count} appointment(s) for {date:yyyy-MM-dd}",
                    appointments
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving appointments: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get appointments by date range
        /// </summary>
        [HttpGet("center/{centerId}/range")]
        public async Task<IActionResult> GetAppointmentsByDateRange(
            int centerId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var dt = await AppointmentsDL.GetAppointmentsByDateRangeAsync(centerId, startDate, endDate);
                var appointments = await ConvertDataTableToAppointmentListAsync(dt);

                return Ok(ApiResponse<List<AppointmentResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {appointments.Count} appointment(s)",
                    appointments
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving appointments: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patient dialysis cycle count
        /// </summary>
        [HttpGet("patient/{patientId}/cycles")]
        public async Task<IActionResult> GetPatientDialysisCycles(int patientId)
        {
            try
            {
                int completedCount = await AppointmentsDL.GetAppointmentCountByPatientAsync(patientId, 4);
                int totalCount = await AppointmentsDL.GetAppointmentCountByPatientAsync(patientId);

                var cycleInfo = new
                {
                    patientId = patientId,
                    completedCycles = completedCount,
                    totalAppointments = totalCount,
                    remainingCycles = 18 - completedCount, // MJPJAY scheme: 18 sessions
                    daysCovered = completedCount > 0 ? (completedCount - 1) * 2 : 0, // Every 2 days
                    nextCycleDate = completedCount < 18 ? DateTime.Today.AddDays(2) : (DateTime?)null
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Patient dialysis cycle information retrieved",
                    cycleInfo
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving cycle information: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Create new appointment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentRequest request)
        {
            try
            {
                // Validate appointment date
                if (request.AppointmentDate.Date < DateTime.Today)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Cannot create appointment for past dates"
                    ));
                }

                // Validate slot times
                if (request.SlotStartTime >= request.SlotEndTime)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Slot end time must be after start time"
                    ));
                }

                // Check if patient already has appointment on this date
                bool hasAppointment = await AppointmentsDL.PatientHasAppointmentOnDateAsync(
                    request.PatientID,
                    request.AppointmentDate
                );

                if (hasAppointment)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Patient already has an appointment scheduled for this date. Only one dialysis session per day is allowed."
                    ));
                }

                // Check if slot is available
                bool isSlotAvailable = await AppointmentsDL.IsSlotAvailableAsync(
                    request.CenterID,
                    request.AppointmentDate,
                    request.SlotStartTime,
                    request.SlotEndTime
                );

                if (!isSlotAvailable)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Selected time slot is not available. Please choose a different time."
                    ));
                }

                // Create appointment
                int appointmentId = await AppointmentsDL.CreateAppointmentAsync(
                    request.PatientID,
                    request.CenterID,
                    request.CompanyID,
                    request.AppointmentDate,
                    request.SlotStartTime,
                    request.SlotEndTime,
                    request.CreatedBy
                );

                // Get created appointment details
                var dtAppointment = await AppointmentsDL.GetAppointmentByIdAsync(appointmentId);
                var appointment = await ConvertRowToAppointmentAsync(dtAppointment.Rows[0]);

                return Ok(ApiResponse<AppointmentResponse>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Appointment created successfully",
                    appointment
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating appointment: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Update appointment status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateAppointmentStatusRequest request)
        {
            try
            {
                // Check if appointment exists
                var dtExisting = await AppointmentsDL.GetAppointmentByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Appointment not found"
                    ));
                }

                // Update status
                int result = await AppointmentsDL.UpdateAppointmentStatusAsync(id, request.NewStatus, request.ModifiedBy);

                if (result > 0)
                {
                    // If status is completed (4), update patient dialysis cycles
                    if (request.NewStatus == 4)
                    {
                        var row = dtExisting.Rows[0];
                        int patientId = Convert.ToInt32(row["PatientID"]);
                        DateTime appointmentDate = Convert.ToDateTime(row["AppointmentDate"]);

                        // Update cycles with cycle management logic
                        await AppointmentsDL.UpdatePatientDialysisCyclesAsync(patientId, appointmentDate);

                        // Check if cycle completed 18 sessions
                        var dtCycleInfo = await PatientCyclesDL.GetPatientCurrentCycleAsync(patientId);
                        if (dtCycleInfo.Rows.Count > 0)
                        {
                            int sessionCount = Convert.ToInt32(dtCycleInfo.Rows[0]["CurrentCycleSessionCount"]);

                            if (sessionCount >= 18)
                            {
                                return Ok(ApiResponse.SuccessResponse(
                                    "Appointment completed successfully! Patient has completed all 18 sessions in current cycle."
                                ));
                            }
                        }
                    }

                    return Ok(ApiResponse.SuccessResponse(
                        "Appointment status updated successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update appointment status"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating appointment status: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patient cycle information with detailed tracking
        /// </summary>
        [HttpGet("patient/{patientId}/cycle-info")]
        public async Task<IActionResult> GetPatientCycleInfo(int patientId)
        {
            try
            {
                var dt = await PatientCyclesDL.GetPatientCurrentCycleAsync(patientId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<object>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Patient not found"
                    ));
                }

                var row = dt.Rows[0];

                var cycleInfo = new
                {
                    patientId = patientId,
                    patientCode = row["PatientCode"]?.ToString(),
                    patientName = row["PatientName"]?.ToString(),
                    currentCycle = new
                    {
                        cycleNumber = Convert.ToInt32(row["CurrentCycleNumber"]),
                        startDate = row["CurrentCycleStartDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["CurrentCycleStartDate"]).ToString("yyyy-MM-dd")
                            : null,
                        endDate = row["CurrentCycleEndDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["CurrentCycleEndDate"]).ToString("yyyy-MM-dd")
                            : null,
                        daysRemaining = row["DaysRemaining"] != DBNull.Value
                            ? Convert.ToInt32(row["DaysRemaining"])
                            : 0,
                        completedSessions = Convert.ToInt32(row["CurrentCycleSessionCount"]),
                        remainingSessions = row["SessionsRemaining"] != DBNull.Value
                            ? Convert.ToInt32(row["SessionsRemaining"])
                            : 18,
                        totalSessions = 18,
                        progressPercentage = (Convert.ToInt32(row["CurrentCycleSessionCount"]) * 100.0 / 18.0)
                    },
                    totalStats = new
                    {
                        totalCompletedCycles = Convert.ToInt32(row["TotalCompletedCycles"]),
                        totalDialysisSessions = Convert.ToInt32(row["DialysisCycles"])
                    },
                    mjpjayScheme = new
                    {
                        schemeName = "MJPJAY",
                        sessionsPerCycle = 18,
                        daysPerCycle = 42,
                        description = "18 dialysis sessions within 42 days"
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Patient cycle information retrieved successfully",
                    cycleInfo
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving cycle information: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patient cycle history
        /// </summary>
        [HttpGet("patient/{patientId}/cycle-history")]
        public async Task<IActionResult> GetPatientCycleHistory(int patientId)
        {
            try
            {
                var dt = await PatientCyclesDL.GetPatientCycleHistoryAsync(patientId);
                var history = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    history.Add(new
                    {
                        cycleNumber = Convert.ToInt32(row["CycleNumber"]),
                        startDate = Convert.ToDateTime(row["CycleStartDate"]).ToString("yyyy-MM-dd"),
                        endDate = Convert.ToDateTime(row["CycleEndDate"]).ToString("yyyy-MM-dd"),
                        plannedSessions = Convert.ToInt32(row["PlannedSessions"]),
                        completedSessions = Convert.ToInt32(row["CompletedSessions"]),
                        status = row["CycleStatus"]?.ToString(),
                        firstAppointment = row["FirstAppointmentDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["FirstAppointmentDate"]).ToString("yyyy-MM-dd")
                            : null,
                        lastAppointment = row["LastAppointmentDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["LastAppointmentDate"]).ToString("yyyy-MM-dd")
                            : null,
                        completedDate = row["CompletedDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["CompletedDate"]).ToString("yyyy-MM-dd HH:mm:ss")
                            : null
                    });
                }

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {history.Count} cycle(s) history",
                    history
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving cycle history: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Manually process expired cycles (admin function)
        /// </summary>
        [HttpPost("process-expired-cycles")]
        public async Task<IActionResult> ProcessExpiredCycles()
        {
            try
            {
                int processedCount = await PatientCyclesDL.ProcessExpiredCyclesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(
                    ResponseStatus.Success,
                    $"Processed {processedCount} expired cycle(s)",
                    new { processedCount = processedCount }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error processing expired cycles: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Reschedule appointment
        /// </summary>
        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> RescheduleAppointment(int id, [FromBody] RescheduleAppointmentRequest request)
        {
            try
            {
                // Check if appointment exists
                var dtExisting = await AppointmentsDL.GetAppointmentByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Appointment not found"
                    ));
                }

                var row = dtExisting.Rows[0];
                int patientId = Convert.ToInt32(row["PatientID"]);
                int centerId = Convert.ToInt32(row["CenterID"]);

                // Validate new appointment date
                if (request.NewAppointmentDate.Date < DateTime.Today)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Cannot reschedule appointment to a past date"
                    ));
                }

                // Check if patient already has appointment on new date (excluding current appointment)
                bool hasAppointment = await AppointmentsDL.PatientHasAppointmentOnDateAsync(
                    patientId,
                    request.NewAppointmentDate,
                    id
                );

                if (hasAppointment)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Patient already has an appointment scheduled for the new date"
                    ));
                }

                // Check if new slot is available (excluding current appointment)
                bool isSlotAvailable = await AppointmentsDL.IsSlotAvailableAsync(
                    centerId,
                    request.NewAppointmentDate,
                    request.NewSlotStartTime,
                    request.NewSlotEndTime,
                    id
                );

                if (!isSlotAvailable)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Selected time slot is not available for rescheduling"
                    ));
                }

                // Reschedule appointment
                int result = await AppointmentsDL.RescheduleAppointmentAsync(
                    id,
                    request.NewAppointmentDate,
                    request.NewSlotStartTime,
                    request.NewSlotEndTime,
                    request.RescheduleReason,
                    request.ModifiedBy
                );

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Appointment rescheduled successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to reschedule appointment"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error rescheduling appointment: {ex.Message}"
                ));
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Cancel appointment
        /// </summary>
        [HttpDelete("{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id, [FromQuery] int modifiedBy)
        {
            try
            {
                int result = await AppointmentsDL.CancelAppointmentAsync(id, modifiedBy);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Appointment cancelled successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Appointment not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error cancelling appointment: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Delete appointment permanently
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                int result = await AppointmentsDL.DeleteAppointmentAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Appointment deleted successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Appointment not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error deleting appointment: {ex.Message}"
                ));
            }
        }


        /// <summary>
        /// Check if patient has appointment on specific date
        /// </summary>
        [HttpGet("check-patient/{patientId}/{appointmentDate}")]
        public async Task<IActionResult> CheckPatientAppointment(int patientId, DateTime appointmentDate)
        {
            try
            {
                bool hasAppointment = await AppointmentsDL.PatientHasAppointmentOnDateAsync(
                    patientId,
                    appointmentDate
                );

                object? appointmentData = null;

                if (hasAppointment)
                {
                    // Get the appointment details
                    var dt = await AppointmentsDL.GetAppointmentsByPatientIdAsync(patientId);
                    var appointment = dt.AsEnumerable()
                        .Where(row => Convert.ToDateTime(row["AppointmentDate"]).Date == appointmentDate.Date)
                        .FirstOrDefault();

                    if (appointment != null)
                    {
                        appointmentData = new
                        {
                            appointmentID = Convert.ToInt32(appointment["AppointmentID"]),
                            appointmentDate = Convert.ToDateTime(appointment["AppointmentDate"]).ToString("yyyy-MM-dd"),
                            status = appointment["StatusName"]?.ToString(),
                            statusColor = appointment["StatusColor"]?.ToString(),
                            centerName = appointment["CenterName"]?.ToString(),
                            isRescheduled = Convert.ToBoolean(appointment["IsRescheduled"])
                        };
                    }
                }

                return Ok(new ApiResponse<object>(
                    ResponseStatus.Success,
                    hasAppointment
                        ? "Patient already has an appointment on this date"
                        : "Patient is available for appointment on this date",
                    new
                    {
                        hasAppointment = hasAppointment,
                        appointment = appointmentData
                    }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error checking patient appointment: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Check slot availability for a center on specific date
        /// </summary>
        [HttpGet("check-availability/{centerId}/{appointmentDate}")]
        public async Task<IActionResult> CheckSlotAvailability(int centerId, DateTime appointmentDate)
        {
            try
            {
                // Get center configuration
                var dtConfig = await SlotsDL.GetCenterConfigurationAsync(centerId);

                if (dtConfig.Rows.Count == 0)
                {
                    return Ok(ApiResponse<object>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Center configuration not found"
                    ));
                }

                var configRow = dtConfig.Rows[0];
                TimeSpan centerOpenTime = (TimeSpan)configRow["CenterOpenTime"];
                TimeSpan centerCloseTime = (TimeSpan)configRow["CenterCloseTime"];
                int slotDuration = Convert.ToInt32(configRow["SlotDuration"]);

                // Calculate total possible slots
                int totalMinutes = (int)(centerCloseTime - centerOpenTime).TotalMinutes;
                int totalSlots = totalMinutes / slotDuration;

                // Get booked slots count
                var dtBooked = await AppointmentsDL.GetBookedSlotsAsync(centerId, appointmentDate);
                int bookedSlots = dtBooked.Rows.Count;

                // Calculate available slots
                int availableSlots = totalSlots - bookedSlots;
                bool hasAvailableSlots = availableSlots > 0;

                return Ok(new ApiResponse<object>(
                    ResponseStatus.Success,
                    hasAvailableSlots
                        ? $"{availableSlots} slot(s) available"
                        : "No slots available",
                    new
                    {
                        hasAvailableSlots = hasAvailableSlots,
                        totalSlots = totalSlots,
                        bookedSlots = bookedSlots,
                        availableSlots = availableSlots,
                        date = appointmentDate.ToString("yyyy-MM-dd"),
                        centerOpenTime = centerOpenTime.ToString(@"hh\:mm"),
                        centerCloseTime = centerCloseTime.ToString(@"hh\:mm"),
                        slotDuration = slotDuration
                    }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error checking slot availability: {ex.Message}"
                ));
            }
        }


        #endregion

        #region Helper Methods

        private async Task<AppointmentResponse> ConvertRowToAppointmentAsync(DataRow row)
        {
            var appointment = new AppointmentResponse
            {
                AppointmentID = Convert.ToInt32(row["AppointmentID"]),
                PatientID = Convert.ToInt32(row["PatientID"]),
                PatientCode = row["PatientCode"]?.ToString() ?? "",
                PatientName = row["PatientName"]?.ToString() ?? "",
                PatientMobile = row["PatientMobile"]?.ToString() ?? "",
                CenterID = Convert.ToInt32(row["CenterID"]),
                CenterName = row["CenterName"]?.ToString() ?? "",
                CompanyID = Convert.ToInt32(row["CompanyID"]),
                CompanyName = row["CompanyName"]?.ToString() ?? "",
                AppointmentStatus = Convert.ToInt32(row["AppointmentStatus"]),
                StatusName = row["StatusName"]?.ToString() ?? "",
                StatusColor = row["StatusColor"]?.ToString() ?? "",
                AppointmentDate = Convert.ToDateTime(row["AppointmentDate"]),
                IsRescheduled = Convert.ToBoolean(row["IsRescheduled"]),
                RescheduleRevision = Convert.ToInt32(row["RescheduleRevision"]),
                RescheduleReason = row["RescheduleReason"]?.ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };

            // Get slots for this appointment
            var dtSlots = await AppointmentsDL.GetSlotsByAppointmentIdAsync(appointment.AppointmentID);
            appointment.Slots = ConvertDataTableToSlotList(dtSlots);

            return appointment;
        }

        private async Task<List<AppointmentResponse>> ConvertDataTableToAppointmentListAsync(DataTable dt)
        {
            var appointments = new List<AppointmentResponse>();
            foreach (DataRow row in dt.Rows)
            {
                appointments.Add(await ConvertRowToAppointmentAsync(row));
            }
            return appointments;
        }

        private List<SlotResponse> ConvertDataTableToSlotList(DataTable dt)
        {
            var slots = new List<SlotResponse>();
            foreach (DataRow row in dt.Rows)
            {
                slots.Add(new SlotResponse
                {
                    SlotID = Convert.ToInt32(row["SlotID"]),
                    AppointmentID = Convert.ToInt32(row["AppointmentID"]),
                    SlotStartTime = (TimeSpan)row["SlotStartTime"],
                    SlotEndTime = (TimeSpan)row["SlotEndTime"],
                    SlotDate = Convert.ToDateTime(row["SlotDate"])
                });
            }
            return slots;
        }

        #endregion
    }
}
