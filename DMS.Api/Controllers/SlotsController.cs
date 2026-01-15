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
        /// Get slot availability for a specific date and center
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

                // Get booked slots for this date
                var dtBooked = await AppointmentsDL.GetBookedSlotsAsync(centerId, date);

                // Generate all possible slots
                var allSlots = GenerateTimeSlots(centerOpenTime, centerCloseTime, slotDuration);

                // Mark booked slots
                var availableSlots = new List<TimeSlot>();
                var bookedSlots = new List<TimeSlot>();

                foreach (var slot in allSlots)
                {
                    bool isBooked = false;
                    DataRow? bookedRow = null;

                    foreach (DataRow row in dtBooked.Rows)
                    {
                        TimeSpan bookedStart = (TimeSpan)row["SlotStartTime"];
                        TimeSpan bookedEnd = (TimeSpan)row["SlotEndTime"];

                        // Check if slots overlap
                        if ((slot.StartTime >= bookedStart && slot.StartTime < bookedEnd) ||
                            (slot.EndTime > bookedStart && slot.EndTime <= bookedEnd) ||
                            (slot.StartTime <= bookedStart && slot.EndTime >= bookedEnd))
                        {
                            isBooked = true;
                            bookedRow = row;
                            break;
                        }
                    }

                    if (isBooked && bookedRow != null)
                    {
                        bookedSlots.Add(new TimeSlot
                        {
                            StartTime = (TimeSpan)bookedRow["SlotStartTime"],
                            EndTime = (TimeSpan)bookedRow["SlotEndTime"],
                            IsAvailable = false,
                            AppointmentID = Convert.ToInt32(bookedRow["AppointmentID"]),
                            PatientName = bookedRow["PatientName"]?.ToString()
                        });
                    }
                    else
                    {
                        availableSlots.Add(new TimeSlot
                        {
                            StartTime = slot.StartTime,
                            EndTime = slot.EndTime,
                            IsAvailable = true
                        });
                    }
                }

                var response = new SlotAvailabilityResponse
                {
                    Date = date,
                    CenterOpenTime = centerOpenTime,
                    CenterCloseTime = centerCloseTime,
                    SlotDuration = slotDuration,
                    AvailableSlots = availableSlots,
                    BookedSlots = bookedSlots
                };

                return Ok(ApiResponse<SlotAvailabilityResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Found {availableSlots.Count} available slot(s) and {bookedSlots.Count} booked slot(s)",
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
        /// Check if specific time slot is available
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

                bool isAvailable = await AppointmentsDL.IsSlotAvailableAsync(
                    centerId,
                    date,
                    slotStartTime,
                    slotEndTime
                );

                return Ok(new ApiResponse<object>(
                    isAvailable ? ResponseStatus.Success : ResponseStatus.ValidationError,
                    isAvailable ? "Time slot is available" : "Time slot is not available",
                    new
                    {
                        centerId = centerId,
                        date = date,
                        startTime = startTime,
                        endTime = endTime,
                        isAvailable = isAvailable
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
        /// Get booked slots for a specific date
        /// </summary>
        [HttpGet("booked")]
        public async Task<IActionResult> GetBookedSlots(
            [FromQuery] int centerId,
            [FromQuery] DateTime date)
        {
            try
            {
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

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {bookedSlots.Count} booked slot(s)",
                    bookedSlots
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
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
                slots.Add(new TimeSlot
                {
                    StartTime = currentTime,
                    EndTime = currentTime.Add(TimeSpan.FromMinutes(durationMinutes)),
                    IsAvailable = true
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(durationMinutes));
            }

            return slots;
        }

        #endregion
    }
}
