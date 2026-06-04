using Microsoft.EntityFrameworkCore;
using NexusBackend.Data;
using NexusBackend.DTOs;
using NexusBackend.Models;

namespace NexusBackend.Services;

public interface ICommentService
{
    Task<List<CommentDTO>> GetAllAsync(Guid taskId);
    Task<CommentDTO> CreateAsync(CreateCommentDTO dto, Guid userId);
    Task<CommentDTO> UpdateAsync(Guid id, UpdateCommentDTO dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;

    public CommentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommentDTO>> GetAllAsync(Guid taskId)
    {
        var comments = await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(MapToDTO).ToList();
    }

    public async Task<CommentDTO> CreateAsync(CreateCommentDTO dto, Guid userId)
    {
        var comment = new Comment
        {
            Text = dto.Text,
            TaskId = dto.TaskId,
            AuthorId = userId
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var created = await _context.Comments
            .Include(c => c.Author)
            .FirstAsync(c => c.Id == comment.Id);

        return MapToDTO(created);
    }

    public async Task<CommentDTO> UpdateAsync(Guid id, UpdateCommentDTO dto, Guid userId)
    {
        var comment = await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id && c.AuthorId == userId);

        if (comment == null)
            throw new KeyNotFoundException("Comment not found.");

        comment.Text = dto.Text;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDTO(comment);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id && c.AuthorId == userId);

        if (comment == null)
            throw new KeyNotFoundException("Comment not found.");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    private static CommentDTO MapToDTO(Comment c) => new()
    {
        Id = c.Id,
        Text = c.Text,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        TaskId = c.TaskId,
        Author = new UserDTO
        {
            Id = c.Author.Id,
            FullName = c.Author.FullName,
            Email = c.Author.Email!,
            AvatarUrl = c.Author.AvatarUrl,
            CreatedAt = c.Author.CreatedAt
        }
    };
}