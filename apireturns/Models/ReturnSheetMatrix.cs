using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

[Table("ReturnSheetMatrix")]
public partial class ReturnSheetMatrix
{
    [Key]
    public long id { get; set; }

    public long RowId { get; set; }

    public long ColumnId { get; set; }

    public long DataTypeId { get; set; }

    [Column(TypeName = "text")]
    public string Cell { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    [ForeignKey("ColumnId")]
    [InverseProperty("ReturnSheetMatrices")]
    public virtual ReturnSheetColumn Column { get; set; } = null!;

    [ForeignKey("DataTypeId")]
    [InverseProperty("ReturnSheetMatrices")]
    public virtual ReturnSheetDataType DataType { get; set; } = null!;

    [InverseProperty("Matrix")]
    public virtual ICollection<ReturnSheetDatum> ReturnSheetData { get; set; } = new List<ReturnSheetDatum>();

    [ForeignKey("RowId")]
    [InverseProperty("ReturnSheetMatrices")]
    public virtual ReturnSheetRow Row { get; set; } = null!;
}
