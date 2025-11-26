namespace Backend.Models.DTO.Tags;

public class TagDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#000000";
    public int DocumentCount { get; set; }
}
