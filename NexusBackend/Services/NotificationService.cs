using Microsoft.EntityFrameworkCore;
using NexusBackend.Data;
using NexusBackend.DTOs;

namespace NexusBackend.Services;

public interface INotificationService
{
    Task<List<NotificationDTO>> GetAllAsync(Guid userId);
    Task MarkAsReadAsync(Guid id, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationDTO>> GetAllAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Include(n => n.FromUser)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(n => new NotificationDTO
        {
            Id = n.Id,
            Type = n.Type,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            TaskId = n.TaskId,
            FromUser = n.FromUser == null ? null : new UserDTO
            {
                Id = n.FromUser.Id,
                FullName = n.FromUser.FullName,
                Email = n.FromUser.Email!,
                AvatarUrl = n.FromUser.AvatarUrl,
                CreatedAt = n.FromUser.CreatedAt
            }
        }).ToList();
    }

    public async Task MarkAsReadAsync(Guid id, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification == null)
            throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        notifications.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification == null)
            throw new KeyNotFoundException("Notification not found.");

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
    }
}