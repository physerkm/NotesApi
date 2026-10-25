namespace NotesApi.Models;

// Veritabanında (şimdilik bellekte) tutulan not kaydı.
public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    // HashSet: aynı etiket iki kez eklenmez.
    // OrdinalIgnoreCase: "C#" ve "c#" aynı etiket sayılır.
    public HashSet<string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
