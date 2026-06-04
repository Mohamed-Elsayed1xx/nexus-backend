using Microsoft.EntityFrameworkCore;
using NexusBackend.Data;
using NexusBackend.DTOs;
using NexusBackend.Models;

namespace NexusBackend.Services;

public interface IProjectService
{
    Task<List<ProjectDTO>> GetAllAsync(Guid userId);
    Task<ProjectDTO> GetByIdAsync(Guid id, Guid userId);
    Task<ProjectDTO> CreateAsync(CreateProjectDTO dto, Guid userId);
    Task<ProjectDTO> UpdateAsync(Guid id, UpdateProjectDTO dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task AddMemberAsync(Guid projectId, AddMemberDTO dto, Guid userId);
    Task RemoveMemberAsync(Guid projectId, Guid memberId, Guid userId);
}

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDTO>> GetAllAsync(Guid userId)
    {
        var projects = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
            .ToListAsync();

        return projects.Select(MapToDTO).ToList();
    }

    public async Task<ProjectDTO> GetByIdAsync(Guid id, Guid userId)
    {
        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

        if (project == null)
            throw new KeyNotFoundException("Project not found.");

        return MapToDTO(project);
    }

    public async Task<ProjectDTO> CreateAsync(CreateProjectDTO dto, Guid userId)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color ?? "#6366f1",
            OwnerId = userId
        };

        _context.Projects.Add(project);

        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = "Admin"
        });

        await _context.SaveChangesAsync();

        return await GetByIdAsync(project.Id, userId);
    }

    public async Task<ProjectDTO> UpdateAsync(Guid id, UpdateProjectDTO dto, Guid userId)
    {
        // BUG FIX: كان بيسمح بس للـ owner يعدل، المفروض Admin members برضو يقدروا
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId && m.Role == "Admin")));

        if (project == null)
            throw new KeyNotFoundException("Project not found or insufficient permissions.");

        if (dto.Name != null) project.Name = dto.Name;
        if (dto.Description != null) project.Description = dto.Description;
        if (dto.Color != null) project.Color = dto.Color;
        if (dto.Progress.HasValue) project.Progress = dto.Progress.Value;
        if (dto.IsArchived.HasValue) project.IsArchived = dto.IsArchived.Value;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, userId);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        // Only owner can delete
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (project == null)
            throw new KeyNotFoundException("Project not found or you are not the owner.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    public async Task AddMemberAsync(Guid projectId, AddMemberDTO dto, Guid userId)
    {
        // BUG FIX: فقط Admin members أو الـ owner يقدروا يضيفوا members
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId && m.Role == "Admin")));

        if (project == null)
            throw new KeyNotFoundException("Project not found or insufficient permissions.");

        // BUG FIX: prevent adding owner as member again
        if (project.OwnerId == dto.UserId)
            throw new ArgumentException("User is the project owner and already has full access.");

        var exists = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == dto.UserId);

        if (exists)
            throw new ArgumentException("User is already a member.");

        // BUG FIX: Validate that the user being added actually exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists)
            throw new KeyNotFoundException("User not found.");

        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = dto.UserId,
            Role = dto.Role
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid memberId, Guid userId)
    {
        // BUG FIX: Admin أو Owner يقدر يشيل members
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId &&
                (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId && m.Role == "Admin")));

        if (project == null)
            throw new KeyNotFoundException("Project not found or insufficient permissions.");

        // BUG FIX: Cannot remove the project owner
        if (project.OwnerId == memberId)
            throw new ArgumentException("Cannot remove the project owner.");

        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberId);

        if (member == null)
            throw new KeyNotFoundException("Member not found.");

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();
    }

    private static ProjectDTO MapToDTO(Project p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Color = p.Color,
        Progress = p.Progress,
        IsArchived = p.IsArchived,
        CreatedAt = p.CreatedAt,
        Owner = new UserDTO
        {
            Id = p.Owner.Id,
            FullName = p.Owner.FullName,
            Email = p.Owner.Email!,
            AvatarUrl = p.Owner.AvatarUrl,
            CreatedAt = p.Owner.CreatedAt
        },
        Members = p.Members.Select(m => new ProjectMemberDTO
        {
            UserId = m.UserId,
            FullName = m.User.FullName,
            Email = m.User.Email!,
            AvatarUrl = m.User.AvatarUrl,
            Role = m.Role,
            JoinedAt = m.JoinedAt
        }).ToList(),
        TasksCount = p.Tasks.Count
    };
}