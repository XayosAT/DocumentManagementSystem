using System.ComponentModel.DataAnnotations;

namespace DocumentManagementSystem.DTOs;

public class DocumentDTO
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name is too long")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Path is required")]
    public string Path { get; set; }
    
    [Required(ErrorMessage = "FileType is required")]
    public string FileType { get; set; }
}