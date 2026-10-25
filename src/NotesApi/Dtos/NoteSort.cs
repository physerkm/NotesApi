using System.Text.Json.Serialization;

namespace NotesApi.Dtos;

// GET api/notes?sort=... için izin verilen değerler.
// Büyük/küçük harf fark etmez: ?sort=title ve ?sort=Title aynı.
// JsonStringEnumConverter: Swagger'da sayı (0, 1, 2) yerine isimler görünsün.
[JsonConverter(typeof(JsonStringEnumConverter<NoteSort>))]
public enum NoteSort
{
    Newest, // En yeni önce (varsayılan)
    Oldest, // En eski önce
    Title   // Başlığa göre Türkçe alfabetik (A → Z)
}
