using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

[Table("ReturnSheet")]
public partial class ReturnSheet
{
    [Key]
    public long id { get; set; }

    [StringLength(50)]
    public string Code { get; set; } = null!;

    public long ReturnSheetTypeId { get; set; }

    public long ReturnId { get; set; }

    public long Sheet { get; set; }

    public DateOnly CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    [ForeignKey("ReturnId")]
    [InverseProperty("ReturnSheets")]
    public virtual Return Return { get; set; } = null!;

    [ForeignKey("ReturnSheetTypeId")]
    [InverseProperty("ReturnSheets")]
    public virtual ReturnSheetType ReturnSheetType { get; set; } = null!;
}
