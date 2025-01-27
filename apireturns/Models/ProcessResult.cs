namespace apireturns.Models
{
    public class ProcessResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long? return_submission_id { get; set; }
    }
}
