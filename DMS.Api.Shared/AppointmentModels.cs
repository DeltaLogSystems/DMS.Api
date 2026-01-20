using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    #region Appointment Models

    /// <summary>
    /// Appointment request model
    /// </summary>
    public class AppointmentRequest
    {
        public int PatientID { get; set; }
        public int CenterID { get; set; }
        public int CompanyID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan SlotStartTime { get; set; }
        public TimeSpan SlotEndTime { get; set; }
        public int CreatedBy { get; set; }
    }

    /// <summary>
    /// Appointment response model
    /// </summary>
    public class AppointmentResponse
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientMobile { get; set; } = string.Empty;
        public int CenterID { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int AppointmentStatus { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public bool IsRescheduled { get; set; }
        public int RescheduleRevision { get; set; }
        public string? RescheduleReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<SlotResponse> Slots { get; set; } = new List<SlotResponse>();
    }

    /// <summary>
    /// Slot response model
    /// </summary>
    public class SlotResponse
    {
        public int SlotID { get; set; }
        public int AppointmentID { get; set; }
        public TimeSpan SlotStartTime { get; set; }
        public TimeSpan SlotEndTime { get; set; }
        public DateTime SlotDate { get; set; }
        public string SlotTimeRange => $"{FormatTime(SlotStartTime)} - {FormatTime(SlotEndTime)}";

        /// <summary>
        /// Format TimeSpan to handle times crossing midnight (normalize to 24-hour cycle)
        /// </summary>
        private string FormatTime(TimeSpan time)
        {
            // Normalize time to 24-hour cycle (handle times >= 24:00)
            int totalHours = (int)time.TotalHours;
            int normalizedHours = totalHours % 24;
            int minutes = time.Minutes;

            // Create a normalized TimeSpan
            var normalizedTime = new TimeSpan(normalizedHours, minutes, 0);

            // Format in 12-hour format with AM/PM
            DateTime dateTime = DateTime.Today.Add(normalizedTime);
            return dateTime.ToString("hh:mm tt");
        }
    }

    /// <summary>
    /// Reschedule appointment request
    /// </summary>
    public class RescheduleAppointmentRequest
    {
        public int AppointmentID { get; set; }
        public DateTime NewAppointmentDate { get; set; }
        public TimeSpan NewSlotStartTime { get; set; }
        public TimeSpan NewSlotEndTime { get; set; }
        public string RescheduleReason { get; set; } = string.Empty;
        public int ModifiedBy { get; set; }
    }

    /// <summary>
    /// Update appointment status request
    /// </summary>
    public class UpdateAppointmentStatusRequest
    {
        public int AppointmentID { get; set; }
        public int NewStatus { get; set; }
        public int ModifiedBy { get; set; }
    }

    /// <summary>
    /// Slot availability request
    /// </summary>
    public class SlotAvailabilityRequest
    {
        public int CenterID { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Slot availability response
    /// </summary>
    public class SlotAvailabilityResponse
    {
        public DateTime Date { get; set; }
        public TimeSpan CenterOpenTime { get; set; }
        public TimeSpan CenterCloseTime { get; set; }
        public int SlotDuration { get; set; }
        public List<TimeSlot> AvailableSlots { get; set; } = new List<TimeSlot>();
        public List<TimeSlot> BookedSlots { get; set; } = new List<TimeSlot>();
    }

    /// <summary>
    /// Time slot model
    /// </summary>
    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string TimeRange => $"{FormatTime(StartTime)} - {FormatTime(EndTime)}";
        public int? AppointmentID { get; set; }
        public string? PatientName { get; set; }

        /// <summary>
        /// Format TimeSpan to handle times crossing midnight (normalize to 24-hour cycle)
        /// </summary>
        private string FormatTime(TimeSpan time)
        {
            // Normalize time to 24-hour cycle (handle times >= 24:00)
            int totalHours = (int)time.TotalHours;
            int normalizedHours = totalHours % 24;
            int minutes = time.Minutes;

            // Create a normalized TimeSpan
            var normalizedTime = new TimeSpan(normalizedHours, minutes, 0);

            // Format in 12-hour format with AM/PM
            DateTime dateTime = DateTime.Today.Add(normalizedTime);
            return dateTime.ToString("hh:mm tt");
        }
    }

    /// <summary>
    /// Appointment status response
    /// </summary>
    public class AppointmentStatusResponse
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
    }

    #endregion
}
