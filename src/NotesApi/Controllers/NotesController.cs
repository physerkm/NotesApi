using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Dtos;
using NotesApi.Services;

namespace NotesApi.Controllers;

/// <summary>
/// Not ekleme, listeleme, güncelleme ve silme işlemleri.
/// </summary>
// [ApiController]: gelen veriyi otomatik doğrular, hatalıysa 400 döner.
// [ProducesResponseType]: Swagger'a her endpoint'in hangi durum kodlarını dönebileceğini söyler.
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

    /// <summary>
    /// Notları sayfa sayfa listeler.
    /// </summary>
    /// <remarks>
    /// Etikete göre filtrelenebilir, sıralanabilir ve sayfalanabilir; hepsi birlikte de kullanılabilir.
    /// Örnekler: <c>?tag=backend</c>, <c>?sort=title</c>, <c>?page=2</c>, <c>?pageSize=50</c>
    /// </remarks>
    /// <param name="tag">Sadece bu etiketi taşıyan notlar döner. Büyük/küçük harf fark etmez, "İstanbul" ile "istanbul" aynıdır.</param>
    /// <param name="sort">Sıralama: <c>Newest</c> (varsayılan, en yeni önce), <c>Oldest</c> veya <c>Title</c> (başlığa göre Türkçe alfabetik).</param>
    /// <param name="page">Sayfa numarası. 1'den başlar, varsayılan 1.</param>
    /// <param name="pageSize">Sayfa başına not sayısı. 1 ile 100 arası, varsayılan 20.</param>
    /// <response code="200">İstenen sayfa ve toplam kayıt bilgisi. Hiç not yoksa boş sayfa döner.</response>
    /// <response code="400">Geçersiz sıralama, sayfa numarası ya da sayfa boyutu.</response>
    // EnumDataType: ?sort=99 gibi tanımsız sayıları reddeder (400).
    // Range: ?page=0 ya da ?pageSize=1000 gibi değerleri reddeder (400).
    [HttpGet]
    [ProducesResponseType<PagedResponse<NoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<PagedResponse<NoteResponse>> GetAll(
        [FromQuery] string? tag,
        [FromQuery, EnumDataType(typeof(NoteSort))] NoteSort sort = NoteSort.Newest,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20) =>
        Ok(_noteService.GetAll(tag, sort, page, pageSize));

    /// <summary>
    /// Tek bir notu getirir.
    /// </summary>
    /// <param name="id">Notun numarası.</param>
    /// <response code="200">İstenen not.</response>
    /// <response code="404">Bu numarada bir not yok.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<NoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<NoteResponse> GetById(int id)
    {
        var note = _noteService.GetById(id);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>
    /// Yeni not oluşturur.
    /// </summary>
    /// <remarks>
    /// Etiketlerdeki tekrarlar ve boş değerler ayıklanır:
    /// <c>["C#", "c#", "backend"]</c> gönderilirse <c>["backend", "C#"]</c> kaydedilir.
    /// </remarks>
    /// <param name="request">Notun başlığı, içeriği ve etiketleri.</param>
    /// <response code="201">Not oluşturuldu. Yeni notun adresi <c>Location</c> başlığında döner.</response>
    /// <response code="400">Gönderilen veri geçersiz (ör. başlık boş ya da 10'dan fazla etiket var).</response>
    [HttpPost]
    [ProducesResponseType<NoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<NoteResponse> Create(CreateNoteRequest request)
    {
        var note = _noteService.Create(request);
        // 201 Created + yeni notun adresi (Location header)
        return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
    }

    /// <summary>
    /// Var olan bir notu günceller.
    /// </summary>
    /// <remarks>
    /// Başlık, içerik ve etiketlerin hepsi gönderilen değerlerle değiştirilir.
    /// </remarks>
    /// <param name="id">Güncellenecek notun numarası.</param>
    /// <param name="request">Notun yeni başlığı, içeriği ve etiketleri.</param>
    /// <response code="200">Güncellenmiş not.</response>
    /// <response code="400">Gönderilen veri geçersiz.</response>
    /// <response code="404">Bu numarada bir not yok.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<NoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<NoteResponse> Update(int id, UpdateNoteRequest request)
    {
        var note = _noteService.Update(id, request);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>
    /// Bir notu siler.
    /// </summary>
    /// <remarks>
    /// Silinen not geri alınabilir: bkz. <c>POST /api/notes/undo-delete</c>.
    /// </remarks>
    /// <param name="id">Silinecek notun numarası.</param>
    /// <response code="204">Not silindi.</response>
    /// <response code="404">Bu numarada bir not yok.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id) =>
        _noteService.Delete(id) ? NoContent() : NotFound();

    /// <summary>
    /// En son silinen notu geri getirir.
    /// </summary>
    /// <remarks>
    /// Silinen notlar bir yığında (stack) tutulur: son silinen ilk geri gelir.
    /// Not aynı numarasıyla geri konur.
    /// </remarks>
    /// <response code="200">Geri getirilen not.</response>
    /// <response code="404">Geri alınacak silinmiş not yok.</response>
    // POST kullanıyoruz çünkü bu istek sunucudaki durumu değiştiriyor.
    [HttpPost("undo-delete")]
    [ProducesResponseType<NoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<NoteResponse> UndoDelete()
    {
        var note = _noteService.UndoDelete();
        return note is null
            ? Problem(statusCode: StatusCodes.Status404NotFound,
                      title: "Geri alınacak silinmiş not yok.")
            : Ok(note);
    }
}
