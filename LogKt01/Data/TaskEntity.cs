using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace LogKt01.Data;

[Table("tasks")]
[PrimaryKey(nameof(Id))]
public class TaskEntity
{
	[Column("id")] public int Id { get; set; }
	[Column("title")] [StringLength(80)] public string Title { get; set; } = string.Empty;
	[Column("category")] [StringLength(50)] public string Category { get; set; } = string.Empty;
	[Column("is_done")] public bool IsDone { get; set; }
	[Column("created_at")] public DateTime CreatedAt { get; set; }
	[Column("marked_as_removed")] public bool IsMarkedAsRemoved { get; set; }
}
