using NotesApi.Dtos;
using NotesApi.Models;

namespace NotesApi.Mappings;

// Model -> DTO dönüşümleri (mapping).
// Extension method kuralları: sınıf static, metot static, ilk parametrenin önünde "this".
public static class NoteMappingExtensions
{
    // "this Note note": Note tipine yeni bir metot ekliyoruz, Note sınıfını değiştirmeden.
    // Kullanımı:              note.ToResponse()
    // Derleyicinin çevirisi:  NoteMappingExtensions.ToResponse(note)
    public static NoteResponse ToResponse(this Note note) =>
        new(note.Id,
            note.Title,
            note.Content,
            // HashSet sıra garantisi vermez, bu yüzden yanıtta Türkçe alfabetik sıralıyoruz.
            // ToArray() bir kopya üretir: dışarı verdiğimiz şey içerideki seti değiştiremez.
            note.Tags.Order(TextComparers.Turkish).ToArray(),
            note.CreatedAt,
            note.UpdatedAt);
}
