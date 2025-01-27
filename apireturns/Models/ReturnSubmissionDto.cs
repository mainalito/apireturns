namespace apireturns.Models
{
    public class ReturnSubmissionDto
    {
        public int? id { get; set; }
        public int EntityId { get; set; }
        public int ReturnId { get; set; }
        public int? Month { get; set; }
        public int? Quarter { get; set; }
        public int Year { get; set; }
        public string ReportPeriod { get; set; }
        public string FileName { get; set; }
        public int StatusId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
