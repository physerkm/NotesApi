using NotesApi.Dtos;

namespace NotesApi.Services;

// Controller bu arayüzü kullanır, arkadaki gerçek sınıfı bilmez.
// İleride bellekteki listeyi veritabanıyla değiştirdiğimizde controller hiç değişmeyecek.
public interface INoteService
{
    // tag verilirse sadece o etiketi taşıyan notlar döner.
    // sort: sonuçların sırası (varsayılan: en yeni önce).
    // page / pageSize: hangi sayfanın, sayfa başına kaç kayıtla döneceği.
    PagedResponse<NoteResponse> GetAll(
        string? tag = null, NoteSort sort = NoteSort.Newest, int page = 1, int pageSize = 20);
    NoteResponse? GetById(int id);
    NoteResponse Create(CreateNoteRequest request);
    NoteResponse? Update(int id, UpdateNoteRequest request);
    bool Delete(int id);

    // En son silinen notu geri getirir. Geri alınacak not yoksa null döner.
    NoteResponse? UndoDelete();
}
