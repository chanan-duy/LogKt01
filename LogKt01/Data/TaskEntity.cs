using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LogKt01.Data;

[Table("tasks")]
[PrimaryKey(nameof(Id))]
public class TaskEntity
{
	[Column("Id")] public int Id { get; set; }
}
