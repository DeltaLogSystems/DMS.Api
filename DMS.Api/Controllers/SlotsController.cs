using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        /// <summary>
        /// Get slot availability for a specific date and center (based on active machine count)
        /// </summary>
        [HttpGet("availability")]
        public async Task<IActionResult> GetSlotAvailability(
            [FromQuery] int centerId,
            [FromQuery] DateTime date)
        {
            try
            {
                // Get center configuration
                var dtConfig = await SlotsDL.GetCenterConfigurationAsync(centerId);

                if (dtConfig.Rows.Count == 0)
                {
                    return Ok(ApiResponse<SlotAvailabilityResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Center configuration not found"
                    ));
                }

                var configRow = dtConfig.Rows[0];
                TimeSpan centerOpenTime = (TimeSpan)configRow["CenterOpenTime"];
                TimeSpan centerCloseTime = (TimeSpan)configRow["CenterCloseTime"];
                int slotDuration = Convert.ToInt32(configRow["SlotDuration"]);

                // Get total active dialysis machines for the center
                int totalMachines = await AssetsDL.GetActiveMachineCountAsync(centerId);

                if (totalMachines == 0)
                {
                    return Ok(ApiResponse<SlotAvailabilityResponse>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "No active dialysis machines available at this center"
                    ));
                }

                // Get booked slots for this date
                var dtBooked = await AppointmentsDL.GetBookedSlotsAsync(centerId, date);

                // Generate all possible slots
                var allSlots = GenerateTimeSlots(centerOpenTime, centerCloseTime, slotDuration);

                // Collect slots with machine availability information
                var slots = new List<TimeSlot>();

                foreach (var slot in allSlots)
                {
                    // Count how many machines are booked for this slot
                    int bookedCount = await AppointmentsDL.GetBookedSlotsCountAsync(
                        centerId, date, slot.StartTime, slot.EndTime
                    );

                    int availableCount = totalMachines - bookedCount;
                    bool isAvailable = availableCount > 0;

                    slots.Add(new TimeSlot
                    {
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        IsAvailable = isAvailable,
                        AvailableMachines = availableCount,
                        BookedMachines = bookedCount
                    });
                }

                // Separate available and fully booked slots
                var availableSlots = slots.Where(s => s.IsAvailable).ToList();
                var fullyBookedSlots = slots.Where(s => !s.IsAvailable).ToList();

                var response = new SlotAvailabilityResponse
                {
                    Date = date,
                    CenterOpenTime = centerOpenTime,
                    CenterCloseTime = centerCloseTime,
                    SlotDuration = slotDuration,
                    TotalMachines = totalMachines,
                    AvailableSlots = availableSlots,
                    BookedSlots = fullyBookedSlots
                };

                return Ok(ApiResponse<SlotAvailabilityResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Found {availableSlots.Count} available slot(s) with {totalMachines} machines",
                    response
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SlotAvailabilityResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving slot availability: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Check if specific time slot is available (based on active machine count)
        /// </summary>
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckSlotAvailability(
            [FromQuery] int centerId,
            [FromQuery] DateTime date,
            [FromQuery] string startTime,
            [FromQuery] string endTime)
        {
            try
            {
                TimeSpan slotStartTime = TimeSpan.Parse(startTime);
                TimeSpan slotEndTime = TimeSpan.Parse(endTime);

                // Get total active machines
                int totalMachines = await AssetsDL.GetActiveMachineCountAsync(centerId);

                if (totalMachines == 0)
                {
                    return Ok(new ApiResponse<object>(
                        ResponseStatus.ValidationError,
                        "No active dialysis machines available at this center",
                        new
                        {
                            centerId = centerId,
                            date = date,
                            startTime = startTime,
                            endTime = endTime,
                            isAvailable = false,
                            totalMachines = 0,
                            bookedMachines = 0,
                            availableMachines = 0
                        }
                    ));
                }

                // Get booked count for this slot
                int bookedCount = await AppointmentsDL.GetBookedSlotsCountAsync(
                    centerId, date, slotStartTime, slotEndTime
                );

                int availableCount = totalMachines - bookedCount;
                bool isAvailable = availableCount > 0;

                return Ok(new ApiResponse<object>(
                    isAvailable ? ResponseStatus.Success : ResponseStatus.ValidationError,
                    isAvailable
                        ? $"Time slot is available ({availableCount} of {totalMachines} machines available)"
                        : "Time slot is fully booked (all machines occupied)",
                    new
                    {
                        centerId = centerId,
                        date = date,
                        startTime = startTime,
                        endTime = endTime,
                        isAvailable = isAvailable,
                        totalMachines = totalMachines,
                        bookedMachines = bookedCount,
                        availableMachines = availableCount
                    }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error checking slot availability: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get booked slots for a specific date (grouped by time with machine utilization)
        /// </summary>
        [HttpGet("booked")]
        public async Task<IActionResult> GetBookedSlots(
            [FromQuery] int centerId,
            [FromQuery] DateTime date)
        {
            try
            {
                // Get total active machines
                int totalMachines = await AssetsDL.GetActiveMachineCountAsync(centerId);

                var dt = await AppointmentsDL.GetBookedSlotsAsync(centerId, date);
                var bookedSlots = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    bookedSlots.Add(new
                    {
                        slotID = Convert.ToInt32(row["SlotID"]),
                        appointmentID = Convert.ToInt32(row["AppointmentID"]),
                        startTime = row["SlotStartTime"]?.ToString(),
                        endTime = row["SlotEndTime"]?.ToString(),
                        patientName = row["PatientName"]?.ToString(),
                        patientCode = row["PatientCode"]?.ToString(),
                        status = row["StatusName"]?.ToString(),
                        statusColor = row["StatusColor"]?.ToString()
                    });
                }

                return Ok(ApiResponse<object>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {bookedSlots.Count} booked slot(s) from {totalMachines} total machines",
                    new
                    {
                        totalMachines = totalMachines,
                        bookedAppointmentsCount = bookedSlots.Count,
                        slots = bookedSlots
                    }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving booked slots: {ex.Message}"
                ));
            }
        }

        #region Helper Methods

        /// <summary>
        /// Generate time slots based on center working hours
        /// </summary>
        private List<TimeSlot> GenerateTimeSlots(TimeSpan openTime, TimeSpan closeTime, int durationMinutes)
        {
            var slots = new List<TimeSlot>();
            TimeSpan currentTime = openTime;

            while (currentTime.Add(TimeSpan.FromMinutes(durationMinutes)) <= closeTime)
            {
                TimeSpan endTime = currentTime.Add(TimeSpan.FromMinutes(durationMinutes));

                slots.Add(new TimeSlot
                {
                    StartTime = NormalizeTimeSpan(currentTime),
                    EndTime = NormalizeTimeSpan(endTime),
                    IsAvailable = true
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(durationMinutes));
            }

            return slots;
        }

        /// <summary>
        /// Normalize TimeSpan to 24-hour cycle (0-23:59:59)
        /// </summary>
        private TimeSpan NormalizeTimeSpan(TimeSpan time)
        {
            int totalHours = (int)time.TotalHours;
            int normalizedHours = totalHours % 24;
            int minutes = time.Minutes;
            int seconds = time.Seconds;

            return new TimeSpan(normalizedHours, minutes, seconds);
        }

        #endregion
    }
}
