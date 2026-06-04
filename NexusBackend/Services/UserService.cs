using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusBackend.Data;
using NexusBackend.DTOs;
using NexusBackend.Models;

namespace NexusBackend.Services;

public interface IUserService
{
    Task<List<UserDTO>> GetAllAsync();
    Task<UserDTO> GetByIdAsync(Guid id);
    Task<UserDTO> UpdateProfileAsync(Guid id, UpdateProfileDTO dto);
    Task ChangePasswordAsync(Guid id, ChangePasswordDTO dto);
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public UserService(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ✅ FIX: حلينا مشكلة N+1 — بدل ما نعمل query لكل user عشان نجيب الـ role
    // بنعمل query واحدة بـ JOIN بتجيب كل الـ users والـ roles في نفس الوقت
    public async Task<List<UserDTO>> GetAllAsync()
    {
        var usersWithRoles = await _context.Users
            .GroupJoin(
                _context.UserRoles,
                user => user.Id,
                userRole => userRole.UserId,
                (user, userRoles) => new { user, userRoles }
            )
            .SelectMany(
                x => x.userRoles.DefaultIfEmpty(),
                (x, userRole) => new { x.user, userRole }
            )
            .GroupJoin(
                _context.Roles,
                x => x.userRole != null ? x.userRole.RoleId : (Guid?)null,
                role => role.Id,
                (x, roles) => new { x.user, roles }
            )
            .SelectMany(
                x => x.roles.DefaultIfEmpty(),
                (x, role) => new
                {
                    x.user.Id,
                    x.user.FullName,
                    Email = x.user.Email ?? "",
                    x.user.AvatarUrl,
                    x.user.CreatedAt,
                    RoleName = role != null ? role.Name : null
                }
            )
            .ToListAsync();

        // في حالة المستخدم عنده أكتر من role، بناخد الأول بس
        var grouped = usersWithRoles
            .GroupBy(x => x.Id)
            .Select(g => new UserDTO
            {
                Id = g.Key,
                FullName = g.First().FullName,
                Email = g.First().Email,
                AvatarUrl = g.First().AvatarUrl,
                Role = g.First(x => x.RoleName != null)?.RoleName ?? "User",
                CreatedAt = g.First().CreatedAt
            })
            .ToList();

        return grouped;
    }

    public async Task<UserDTO> GetByIdAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) throw new KeyNotFoundException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            AvatarUrl = user.AvatarUrl,
            Role = roles.FirstOrDefault() ?? "User",
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDTO> UpdateProfileAsync(Guid id, UpdateProfileDTO dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

        await _context.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            AvatarUrl = user.AvatarUrl,
            Role = roles.FirstOrDefault() ?? "User",
            CreatedAt = user.CreatedAt
        };
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordDTO dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new KeyNotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new ArgumentException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
