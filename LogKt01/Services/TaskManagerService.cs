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
		_logger.LogTrace("Initialized {ServiceName}", nameof(TaskManagerService));
	}

	public async Task<List<TaskDto>> GetAllTasks()
	{
		_logger.LogTrace("Fetching all tasks from database");

		var tasks = _context.Tasks
			.Where(x => !x.IsMarkedAsRemoved)
			.OrderBy(x => x.Id)
			.Select(x => new TaskDto
			{
				Id = x.Id,
				Title = x.Title,
				Category = x.Category,
				IsDone = x.IsDone,
				CreatedAt = x.CreatedAt,
			});

		var tasksList = await tasks.ToListAsync();

		_logger.LogInformation("Retrieved {Count} tasks", tasksList.Count);

		return tasksList;
	}

	public async Task<bool> AddTask(string title, string category)
	{
		_logger.LogTrace("Attempting to add task. Title: {Title}, Category: {Category}", title, category);

		if (string.IsNullOrEmpty(title) || title.Length > 80)
		{
			_logger.LogWarning("Validation failed: Title is invalid");
			return false;
		}

		if (string.IsNullOrEmpty(category) || category.Length > 50)
		{
			_logger.LogWarning("Validation failed: Category is invalid");
			return false;
		}

		var task = new TaskEntity
		{
			Title = title,
			Category = category,
			CreatedAt = DateTime.UtcNow,
			IsDone = false,
		};

		_context.Tasks.Add(task);
		await _context.SaveChangesAsync();

		_logger.LogInformation("Task added successfully. Id: {Id}", task.Id);

		return true;
	}

	public async Task<bool> ToggleTaskStatus(int id)
	{
		_logger.LogTrace("Toggling status for task {Id}", id);

		var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsMarkedAsRemoved);
		if (task == null)
		{
			_logger.LogWarning("Task {Id} not found or removed", id);
			return false;
		}

		task.IsDone = !task.IsDone;
		await _context.SaveChangesAsync();

		_logger.LogInformation("Task {Id} status updated. IsDone: {IsDone}", id, task.IsDone);

		return true;
	}

	public async Task<bool> RemoveTask(int id)
	{
		_logger.LogTrace("Attempting to remove task {Id}", id);

		var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsMarkedAsRemoved);
		if (task == null)
		{
			_logger.LogWarning("Task {Id} not found for removal", id);
			return false;
		}

		task.IsMarkedAsRemoved = true;
		await _context.SaveChangesAsync();

		_logger.LogInformation("Task {Id} marked as removed", id);

		return true;
	}
}
