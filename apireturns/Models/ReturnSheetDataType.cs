using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

[Table("ReturnSheetDataType")]
public partial class ReturnSheetDataType
{
    [Key]
    public long id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    [InverseProperty("DataType")]
    public virtual ICollection<ReturnSheetMatrix> ReturnSheetMatrices { get; set; } = new List<ReturnSheetMatrix>();
}
