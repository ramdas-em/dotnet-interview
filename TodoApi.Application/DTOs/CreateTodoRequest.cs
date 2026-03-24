using System.ComponentModel.DataAnnotations;

namespace TodoApi.Application.DTOs;

public class CreateTodoRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}
