using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

public partial class ReturnSheetDatum
{
    [Key]
    public long id { get; set; }

    public long SubmissionId { get; set; }

    public long ReturnSheetId { get; set; }

    public long MatrixId { get; set; }

    public long? RecordId { get; set; }

    public string Data { get; set; } = null!;

    public bool Deleted { get; set; }

    public DateOnly? DeletedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    [ForeignKey("MatrixId")]
    [InverseProperty("ReturnSheetData")]
    public virtual ReturnSheetMatrix Matrix { get; set; } = null!;

    [ForeignKey("SubmissionId")]
    [InverseProperty("ReturnSheetData")]
    public virtual ReturnSubmission Submission { get; set; } = null!;
}
