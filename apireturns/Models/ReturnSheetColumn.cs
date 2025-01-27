using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

public partial class ReturnSheetColumn
{
    [Key]
    public long id { get; set; }

    [StringLength(3000)]
    public string Name { get; set; } = null!;

    public long ReturnSheetId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    [InverseProperty("Column")]
    public virtual ICollection<ReturnSheetMatrix> ReturnSheetMatrices { get; set; } = new List<ReturnSheetMatrix>();
}
