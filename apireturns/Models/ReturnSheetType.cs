using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

[Table("ReturnSheetType")]
public partial class ReturnSheetType
{
    [Key]
    public long id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    [InverseProperty("ReturnSheetType")]
    public virtual ICollection<ReturnSheet> ReturnSheets { get; set; } = new List<ReturnSheet>();
}
