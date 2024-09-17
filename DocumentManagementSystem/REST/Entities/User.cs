using System.ComponentModel.DataAnnotations;

namespace DocumentManagementSystem.Entities;

public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }
    
    [Required, EmailAddress]
    public string Email { get; set; }
}