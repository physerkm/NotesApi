using System.Collections.Concurrent;
using NotesApi.Dtos;
using NotesApi.Models;

namespace NotesApi.Services;

// Notları bellekte tutar. Uygulama kapanınca notlar silinir.
// Bir sonraki adımda bunun yerine Entity Framework Core + veritabanı kullanacağız.
public class InMemoryNoteService : INoteService
{
    // Aynı anda birden fazla istek gelebilir, bu yüzden thread-safe bir sözlük kullanıyoruz.
    private readonly ConcurrentDictionary<int, Note> _notes = new();
    private int _lastId;

    // Silinen notlar yığını (LIFO): en son silinen en üstte durur.
    // Stack<T> thread-safe değil; aynı mantığın thread-safe hali ConcurrentStack<T>.
    private readonly ConcurrentStack<Note> _deletedNotes = new();

    public IEnumerable<NoteResponse> GetAll(string? tag = null)
    {
        IEnumerable<Note> notes = _notes.Values;

        // HashSet.Contains O(1): not başına etiket sayısı ne olursa olsun hızlı.
        // Set OrdinalIgnoreCase ile kurulduğu için ?tag=CSHARP da "csharp" etiketini bulur.
        if (!string.IsNullOrWhiteSpace(tag))
            notes = notes.Where(n => n.Tags.Contains(tag.Trim()));

        return notes
            .OrderByDescending(n => n.CreatedAt)
            .Select(ToResponse)
            .ToList();
    }

    public NoteResponse? GetById(int id) =>
        _notes.TryGetValue(id, out var note) ? ToResponse(note) : null;

    public NoteResponse Create(CreateNoteRequest request)
    {
        var note = new Note
        {
            Id = Interlocked.Increment(ref _lastId),
            Title = request.Title.Trim(),
            Content = request.Content ?? string.Empty,
            Tags = NormalizeTags(request.Tags),
            CreatedAt = DateTime.UtcNow
        };

        _notes[note.Id] = note;
        return ToResponse(note);
    }

    public NoteResponse? Update(int id, UpdateNoteRequest request)
    {
        if (!_notes.TryGetValue(id, out var note))
            return null;

        note.Title = request.Title.Trim();
        note.Content = request.Content ?? string.Empty;
        // Mevcut seti değiştirmek yerine yenisini atıyoruz.
        // Böylece o sırada notu okuyan başka bir istek yarım kalmış bir set görmez.
        note.Tags = NormalizeTags(request.Tags);
        note.UpdatedAt = DateTime.UtcNow;
        return ToResponse(note);
    }

    public bool Delete(int id)
    {
        if (!_notes.TryRemove(id, out var note))
            return false;

        _deletedNotes.Push(note); // Yığının en üstüne koy.
        return true;
    }

    public NoteResponse? UndoDelete()
    {
        // TryPop: en üstteki notu alır ve yığından çıkarır. Yığın boşsa false döner.
        // (Stack<T>.Pop boş yığında hata fırlatır; TryPop fırlatmaz.)
        if (!_deletedNotes.TryPop(out var note))
            return null;

        // Id'ler hiç tekrar kullanılmadığı için (Interlocked.Increment) aynı Id ile geri koymak güvenli.
        _notes[note.Id] = note;
        return ToResponse(note);
    }

    // Boş etiketleri atar, baş/son boşlukları siler, tekrarları HashSet ayıklar.
    private static HashSet<string> NormalizeTags(IEnumerable<string>? tags)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (tags is null)
            return set;

        foreach (var tag in tags)
        {
            if (!string.IsNullOrWhiteSpace(tag))
                set.Add(tag.Trim()); // Aynısı zaten varsa Add sessizce yok sayar.
        }
        return set;
    }

    // HashSet sıra garantisi vermez, bu yüzden yanıtta alfabetik sıralıyoruz.
    // ToArray() bir kopya üretir: dışarı verdiğimiz şey içerideki seti değiştiremez.
    private static NoteResponse ToResponse(Note n) =>
        new(n.Id,
            n.Title,
            n.Content,
            n.Tags.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            n.CreatedAt,
            n.UpdatedAt);
}
