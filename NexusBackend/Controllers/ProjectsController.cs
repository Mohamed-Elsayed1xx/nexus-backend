using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBackend.DTOs;
using NexusBackend.Helpers;
using NexusBackend.Services;

namespace NexusBackend.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _projectService.GetAllAsync(GetUserId());
        return Ok(ApiResponse<List<ProjectDTO>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _projectService.GetByIdAsync(id, GetUserId());
        return Ok(ApiResponse<ProjectDTO>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDTO dto)
    {
        var result = await _projectService.CreateAsync(dto, GetUserId());
        return Ok(ApiResponse<ProjectDTO>.Created(result, "Project created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDTO dto)
    {
        var result = await _projectService.UpdateAsync(id, dto, GetUserId());
        return Ok(ApiResponse<ProjectDTO>.Ok(result, "Project updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _projectService.DeleteAsync(id, GetUserId());
        return Ok(ApiResponse<string>.Ok("Project deleted successfully."));
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDTO dto)
    {
        await _projectService.AddMemberAsync(id, dto, GetUserId());
        return Ok(ApiResponse<string>.Ok("Member added successfully."));
    }

    [HttpDelete("{id}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
    {
        await _projectService.RemoveMemberAsync(id, memberId, GetUserId());
        return Ok(ApiResponse<string>.Ok("Member removed successfully."));
    }
}