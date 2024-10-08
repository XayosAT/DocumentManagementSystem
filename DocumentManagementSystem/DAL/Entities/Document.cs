namespace DAL.Entities;

public class Document
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    
    public string FileType { get; set; }
}