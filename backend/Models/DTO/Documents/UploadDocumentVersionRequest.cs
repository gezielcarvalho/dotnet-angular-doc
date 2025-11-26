using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Models.DTO.Documents;

public class UploadDocumentVersionRequest
{
    [Required]
    public IFormFile? File { get; set; }

    [StringLength(500)]
    public string? ChangeComment { get; set; }
}
