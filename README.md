# NotesApi

ASP.NET Core (.NET 10) ile yazılmış bir not uygulaması REST API'si.
Backend geliştirmeyi adım adım öğrenirken büyüttüğüm bir proje: her yeni konu projeye bir özellik olarak ekleniyor.

## Özellikler

- [x] Not ekleme, listeleme, güncelleme, silme (CRUD)
- [x] Gelen verinin doğrulanması (validation)
- [x] Etiketler: tekrarsız, büyük/küçük harf duyarsız (`HashSet`) ve etikete göre filtreleme
- [x] Silmeyi geri alma: en son silinen not önce geri gelir (`ConcurrentStack`, LIFO)
- [x] Swagger arayüzü ile tarayıcıdan test
- [ ] Veritabanı (Entity Framework Core)
- [ ] Global hata yönetimi
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
├── Models/        # Uygulama içi veri modeli
└── Program.cs     # Servis kayıtları ve middleware ayarları
```

## Endpoint'ler

| Metot  | Adres             | Açıklama           | Başarılı yanıt |
|--------|-------------------|--------------------|----------------|
| GET    | `/api/notes`      | Tüm notları getir  | 200 OK         |
| GET    | `/api/notes?tag=backend` | Etikete göre filtrele | 200 OK  |
| GET    | `/api/notes/{id}` | Tek not getir      | 200 OK         |
| POST   | `/api/notes`      | Yeni not ekle      | 201 Created    |
| PUT    | `/api/notes/{id}` | Notu güncelle      | 200 OK         |
| DELETE | `/api/notes/{id}` | Notu sil           | 204 No Content |
| POST   | `/api/notes/undo-delete` | En son silinen notu geri getir | 200 OK |

Not bulunamazsa `404 Not Found`, gönderilen veri hatalıysa `400 Bad Request` döner.

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

## Çalıştırma

Gereken: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
dotnet run --project src/NotesApi --launch-profile http
```

Ardından tarayıcıda **http://localhost:5107/swagger** adresini aç.

## Öğrendiklerim

Her adımda neyi neden yaptığımı `docs/` klasöründe notlarım olarak tutuyorum.
