using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagementSystem.Entities;

[Table("documents")]
public class Document
{   
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("path")]
    public string Path { get; set; }
    [Column("file_type")]
    public string FileType { get; set; }
}