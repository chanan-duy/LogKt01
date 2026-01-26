using LogKt01.Data;
using LogKt01.Dto;
using Microsoft.EntityFrameworkCore;

namespace LogKt01.Services;

public class TaskManagerService
{
	private readonly ILogger<TaskManagerService> _logger;
	private readonly AppDbContext _context;

	public TaskManagerService(ILogger<TaskManagerService> logger, AppDbContext context)
	{
		_logger = logger;
		_context = context;
		logger.LogTrace("Initialized {ServiceName}", nameof(TaskManagerService));
	}

	public async Task<List<TaskDto>> GetAllTasks()
	{
		var tasks = _context.Tasks
			.Where(x => !x.IsMarkedAsRemoved)
			.Select(x => new TaskDto
			{
				Id = x.Id,
				Title = x.Title,
				Category = x.Category,
				IsDone = x.IsDone,
				CreatedAt = x.CreatedAt,
			});

		var tasksList = await tasks.ToListAsync();

		return tasksList;
	}

	public async Task<bool> AddTask(string title, string category)
	{
		if (string.IsNullOrEmpty(title) || title.Length > 80)
		{
			return false;
		}

		if (string.IsNullOrEmpty(category) || category.Length > 50)
		{
			return false;
		}

		var task = new TaskEntity
		{
			Title = title,
			Category = category,
			CreatedAt = DateTime.UtcNow,
		};

		_context.Tasks.Add(task);

		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<bool> RemoveTask(int id)
	{
		var task = _context.Tasks.FirstOrDefault(x => x.Id == id && !x.IsMarkedAsRemoved);
		if (task == null)
		{
			return false;
		}

		task.IsMarkedAsRemoved = true;

		await _context.SaveChangesAsync();

		return true;
	}
}
