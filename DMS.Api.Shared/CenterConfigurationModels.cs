namespace DMS.Api.Shared
{
    public class CenterConfigurationResponse
    {
        public int ConfigurationID { get; set; }
        public int CenterID { get; set; }
        public decimal? MachineSessionHours { get; set; }
        public bool IsFixedHoursForSession { get; set; }
        public TimeSpan CenterOpenTime { get; set; }
        public TimeSpan CenterCloseTime { get; set; }
        public int SlotDuration { get; set; }
        public bool AutoCancelOverdueAppointments { get; set; }
        public bool AutoCancelOverdueSessions { get; set; }
        public int OverdueThresholdHours { get; set; }
    }

    public class UpdateCenterConfigurationRequest
    {
        public decimal? MachineSessionHours { get; set; }
        public bool IsFixedHoursForSession { get; set; }
        public TimeSpan CenterOpenTime { get; set; }
        public TimeSpan CenterCloseTime { get; set; }
        public int SlotDuration { get; set; }
        public bool AutoCancelOverdueAppointments { get; set; }
        public bool AutoCancelOverdueSessions { get; set; }
        public int OverdueThresholdHours { get; set; }
    }
}
