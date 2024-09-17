using System.ComponentModel.DataAnnotations;

namespace DocumentManagementSystem.Entities;

public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Firstname is required")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Lastname is required")]
    public string LastName { get; set; }
    
    [Required, EmailAddress]
    public string Email { get; set; }
}