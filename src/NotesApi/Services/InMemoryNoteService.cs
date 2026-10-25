using System.Collections.Concurrent;
using NotesApi.Dtos;
using NotesApi.Mappings; // note.ToResponse() extension method'u buradan geliyor.
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

    public IEnumerable<NoteResponse> GetAll(string? tag = null, NoteSort sort = NoteSort.Newest)
    {
        IEnumerable<Note> notes = _notes.Values;

        // HashSet.Contains O(1): not başına etiket sayısı ne olursa olsun hızlı.
        // Set TagComparer ile kurulduğu için ?tag=ISTANBUL da "İstanbul" etiketini bulur.
        if (!string.IsNullOrWhiteSpace(tag))
        {
            // Trim'i bir kez yapıyoruz. Lambda bu değişkeni yakalar (closure),
            // böylece her not için Trim tekrar çalışmaz.
            var trimmedTag = tag.Trim();
            notes = notes.Where(n => n.Tags.Contains(trimmedTag));
        }

        // switch ifadesi: her sıralama seçeneği için bir sorgu.
        // ThenBy(Id): başlıklar aynıysa sıra her seferinde aynı olsun diye ikinci ölçüt.
        IOrderedEnumerable<Note> sorted = sort switch
        {
            NoteSort.Oldest => notes.OrderBy(n => n.CreatedAt).ThenBy(n => n.Id),
            NoteSort.Title  => notes.OrderBy(n => n.Title, TextComparers.Turkish).ThenBy(n => n.Id),
            _               => notes.OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id)
        };

        return sorted.Select(n => n.ToResponse()).ToList();
    }

    public NoteResponse? GetById(int id) =>
        _notes.TryGetValue(id, out var note) ? note.ToResponse() : null;

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
        return note.ToResponse();
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
        return note.ToResponse();
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
        return note.ToResponse();
    }

    // Boş etiketleri atar, baş/son boşlukları siler, tekrarları HashSet ayıklar.
    private static HashSet<string> NormalizeTags(IEnumerable<string>? tags)
    {
        var set = new HashSet<string>(TagComparer.Instance);
        if (tags is null)
            return set;

        foreach (var tag in tags)
        {
            if (!string.IsNullOrWhiteSpace(tag))
                set.Add(tag.Trim()); // Aynısı zaten varsa Add sessizce yok sayar.
        }
        return set;
    }
}
