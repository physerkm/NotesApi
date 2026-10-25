namespace NotesApi.Models;

// Veritabanında (şimdilik bellekte) tutulan not kaydı.
public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    // HashSet: aynı etiket iki kez eklenmez.
    // TagComparer: "C#" = "c#", "İstanbul" = "ISTANBUL" = "istanbul", "LINQ" = "linq".
    public HashSet<string> Tags { get; set; } = new(TagComparer.Instance);
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
