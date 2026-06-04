using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBackend.DTOs;
using NexusBackend.Helpers;
using NexusBackend.Services;

namespace NexusBackend.Controllers;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetAll(Guid taskId)
    {
        var result = await _commentService.GetAllAsync(taskId);
        return Ok(ApiResponse<List<CommentDTO>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentDTO dto)
    {
        var result = await _commentService.CreateAsync(dto, GetUserId());
        return Ok(ApiResponse<CommentDTO>.Created(result, "Comment added successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentDTO dto)
    {
        var result = await _commentService.UpdateAsync(id, dto, GetUserId());
        return Ok(ApiResponse<CommentDTO>.Ok(result, "Comment updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _commentService.DeleteAsync(id, GetUserId());
        return Ok(ApiResponse<string>.Ok("Comment deleted successfully."));
    }
}