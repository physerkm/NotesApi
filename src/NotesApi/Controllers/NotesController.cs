using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Dtos;
using NotesApi.Services;

namespace NotesApi.Controllers;

// [ApiController]: gelen veriyi otomatik doğrular, hatalıysa 400 döner.
[ApiController]
[Route("api/notes")]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;

    // Dependency Injection: servis Program.cs'te kaydedildi, .NET buraya otomatik verir.
    public NotesController(INoteService noteService)
    {
        _noteService = noteService;
    }

    // GET api/notes
    // GET api/notes?tag=csharp  -> sadece "csharp" etiketli notlar
    // GET api/notes?sort=title   -> başlığa göre Türkçe alfabetik
    // EnumDataType: ?sort=99 gibi tanımsız sayıları reddeder (400).
    [HttpGet]
    public ActionResult<IEnumerable<NoteResponse>> GetAll(
        [FromQuery] string? tag,
        [FromQuery, EnumDataType(typeof(NoteSort))] NoteSort sort = NoteSort.Newest) =>
        Ok(_noteService.GetAll(tag, sort));

    // GET api/notes/5
    [HttpGet("{id:int}")]
    public ActionResult<NoteResponse> GetById(int id)
    {
        var note = _noteService.GetById(id);
        return note is null ? NotFound() : Ok(note);
    }

    // POST api/notes
    [HttpPost]
    public ActionResult<NoteResponse> Create(CreateNoteRequest request)
    {
        var note = _noteService.Create(request);
        // 201 Created + yeni notun adresi (Location header)
        return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
    }

    // PUT api/notes/5
    [HttpPut("{id:int}")]
    public ActionResult<NoteResponse> Update(int id, UpdateNoteRequest request)
    {
        var note = _noteService.Update(id, request);
        return note is null ? NotFound() : Ok(note);
    }

    // DELETE api/notes/5
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) =>
        _noteService.Delete(id) ? NoContent() : NotFound();

    // POST api/notes/undo-delete
    // POST kullanıyoruz çünkü bu istek sunucudaki durumu değiştiriyor.
    [HttpPost("undo-delete")]
    public ActionResult<NoteResponse> UndoDelete()
    {
        var note = _noteService.UndoDelete();
        return note is null
            ? Problem(statusCode: StatusCodes.Status404NotFound,
                      title: "Geri alınacak silinmiş not yok.")
            : Ok(note);
    }
}
