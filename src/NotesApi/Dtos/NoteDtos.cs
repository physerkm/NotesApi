using System.ComponentModel.DataAnnotations;

namespace NotesApi.Dtos;

// DTO (Data Transfer Object): API'ye gelen ve API'den dönen veri şekli.
// Model sınıfını doğrudan dışarı açmamak için ayrı tutuyoruz.

// Tags: istemci düz bir liste gönderir, tekrarları serviste ayıklarız.
// MaxLength(10): bir notta en fazla 10 etiket olabilir.
public record CreateNoteRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(10_000)] string? Content,
    [MaxLength(10)] List<string>? Tags);

public record UpdateNoteRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(10_000)] string? Content,
    [MaxLength(10)] List<string>? Tags);

// IReadOnlyCollection: API'yi kullanan taraf etiketleri okuyabilir ama değiştiremez.
public record NoteResponse(
    int Id,
    string Title,
    string Content,
    IReadOnlyCollection<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
