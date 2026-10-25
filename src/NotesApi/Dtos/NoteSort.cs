using System.Text.Json.Serialization;

namespace NotesApi.Dtos;

// JsonStringEnumConverter: Swagger'da sayı (0, 1, 2) yerine isimler görünsün.

/// <summary>
/// Not listesinin sıralaması. Büyük/küçük harf fark etmez: <c>title</c> ve <c>Title</c> aynı.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<NoteSort>))]
public enum NoteSort
{
    /// <summary>En yeni not önce (varsayılan).</summary>
    Newest,

    /// <summary>En eski not önce.</summary>
    Oldest,

    /// <summary>Başlığa göre Türkçe alfabetik (A → Z).</summary>
    Title
}
