# NotesApi

ASP.NET Core (.NET 10) ile yazılmış bir not uygulaması REST API'si.
Backend geliştirmeyi adım adım öğrenirken büyüttüğüm bir proje: her yeni konu projeye bir özellik olarak ekleniyor.

## Özellikler

- [x] Not ekleme, listeleme, güncelleme, silme (CRUD)
- [x] Gelen verinin doğrulanması (validation)
- [x] Etiketler: tekrarsız, büyük/küçük harf duyarsız (`HashSet`) ve etikete göre filtreleme
- [x] Türkçe I desteği: `İstanbul`, `ISTANBUL` ve `istanbul` aynı etiket, `LINQ` ve `linq` de aynı (özel `TagComparer`)
- [x] Silmeyi geri alma: en son silinen not önce geri gelir (`ConcurrentStack`, LIFO)
- [x] Sıralama: en yeni, en eski veya başlığa göre Türkçe alfabetik (`tr-TR` comparer)
- [x] Sayfalama: `?page=` ve `?pageSize=` ile, toplam kayıt ve sayfa sayısıyla birlikte (LINQ `Skip` / `Take`)
- [x] Swagger arayüzü ile tarayıcıdan test: her endpoint, parametre ve alan için açıklama ve olası durum kodları
- [ ] Veritabanı (Entity Framework Core)
- [x] Global hata yönetimi: beklenmeyen hatalarda düzenli `500` yanıtı, log ve `traceId`
- [ ] Kullanıcı kaydı ve JWT ile giriş
- [ ] Birim testleri (xUnit)
- [ ] Docker ve canlıya alma

## Teknolojiler

- C# / .NET 10
- ASP.NET Core Web API (Controller tabanlı)
- OpenAPI + Swagger UI

## Proje yapısı

```
src/NotesApi
├── Controllers/   # HTTP isteklerini karşılayan katman
├── Services/      # İş mantığı (INoteService + uygulaması)
├── Dtos/          # API'ye giren / çıkan veri şekilleri
├── Models/        # Uygulama içi veri modeli ve metin karşılaştırıcıları
├── Mappings/      # Model -> DTO dönüşümleri (extension method)
├── ErrorHandling/ # Global hata yönetimi (yakalanmamış hatalar -> 500 + log)
└── Program.cs     # Servis kayıtları ve middleware ayarları
```

## Endpoint'ler

| Metot  | Adres             | Açıklama           | Başarılı yanıt |
|--------|-------------------|--------------------|----------------|
| GET    | `/api/notes`      | Notları sayfa sayfa getir (varsayılan: 1. sayfa, 20 not) | 200 OK |
| GET    | `/api/notes?tag=backend` | Etikete göre filtrele | 200 OK  |
| GET    | `/api/notes?sort=title`  | Sırala: `newest` (varsayılan), `oldest`, `title` | 200 OK |
| GET    | `/api/notes?page=2&pageSize=10` | Sayfala: `page` 1'den başlar, `pageSize` 1–100 | 200 OK |
| GET    | `/api/notes/{id}` | Tek not getir      | 200 OK         |
| POST   | `/api/notes`      | Yeni not ekle      | 201 Created    |
| PUT    | `/api/notes/{id}` | Notu güncelle      | 200 OK         |
| DELETE | `/api/notes/{id}` | Notu sil           | 204 No Content |
| POST   | `/api/notes/undo-delete` | En son silinen notu geri getir | 200 OK |

Not bulunamazsa `404 Not Found`, gönderilen veri hatalıysa `400 Bad Request` döner.
Beklenmeyen bir hata olursa API çökmez, `500` ve bir `traceId` döner. Hatanın ayrıntısı sunucu log'una yazılır,
canlı ortamda istemciye teknik bilgi gösterilmez.

Örnek istek:

```json
POST /api/notes
{
  "title": "İlk notum",
  "content": "REST API'de POST yeni kayıt oluşturur.",
  "tags": ["C#", "c#", "backend"]
}
```

Yanıtta `"tags": ["backend", "C#"]` döner: tekrar eden `c#` ayıklanır.

`?sort=title` Türkçe alfabeyi kullanır: `Cam, Çiçek, Dağ, Ilık, İnce, Ödev, Şeker, Zeytin`.
(Standart karşılaştırma `Çiçek`, `Ödev` ve `Şeker`i `Zeytin`den sonraya koyardı.)

Liste yanıtı sayfalıdır. Filtre, sıralama ve sayfalama birlikte kullanılabilir:

```json
GET /api/notes?tag=backend&sort=title&page=2&pageSize=5
{
  "items": [ { "id": 7, "title": "...", "tags": ["backend"], ... } ],
  "page": 2,
  "pageSize": 5,
  "totalCount": 12,
  "totalPages": 3
}
```

## Çalıştırma

Gereken: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
dotnet run --project src/NotesApi --launch-profile http
```

Ardından tarayıcıda **http://localhost:5107/swagger** adresini aç.

## Öğrendiklerim

Her adımda neyi neden yaptığımı `docs/` klasöründe notlarım olarak tutuyorum.
