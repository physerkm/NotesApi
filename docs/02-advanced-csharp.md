# 02 - İleri C# (Advanced C#)

Bölüm birbirinden bağımsız birçok konu içeriyor. Backend için önemine göre ayırdım
ve her konuyu bu projede nerede kullandığımı yazdım.

## Her gün kullandıklarım

| Konu | Kısaca | Projede nerede? |
|---|---|---|
| Delegate, `Func` / `Action` | Bir metodu değişken gibi taşımak. `Func<Note, bool>`: Note alır, bool döner. | LINQ metotlarına verilen fonksiyonlar |
| Lambda (`=>`) | İsimsiz, kısa metot | `Where(n => ...)`, `OrderBy(n => n.Title, ...)`, Program.cs'teki `options => { ... }` |
| Closure | Lambda dışarıdaki değişkeni yakalar, değeri **çalıştığı anda** okur | `GetAll` içinde `Where` lambdası `trimmedTag` değişkenini yakalıyor. İlk hali `tag.Trim()`'i lambdanın içinde çağırıyordu, yani her not için tekrar çalışıyordu. Trim'i bir kez yapıp sonucu yakalamak daha doğru. |
| `try` / `catch` / `finally` | Hata yakalama ve temizlik | `GlobalExceptionHandler` |
| TryXXX deseni | Hata fırlatmak yerine `false` dönmek | `TryGetValue`, `TryRemove`, `TryPop` |
| Nullable (`?`, `??`, `?.`) | Değerin boş olabileceğini tipte göstermek | `DateTime? UpdatedAt`, `string? tag`, `Content ?? string.Empty` |
| Extension method | Var olan bir tipe, onu değiştirmeden metot eklemek | `Where`, `OrderBy`, `AddControllers()`, `AddSingleton()` |
| Record | Değiştirilemeyen, içeriğe göre karşılaştırılan veri tipi | Bütün DTO'lar |
| Attribute (`[...]`) | Koda ek bilgi eklemek, framework bu bilgiyi okur | `[ApiController]`, `[HttpGet]`, `[Required]`, `[FromQuery]`, `[JsonConverter]` |
| Pattern matching | `is`, `switch` ifadesi, `_` (geri kalan her şey) | `note is null ? ...`, `sort switch { ... }` |
| Anonymous type | İsimsiz, geçici nesne | `CreatedAtAction(..., new { id = note.Id }, ...)` |

## Exception kuralları (kitaptan)

- Önce **özel** hata türlerini yakala, genel `Exception` en sonda olsun.
- Hatayı tekrar fırlatırken `throw ex;` değil `throw;` yaz. Yoksa hatanın çıktığı yer (stack trace) kaybolur.
- **Önlenebilir hatalar için exception kullanma, önceden kontrol et.** Exception yakalamak yavaş bir işlemdir.
  Projede notu bulamamak normal bir durum, bu yüzden exception değil `TryGetValue` + `404` kullanıyorum.
- **Güven sınırını geçerken teknik ayrıntı sızdırma.** Kullanıcıya genel bir mesaj ver, ayrıntıyı log'a yaz.

## Bu projede: global hata yönetimi

Beklenmeyen bir hata (ör. veritabanı bağlantısının kopması) olduğunda:

1. `app.UseExceptionHandler()` pipeline'ın en başında. Kendinden sonraki her adımda oluşan hatayı yakalıyor.
2. `GlobalExceptionHandler` hatayı **log'a yazıyor**: istek, mesaj, stack trace ve `traceId`.
3. İstemciye standart bir **ProblemDetails** yanıtı dönüyor: `500`, genel bir başlık ve `traceId`.
4. Hata mesajı (`detail`) **sadece geliştirme ortamında** gösteriliyor. Canlı ortamda sınıf adları, dosya yolları gibi iç bilgiler dışarı sızmıyor.
5. Yanıttaki `traceId` ile log'daki aynı. Kullanıcı bu kimliği bildirirse hatayı log'da bulabiliyorum.

Test ederken API'nin hatadan sonra çalışmaya devam ettiğini ve normal `404` yanıtlarının bozulmadığını da kontrol ettim.

## Bu projede: Swagger açıklamaları (XML dokümantasyonu + attribute'lar)

- `.csproj` içinde `<GenerateDocumentationFile>true</GenerateDocumentationFile>` açık. Derleyici `///` açıklamalarını
  bir XML dosyasına çıkarıyor, .NET 10'un OpenAPI paketi de bu dosyadan Swagger açıklamalarını üretiyor.
- Kullandığım etiketler: `<summary>` (kısa açıklama), `<remarks>` (ayrıntı), `<param>` (parametre),
  `<response code="...">` (durum kodunun anlamı), `<c>` (satır içi kod).
- Record'larda alan açıklamaları türün üstündeki `<param name="Title">` etiketleriyle yazılıyor.
- `[ProducesResponseType]` attribute'u olmadan Swagger her endpoint'i sadece `200` dönüyormuş gibi gösteriyordu
  (oysa `POST` `201`, `DELETE` `204` döner). Bu attribute ile her endpoint'in olası yanıtlarını belirttim.
- Açıklaması olmayan her public üye için derleyici CS1591 uyarısı veriyor. Her üyeye açıklama zorunlu olmadığı için
  `<NoWarn>` ile bu uyarıyı kapattım. Kitaptaki "gürültülü uyarıları bastır ki önemli olanlar görünsün" fikri.
- Dikkat: XML içinde `&` yazmak için `&amp;` kullanmak gerekiyor ve Swagger bunu çözmeden `&amp;` olarak gösterdi.
  Açıklamalarda `&` kullanmamak en temizi.

## Şimdilik atladıklarım

`dynamic`, operator overloading, static polymorphism / generic math, `unsafe` kod ve pointer'lar,
`stackalloc`, preprocessor direktifleri (`#if`). Bunlar Web API geliştirirken nadiren gerekiyor.
