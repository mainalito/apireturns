using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace apireturns.Models;

public partial class ReturnTemplate
{
    [Key]
    public long id { get; set; }

    public long ReturnId { get; set; }

    public long EntityRoleTypeId { get; set; }

    [Column(TypeName = "text")]
    public string FileName { get; set; } = null!;

    public bool InActive { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(50)]
    public string Version { get; set; } = null!;

    public DateOnly CreatedDate { get; set; }

    public long CreatedBy { get; set; }
}
