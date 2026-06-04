using Microsoft.EntityFrameworkCore;
using NexusBackend.Data;
using NexusBackend.DTOs;
using NexusBackend.Models;

namespace NexusBackend.Services;

public interface ITaskService
{
    Task<List<TaskDTO>> GetAllAsync(Guid projectId, Guid userId);
    Task<TaskDTO> GetByIdAsync(Guid id, Guid userId);
    Task<TaskDTO> CreateAsync(CreateTaskDTO dto, Guid userId);
    Task<TaskDTO> UpdateAsync(Guid id, UpdateTaskDTO dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<TaskDTO> ChangeStatusAsync(Guid id, ChangeStatusDTO dto, Guid userId);
}

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    // BUG FIX: Status values الصح (مطابقة للـ frontend)
    private static readonly HashSet<string> ValidStatuses = ["todo", "in-progress", "done"];
    private static readonly HashSet<string> ValidPriorities = ["low", "medium", "high", "urgent"];

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    private static DateTime? ToUtcNullable(DateTime? dt) =>
        dt.HasValue ? ToUtc(dt.Value) : null;

    public async Task<List<TaskDTO>> GetAllAsync(Guid projectId, Guid userId)
    {
        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == projectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

        if (!hasAccess)
            throw new UnauthorizedAccessException("You are not a member of this project.");

        var tasks = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .Include(t => t.Project)
            .Include(t => t.Collaborators).ThenInclude(c => c.User)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDTO).ToList();
    }

    public async Task<TaskDTO> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .Include(t => t.Project)
            .Include(t => t.Collaborators).ThenInclude(c => c.User)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new KeyNotFoundException("Task not found.");

        // BUG FIX: GetByIdAsync مكانتش بتتحقق من الصلاحية
        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == task.ProjectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

        if (!hasAccess)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        return MapToDTO(task);
    }

    public async Task<TaskDTO> CreateAsync(CreateTaskDTO dto, Guid userId)
    {
        // BUG FIX: Validate status and priority values
        var status = dto.Status?.ToLower() ?? "todo";
        var priority = dto.Priority?.ToLower() ?? "medium";

        if (!ValidStatuses.Contains(status))
            throw new ArgumentException($"Invalid status '{dto.Status}'. Valid values: todo, in-progress, done.");

        if (!ValidPriorities.Contains(priority))
            throw new ArgumentException($"Invalid priority '{dto.Priority}'. Valid values: low, medium, high, urgent.");

        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == dto.ProjectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

        if (!hasAccess)
            throw new UnauthorizedAccessException("You are not a member of this project.");

        // BUG FIX: Validate assigneeId is actually a member of the project
        if (dto.AssigneeId.HasValue)
        {
            var isMember = await _context.Projects
                .AnyAsync(p => p.Id == dto.ProjectId &&
                    (p.OwnerId == dto.AssigneeId.Value || p.Members.Any(m => m.UserId == dto.AssigneeId.Value)));
            if (!isMember)
                throw new ArgumentException("Assignee is not a member of this project.");
        }

        var task = new Task_
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = status,
            Priority = priority,
            DueDate = ToUtcNullable(dto.DueDate),
            ProjectId = dto.ProjectId,
            AssigneeId = dto.AssigneeId,
            CreatedById = userId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(task.Id, userId);
    }

    public async Task<TaskDTO> UpdateAsync(Guid id, UpdateTaskDTO dto, Guid userId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new KeyNotFoundException("Task not found.");

        // BUG FIX: كان بيسمح لأي حد يعدل أي task حتى لو مش في البروجكت
        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == task.ProjectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

        if (!hasAccess)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        // BUG FIX: Validate status and priority if provided
        if (dto.Status != null)
        {
            var status = dto.Status.ToLower();
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException($"Invalid status '{dto.Status}'.");
            task.Status = status;
        }

        if (dto.Priority != null)
        {
            var priority = dto.Priority.ToLower();
            if (!ValidPriorities.Contains(priority))
                throw new ArgumentException($"Invalid priority '{dto.Priority}'.");
            task.Priority = priority;
        }

        if (dto.Title != null) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.Progress.HasValue) task.Progress = dto.Progress.Value;
        if (dto.DueDate.HasValue) task.DueDate = ToUtc(dto.DueDate.Value);

        // BUG FIX: Allow clearing description (set to null)
        // NOTE: لو حابب تمسح الـ description ابعت string فاضية ""
        if (dto.Description == "") task.Description = null;

        if (dto.AssigneeId.HasValue)
        {
            // BUG FIX: Validate assignee is project member
            var isMember = await _context.Projects
                .AnyAsync(p => p.Id == task.ProjectId &&
                    (p.OwnerId == dto.AssigneeId.Value || p.Members.Any(m => m.UserId == dto.AssigneeId.Value)));
            if (!isMember)
                throw new ArgumentException("Assignee is not a member of this project.");
            task.AssigneeId = dto.AssigneeId.Value;
        }

        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, userId);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new KeyNotFoundException("Task not found.");

        // BUG FIX: كان بيسمح لأي حد يحذف أي task
        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == task.ProjectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

        if (!hasAccess)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<TaskDTO> ChangeStatusAsync(Guid id, ChangeStatusDTO dto, Guid userId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new KeyNotFoundException("Task not found.");

        // BUG FIX: Authorization check كان مفقود
        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == task.ProjectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

        if (!hasAccess)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        // BUG FIX: Validate status value
        var status = dto.Status.ToLower();
        if (!ValidStatuses.Contains(status))
            throw new ArgumentException($"Invalid status '{dto.Status}'. Valid values: todo, in-progress, done.");

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        if (status == "done") task.Progress = 100;
        else if (status == "in-progress" && task.Progress == 0) task.Progress = 10;
        // BUG FIX: لو رجع todo نرجع الـ progress للـ 0
        else if (status == "todo") task.Progress = 0;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, userId);
    }

    private static TaskDTO MapToDTO(Task_ t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        Progress = t.Progress,
        DueDate = t.DueDate,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        ProjectId = t.ProjectId,
        ProjectName = t.Project.Name,
        Assignee = t.Assignee == null ? null : new UserDTO
        {
            Id = t.Assignee.Id,
            FullName = t.Assignee.FullName,
            Email = t.Assignee.Email!,
            AvatarUrl = t.Assignee.AvatarUrl,
            CreatedAt = t.Assignee.CreatedAt
        },
        CreatedBy = new UserDTO
        {
            Id = t.CreatedBy.Id,
            FullName = t.CreatedBy.FullName,
            Email = t.CreatedBy.Email!,
            AvatarUrl = t.CreatedBy.AvatarUrl,
            CreatedAt = t.CreatedBy.CreatedAt
        },
        Collaborators = t.Collaborators.Select(c => new ProjectMemberDTO
        {
            UserId = c.UserId,
            FullName = c.User.FullName,
            Email = c.User.Email!,
            AvatarUrl = c.User.AvatarUrl,
        }).ToList(),
        CommentsCount = t.Comments.Count,
        AttachmentsCount = t.Attachments.Count
    };
}