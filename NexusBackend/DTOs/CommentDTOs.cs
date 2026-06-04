using System.ComponentModel.DataAnnotations;

namespace NexusBackend.DTOs;

public class CreateCommentDTO
{
    [Required(ErrorMessage = "Comment text is required.")]
    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "TaskId is required.")]
    public Guid TaskId { get; set; }
}

public class UpdateCommentDTO
{
    [Required(ErrorMessage = "Comment text is required.")]
    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
    public string Text { get; set; } = string.Empty;
}

public class CommentDTO
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid TaskId { get; set; }
    public UserDTO Author { get; set; } = null!;
}
