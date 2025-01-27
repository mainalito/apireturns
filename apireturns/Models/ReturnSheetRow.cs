using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

public partial class ReturnSheetRow
{
    [Key]
    public long id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public long ReturnSheetId { get; set; }

    public long? RowHeaderId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    [InverseProperty("Row")]
    public virtual ICollection<ReturnSheetMatrix> ReturnSheetMatrices { get; set; } = new List<ReturnSheetMatrix>();
}
