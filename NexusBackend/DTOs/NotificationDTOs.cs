namespace NexusBackend.DTOs;

public class NotificationDTO
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserDTO? FromUser { get; set; }
    public Guid? TaskId { get; set; }
}