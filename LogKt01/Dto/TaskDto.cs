namespace LogKt01.Dto;

public record TaskDto
{
	public required int Id;
	public required string Title;
	public required string Category;
	public required bool IsDone;
	public required DateTime CreatedAt;
}
