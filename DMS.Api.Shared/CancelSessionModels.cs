namespace DMS.Api.Shared
{
    public class CancelSessionRequest
    {
        public string CancellationReason { get; set; } = string.Empty;
        public int CancelledBy { get; set; }
    }

    public class BulkCancelSessionsRequest
    {
        public List<int> SessionIds { get; set; } = new List<int>();
        public string CancellationReason { get; set; } = string.Empty;
        public int CancelledBy { get; set; }
    }

    public class BulkOperationResult
    {
        public int SessionID { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BulkCancelResponse
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<BulkOperationResult> Results { get; set; } = new List<BulkOperationResult>();
    }
}
