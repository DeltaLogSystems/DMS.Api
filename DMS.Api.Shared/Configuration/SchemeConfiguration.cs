namespace DMS.Api.Shared.Configuration
{
    /// <summary>
    /// Configuration constants for dialysis scheme types
    /// </summary>
    public static class SchemeConfiguration
    {
        /// <summary>
        /// MJPJAY (Mahatma Jyotiba Phule Jan Arogya Yojana) scheme configuration
        /// </summary>
        public static class MJPJAY
        {
            public const int SessionsPerCycle = 18;
            public const int DaysBetweenSessions = 2;
            public const int CycleDurationDays = 42;
            public const string SchemeCode = "MJPJAY";
        }

        /// <summary>
        /// Standard/Default dialysis cycle configuration
        /// </summary>
        public static class Standard
        {
            public const int SessionsPerCycle = 12;
            public const int DaysBetweenSessions = 3;
            public const int CycleDurationDays = 36;
        }
    }
}
