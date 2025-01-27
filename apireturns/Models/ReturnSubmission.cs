using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

[Table("ReturnSubmission")]
public partial class ReturnSubmission
{
    [Key]
    public long id { get; set; }

    public long ReturnId { get; set; }

    public long EntityId { get; set; }

    public DateOnly ReportPeriod { get; set; }

    public int? Month { get; set; }

    public int? Quarter { get; set; }

    public int? Year { get; set; }

    public bool Declaration { get; set; }

    public string? FileName { get; set; }

    public bool NilReturn { get; set; }

    public bool Resubmission { get; set; }

    public long? SubmissionId { get; set; }

    public long? StatusId { get; set; }

    public long? ReturnResubmissionRequestId { get; set; }

    public string? Notes { get; set; }

    public DateOnly CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    public bool Deleted { get; set; }

    public DateOnly? DateDeleted { get; set; }

    public DateOnly? DateSubmitted { get; set; }

    public long? SubmittedBy { get; set; }

    public long? DeletedBy { get; set; }

    [ForeignKey("ReturnId")]
    [InverseProperty("ReturnSubmissions")]
    public virtual Return Return { get; set; } = null!;

    [InverseProperty("Submission")]
    public virtual ICollection<ReturnSheetDatum> ReturnSheetData { get; set; } = new List<ReturnSheetDatum>();
}
