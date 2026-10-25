# 03 - LINQ Sorguları

LINQ (Language Integrated Query): koleksiyonlar üzerinde sorgu yazmanın yolu.
`Where`, `OrderBy`, `Select` gibi yaklaşık 40 hazır **sorgu operatörü** var ve hepsi **extension method**.

- Namespace'leri `System.Linq`. Projede hiç `using System.Linq;` yazmadım, çünkü `.csproj` dosyasındaki
  `<ImplicitUsings>enable</ImplicitUsings>` bu satırı her dosyaya kendisi ekliyor.
- Extension method kuralı burada da geçerli: namespace eklenmezse `Where` bulunamaz.

## Zincirleme: üretim bandı

```csharp
notes.Where(n => ...)              // süz
     .OrderBy(n => n.Title)        // sırala
     .Select(n => n.ToResponse()); // dönüştür
```

- Veri soldan sağa akar. Her operatörün çıktısı bir sonrakinin girdisi olur.
- Operatörler orijinal koleksiyonu **hiç değiştirmez**, her seferinde yeni bir dizi döndürür.

| Operatör | Lambda'nın görevi | Adı | Projede |
|---|---|---|---|
| `Where` | "Bu eleman kalsın mı?" (`bool`) | predicate | `n => n.Tags.Contains(trimmedTag)` |
| `OrderBy` | "Neye göre sıralayayım?" | key selector | `n => n.Title` |
| `Select` | "Neye dönüştüreyim?" | projection | `n => n.ToResponse()` |

**Query syntax** (`from n in notes where ... select n`) de var. Derleyici onu aynı zincire çevirir.
Ben fluent syntax kullanıyorum, sektörde daha yaygın.

## Deferred execution (ertelenmiş çalışma)

`Where`, `OrderBy`, `Select` çağrıldığında **hiçbir şey çalışmaz**, sadece sorgunun tarifi hazırlanır.
Sorgu ancak şu durumlarda çalışır:

- Üzerinde `foreach` ile gezilince
- `ToList`, `ToArray`, `ToDictionary` gibi dönüştürücüler çağrılınca
- `First`, `Count`, `Any` gibi tek değer döndüren operatörler çağrılınca

İki önemli sonucu var:

- **Reevaluation:** Aynı sorgu üzerinde iki kez gezilirse sorgu iki kez baştan çalışır.
- **Closure:** Lambda dışarıdaki bir değişkeni kullanıyorsa, o değişkenin **sorgu çalıştığı andaki** değeri kullanılır.

Arka planda `Where`, `yield return` ile yazılmış bir iterator:

```csharp
public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    foreach (var element in source)
        if (predicate(element))
            yield return element;
}
```

## Sorguyu adım adım kurmak

Koşullu filtrede kitap ilk yolu öneriyor:

```csharp
if (filtreVar) query = query.Where(...);       // ✅ filtre yoksa hiç operatör eklenmez
query = query.Where(n => !filtreVar || ...);   // ❌ her eleman için gereksiz kontrol
```

`GetAll` içindeki etiket filtresi tam olarak böyle: `tag` boşsa `Where` hiç eklenmiyor.

## Diğer operatörler

- **Tek eleman:** `First`, `Last`, `Single`, `ElementAt`
- **Toplama:** `Count`, `Min`, `Max`
- **Evet/hayır:** `Any`, `Contains`
- **Sırayı kullananlar:** `Take(n)` (ilk n eleman), `Skip(n)` (ilk n elemanı atla)

## Bu projede: sayfalama (`Skip` / `Take`)

`GET /api/notes?page=2&pageSize=5` → atlanacak kayıt sayısı = `(page - 1) * pageSize`.

```csharp
List<Note> matching = sorted.ToList();          // filtre + sıralama BİR KEZ çalışır
long skip = (long)(page - 1) * pageSize;
var items = matching.Skip((int)skip).Take(pageSize).Select(n => n.ToResponse()).ToList();
```

- **Neden önce `ToList`?** Hem toplam sayıya (`Count`) hem de o sayfanın kayıtlarına ihtiyacım var.
  Sorguyu olduğu gibi bıraksaydım filtre ve sıralama iki kez çalışırdı (reevaluation).
- **Neden `long`?** `page` çok büyük olursa (ör. `int.MaxValue`) `int` çarpımı taşar ve negatife döner.
  `Skip` negatif sayı alınca hiçbir şey atlamaz, bu yüzden yanlışlıkla ilk sayfa dönerdi.
- Sayfa numarası son sayfayı aşarsa hata değil, **boş sayfa** dönüyor. `totalPages` yine doğru geliyor.
- `[Range(1, int.MaxValue)]` ve `[Range(1, 100)]` ile `page=0`, `pageSize=101` gibi değerler `400` dönüyor.
  `pageSize` için üst sınır var ki kimse `pageSize=1000000` isteyip sunucuyu yormasın.
- Yanıt `PagedResponse<T>`: generic bir record. İleride başka listeler de aynı biçimi kullanabilir.
- Yanıtın biçimi düz listeden `{ items, page, pageSize, totalCount, totalPages }` biçimine döndü.
  Bu bir **kırıcı değişiklik** (breaking change). Gerçek bir API'de yeni bir sürüm (`v2`) olarak yayınlanırdı.

## İleride: `IQueryable` ve EF Core

| | `IEnumerable` (bellek) | `IQueryable` (veritabanı) |
|---|---|---|
| Lambda | Normal C# kodu olarak çalışır | İfade ağacına (expression tree) çevrilir |
| Sorgu nerede çalışır? | Uygulamanın içinde | EF Core SQL'e çevirir, veritabanında çalışır |

- EF Core temelleri: `DbContext`, `DbSet<T>`, migration'lar, `SaveChanges`, `AsNoTracking`, `Include`.
- ⚠️ **Her LINQ sorgusunun SQL karşılığı yok.** `TagComparer` ve Türkçe `StringComparer` SQL'e çevrilemez.
  Veritabanına geçerken etiket eşleştirmesini ve Türkçe sıralamayı yeniden tasarlamam gerekecek
  (ör. normalize edilmiş etiket saklamak, veritabanının Türkçe collation ayarı).

## Şimdilik atladıklarım

Expression tree'leri elle kurmak (`Expression.Parameter`, `Expression.Lambda`), `AsQueryable`, lazy loading proxy'leri.
