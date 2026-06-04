using System.ComponentModel.DataAnnotations;

namespace NexusBackend.DTOs;

public class CreateTaskDTO
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    public string Status { get; set; } = "todo";
    public string Priority { get; set; } = "medium";
    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "ProjectId is required.")]
    public Guid ProjectId { get; set; }

    public Guid? AssigneeId { get; set; }
}

public class UpdateTaskDTO
{
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string? Title { get; set; }

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    public string? Status { get; set; }
    public string? Priority { get; set; }

    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100.")]
    public int? Progress { get; set; }

    public DateTime? DueDate { get; set; }
    public Guid? AssigneeId { get; set; }
}

public class TaskDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Progress { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public UserDTO? Assignee { get; set; }
    public UserDTO CreatedBy { get; set; } = null!;
    public List<ProjectMemberDTO> Collaborators { get; set; } = [];
    public int CommentsCount { get; set; }
    public int AttachmentsCount { get; set; }
}

public class ChangeStatusDTO
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;
}
