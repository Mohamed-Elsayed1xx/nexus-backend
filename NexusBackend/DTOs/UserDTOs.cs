using System.ComponentModel.DataAnnotations;

namespace NexusBackend.DTOs;

public class UpdateProfileDTO
{
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public string? FullName { get; set; }

    [Url(ErrorMessage = "Invalid URL format.")]
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

public class ChangePasswordDTO
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}
