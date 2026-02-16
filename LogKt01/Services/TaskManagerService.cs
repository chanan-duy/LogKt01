using System.Diagnostics;
using LogKt01.Data;
using LogKt01.Dto;
using Microsoft.EntityFrameworkCore;

namespace LogKt01.Services;

public class TaskManagerService
{
	private readonly AppDbContext _context;
	private readonly ILogger<TaskManagerService> _logger;

	public TaskManagerService(ILogger<TaskManagerService> logger, AppDbContext context)
	{
		_logger = logger;
		_context = context;
		_logger.LogTrace("Initialized {ServiceName}", nameof(TaskManagerService));
	}

	public async Task<List<TaskDto>> GetAllTasks()
	{
		const string operation = nameof(GetAllTasks);
		var stopwatch = Stopwatch.StartNew();
		using var scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["Operation"] = operation,
		});

		_logger.LogTrace(LogEvents.GetAllTasksStarted, "Fetching tasks from database");

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
		var doneCount = tasksList.Count(x => x.IsDone);
		stopwatch.Stop();

		_logger.LogInformation(
			LogEvents.GetAllTasksCompleted,
			"Retrieved tasks list. Count: {TaskCount}, DoneCount: {DoneCount}, ElapsedMs: {ElapsedMs}",
			tasksList.Count,
			doneCount,
			stopwatch.ElapsedMilliseconds);

		return tasksList;
	}

	public async Task<bool> AddTask(string? title, string? category)
	{
		const string operation = nameof(AddTask);
		var stopwatch = Stopwatch.StartNew();
		using var scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["Operation"] = operation,
			["TitleLength"] = title?.Length ?? 0,
			["CategoryLength"] = category?.Length ?? 0,
		});

		_logger.LogTrace(
			"Attempting to add task. Category: {Category}, TitleLength: {TitleLength}, CategoryLength: {CategoryLength}",
			category,
			title?.Length ?? 0,
			category?.Length ?? 0);

		if (string.IsNullOrEmpty(title) || title.Length > 80)
		{
			_logger.LogWarning(
				LogEvents.AddTaskValidationFailed,
				"Validation failed for new task. Field: {Field}, Rule: {Rule}, ActualLength: {ActualLength}, MaxLength: {MaxLength}",
				"Title",
				"RequiredAndMaxLength",
				title?.Length ?? 0,
				80);
			return false;
		}

		if (string.IsNullOrEmpty(category) || category.Length > 50)
		{
			_logger.LogWarning(
				LogEvents.AddTaskValidationFailed,
				"Validation failed for new task. Field: {Field}, Rule: {Rule}, ActualLength: {ActualLength}, MaxLength: {MaxLength}",
				"Category",
				"RequiredAndMaxLength",
				category?.Length ?? 0,
				50);
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
		stopwatch.Stop();

		_logger.LogInformation(
			LogEvents.AddTaskCompleted,
			"Task added successfully. TaskId: {TaskId}, Category: {Category}, CreatedAtUtc: {CreatedAtUtc}, ElapsedMs: {ElapsedMs}",
			task.Id,
			task.Category,
			task.CreatedAt,
			stopwatch.ElapsedMilliseconds);

		return true;
	}

	public async Task<bool> ToggleTaskStatus(int id)
	{
		const string operation = nameof(ToggleTaskStatus);
		var stopwatch = Stopwatch.StartNew();
		using var scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["Operation"] = operation,
			["TaskId"] = id,
		});

		_logger.LogTrace("Toggling task status. TaskId: {TaskId}", id);

		var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsMarkedAsRemoved);
		if (task == null)
		{
			_logger.LogWarning(LogEvents.ToggleTaskNotFound, "Task not found or already removed. TaskId: {TaskId}", id);
			return false;
		}

		var previousIsDone = task.IsDone;
		task.IsDone = !task.IsDone;
		await _context.SaveChangesAsync();
		stopwatch.Stop();

		_logger.LogInformation(
			LogEvents.ToggleTaskCompleted,
			"Task status updated. TaskId: {TaskId}, PreviousIsDone: {PreviousIsDone}, CurrentIsDone: {CurrentIsDone}, ElapsedMs: {ElapsedMs}",
			id,
			previousIsDone,
			task.IsDone,
			stopwatch.ElapsedMilliseconds);

		return true;
	}

	public async Task<bool> RemoveTask(int id)
	{
		const string operation = nameof(RemoveTask);
		var stopwatch = Stopwatch.StartNew();
		using var scope = _logger.BeginScope(new Dictionary<string, object>
		{
			["Operation"] = operation,
			["TaskId"] = id,
		});

		_logger.LogTrace("Attempting to remove task. TaskId: {TaskId}", id);

		var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsMarkedAsRemoved);
		if (task == null)
		{
			_logger.LogWarning(LogEvents.RemoveTaskNotFound, "Task not found for removal. TaskId: {TaskId}", id);
			return false;
		}

		task.IsMarkedAsRemoved = true;
		await _context.SaveChangesAsync();
		stopwatch.Stop();

		_logger.LogInformation(
			LogEvents.RemoveTaskCompleted,
			"Task marked as removed. TaskId: {TaskId}, RemovedAtUtc: {RemovedAtUtc}, ElapsedMs: {ElapsedMs}",
			id,
			DateTime.UtcNow,
			stopwatch.ElapsedMilliseconds);

		return true;
	}

	private static class LogEvents
	{
		public static readonly EventId GetAllTasksStarted = new(1000, nameof(GetAllTasksStarted));
		public static readonly EventId GetAllTasksCompleted = new(1001, nameof(GetAllTasksCompleted));
		public static readonly EventId AddTaskValidationFailed = new(1100, nameof(AddTaskValidationFailed));
		public static readonly EventId AddTaskCompleted = new(1101, nameof(AddTaskCompleted));
		public static readonly EventId ToggleTaskNotFound = new(1200, nameof(ToggleTaskNotFound));
		public static readonly EventId ToggleTaskCompleted = new(1201, nameof(ToggleTaskCompleted));
		public static readonly EventId RemoveTaskNotFound = new(1300, nameof(RemoveTaskNotFound));
		public static readonly EventId RemoveTaskCompleted = new(1301, nameof(RemoveTaskCompleted));
	}
}
