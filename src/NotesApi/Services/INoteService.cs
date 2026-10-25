using NotesApi.Dtos;

namespace NotesApi.Services;

// Controller bu arayüzü kullanır, arkadaki gerçek sınıfı bilmez.
// İleride bellekteki listeyi veritabanıyla değiştirdiğimizde controller hiç değişmeyecek.
public interface INoteService
{
    // tag verilirse sadece o etiketi taşıyan notlar döner.
    IEnumerable<NoteResponse> GetAll(string? tag = null);
    NoteResponse? GetById(int id);
    NoteResponse Create(CreateNoteRequest request);
    NoteResponse? Update(int id, UpdateNoteRequest request);
    bool Delete(int id);

    // En son silinen notu geri getirir. Geri alınacak not yoksa null döner.
    NoteResponse? UndoDelete();
}
