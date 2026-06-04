using System.ComponentModel.DataAnnotations;

namespace NexusBackend.DTOs;

public class CreateProjectDTO
{
    [Required(ErrorMessage = "Project name is required.")]
    [MaxLength(100, ErrorMessage = "Project name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    public string? Color { get; set; } = "#6366f1";
}

public class UpdateProjectDTO
{
    [MaxLength(100, ErrorMessage = "Project name cannot exceed 100 characters.")]
    public string? Name { get; set; }

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    public string? Color { get; set; }

    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100.")]
    public int? Progress { get; set; }

    public bool? IsArchived { get; set; }
}

public class ProjectDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public int Progress { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserDTO Owner { get; set; } = null!;
    public List<ProjectMemberDTO> Members { get; set; } = [];
    public int TasksCount { get; set; }
}

public class ProjectMemberDTO
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class AddMemberDTO
{
    [Required(ErrorMessage = "UserId is required.")]
    public Guid UserId { get; set; }

    public string Role { get; set; } = "Member";
}
