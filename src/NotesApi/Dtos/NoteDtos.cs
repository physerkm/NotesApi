using System.ComponentModel.DataAnnotations;

namespace NotesApi.Dtos;

// DTO (Data Transfer Object): API'ye gelen ve API'den dönen veri şekli.
// Model sınıfını doğrudan dışarı açmamak için ayrı tutuyoruz.
// Record'larda alan açıklamaları <param> etiketleriyle yazılır.

/// <summary>
/// Yeni not oluşturmak için gönderilen veri.
/// </summary>
/// <param name="Title">Not başlığı. Zorunlu, en fazla 200 karakter.</param>
/// <param name="Content">Not içeriği. İsteğe bağlı, en fazla 10.000 karakter.</param>
/// <param name="Tags">Etiketler. İsteğe bağlı, en fazla 10 tane. Tekrar edenler ve boş olanlar ayıklanır.</param>
public record CreateNoteRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(10_000)] string? Content,
    [MaxLength(10)] List<string>? Tags);

/// <summary>
/// Var olan bir notu güncellemek için gönderilen veri. Tüm alanlar yenileriyle değiştirilir.
/// </summary>
/// <param name="Title">Not başlığı. Zorunlu, en fazla 200 karakter.</param>
/// <param name="Content">Not içeriği. İsteğe bağlı, en fazla 10.000 karakter.</param>
/// <param name="Tags">Etiketler. İsteğe bağlı, en fazla 10 tane. Gönderilmezse notun etiketleri silinir.</param>
public record UpdateNoteRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(10_000)] string? Content,
    [MaxLength(10)] List<string>? Tags);

/// <summary>
/// API'nin döndürdüğü not.
/// </summary>
/// <param name="Id">Notun numarası.</param>
/// <param name="Title">Not başlığı.</param>
/// <param name="Content">Not içeriği. İçerik girilmediyse boş metin.</param>
/// <param name="Tags">Etiketler, Türkçe alfabetik sırada.</param>
/// <param name="CreatedAt">Oluşturulma zamanı (UTC).</param>
/// <param name="UpdatedAt">Son güncellenme zamanı (UTC). Hiç güncellenmediyse <c>null</c>.</param>
public record NoteResponse(
    int Id,
    string Title,
    string Content,
    IReadOnlyCollection<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
