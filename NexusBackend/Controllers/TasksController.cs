using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBackend.DTOs;
using NexusBackend.Helpers;
using NexusBackend.Services;

namespace NexusBackend.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetAll(Guid projectId)
    {
        var result = await _taskService.GetAllAsync(projectId, GetUserId());
        return Ok(ApiResponse<List<TaskDTO>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _taskService.GetByIdAsync(id, GetUserId());
        return Ok(ApiResponse<TaskDTO>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDTO dto)
    {
        var result = await _taskService.CreateAsync(dto, GetUserId());
        return Ok(ApiResponse<TaskDTO>.Created(result, "Task created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDTO dto)
    {
        var result = await _taskService.UpdateAsync(id, dto, GetUserId());
        return Ok(ApiResponse<TaskDTO>.Ok(result, "Task updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _taskService.DeleteAsync(id, GetUserId());
        return Ok(ApiResponse<string>.Ok("Task deleted successfully."));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDTO dto)
    {
        var result = await _taskService.ChangeStatusAsync(id, dto, GetUserId());
        return Ok(ApiResponse<TaskDTO>.Ok(result, "Status updated successfully."));
    }
}