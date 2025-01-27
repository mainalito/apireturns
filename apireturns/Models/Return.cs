using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

public partial class Return
{
    [Key]
    public long id { get; set; }

    [StringLength(50)]
    public string Code { get; set; } = null!;

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public long PeriodTypeId { get; set; }

    public string Notes { get; set; } = null!;

    public long? WorkflowId { get; set; }

    public bool Disabled { get; set; }

    public DateOnly CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    [InverseProperty("Return")]
    public virtual ICollection<ReturnSheet> ReturnSheets { get; set; } = new List<ReturnSheet>();

    [InverseProperty("Return")]
    public virtual ICollection<ReturnSubmission> ReturnSubmissions { get; set; } = new List<ReturnSubmission>();
}
