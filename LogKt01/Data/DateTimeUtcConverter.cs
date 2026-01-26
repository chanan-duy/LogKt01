using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LogKt01.Data;

public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
	public DateTimeUtcConverter()
		: base(
			d => d.ToUniversalTime(),
			d => DateTime.SpecifyKind(d, DateTimeKind.Utc))
	{
	}
}
