# 01 - Koleksiyonlar (Collections)

.NET'te birden fazla veriyi bir arada tutmak için hazır koleksiyonlar var.
Hangisini seçeceğim üç soruya bağlı:

1. Ne sıklıkla ekleyip sileceğim?
2. Nasıl arayacağım? (sırayla mı, anahtarla mı, "var mı?" diye mi)
3. Sıralı kalması gerekiyor mu?

## Hangi durumda hangi koleksiyon?

| Koleksiyon | Ne zaman seçerim | Benzetme |
|---|---|---|
| `T[]` (dizi) | Eleman sayısı baştan belli ve değişmeyecekse | Yumurta kolisi |
| `List<T>` | Çoğu durumda varsayılan seçim. Sona eklemek hızlı, aramak yavaş | Alışveriş listesi |
| `Dictionary<K,V>` | Bir anahtarla (ör. `Id`) anında bulmam gerekiyorsa | Telefon rehberi |
| `HashSet<T>` | Tekrar istemiyorsam ve "bu var mı?" sorusunu hızlı yanıtlamak istiyorsam | Davetli listesi |
| `Queue<T>` | İlk giren ilk çıksın (FIFO) | Market kuyruğu |
| `Stack<T>` | Son giren ilk çıksın (LIFO) | Tabak yığını, "geri al" tuşu |
| `SortedDictionary<K,V>` / `SortedSet<T>` | Her zaman sıralı kalsın istiyorsam | Alfabetik sözlük |
| `LinkedList<T>` | Ortaya sık ekleme yapıyorsam (nadiren gerekir) | Zincir halkaları |
| `ConcurrentDictionary<K,V>` | Aynı anda birden fazla istek aynı veriye erişiyorsa | Aynı anda birden çok kişinin yazabildiği rehber |

## Hız: O(1) ve O(n)

- **O(1):** Eleman sayısı ne olursa olsun süre aynı. `Dictionary` ile anahtardan bulmak, `HashSet.Contains`.
- **O(log n):** Eleman sayısı arttıkça süre çok az artar. Sıralı koleksiyonlarda arama.
- **O(n):** Süre eleman sayısıyla orantılı artar. `List` içinde arama (`Contains`, `Find`).

## Arayüz merdiveni

Her basamak bir öncekinin yaptığı her şeyi yapar, üstüne bir yetenek ekler:

| Arayüz | Yapabildiği |
|---|---|
| `IEnumerable<T>` | Sadece `foreach` ile sırayla gezmek |
| `ICollection<T>` | + `Count`, `Add`, `Remove`, `Contains` |
| `IList<T>` | + sıra numarasıyla erişim: `liste[3]` |
| `IDictionary<K,V>` | + anahtarla erişim: `sozluk["ali"]` |
| `IReadOnlyList<T>` / `IReadOnlyCollection<T>` | Sadece okuma |

Bir metottan veri döndürürken **ihtiyaç duyulan en dar arayüzü** döndürürüm.
Böylece kodu kullanan taraf listeyi yanlışlıkla değiştiremez.

## Eşitlik ve sıralama

- **`IEqualityComparer<T>`:** "Bu ikisi aynı mı?" sorusuna cevap verir. `Dictionary` ve `HashSet` kullanır.
  - `StringComparer.OrdinalIgnoreCase` ile `"C#"` ve `"c#"` aynı kabul edilir.
- **`IComparer<T>`:** "Hangisi önce gelir?" sorusuna cevap verir. Sıralama işlemleri kullanır.
  - Negatif sayı: x önce, `0`: eşit, pozitif sayı: y önce.
  - Türkçe sıralama için `StringComparer.Create(new CultureInfo("tr-TR"), true)` kullanılır.

## Kullanmadığım eski türler

`ArrayList`, `Hashtable`, `CollectionBase`, `DictionaryBase` eski kodlarla uyumluluk için duruyor.
Yeni kodda bunların yerine generic (`<T>`) versiyonları kullanırım.

## Bu projede nerede kullandım?

- `InMemoryNoteService` notları `ConcurrentDictionary<int, Note>` içinde tutuyor.
  - `GetById` çağrısında not, `Id` anahtarıyla doğrudan bulunuyor: O(1). `List` olsaydı tek tek aranırdı: O(n).
  - API'ye aynı anda birden fazla istek gelebildiği için "concurrent" (thread-safe) versiyonu seçtim.
- `INoteService.GetAll()` metodu `IEnumerable<NoteResponse>` döndürüyor. Controller notları sadece gezebiliyor, listeyi değiştiremiyor.

### Etiket özelliği

- Etiketler `Note.Tags` içinde `HashSet<string>` olarak tutuluyor:
  - Aynı etiket iki kez eklenmiyor. `Add` tekrar eden değeri sessizce yok sayıyor.
  - Set `StringComparer.OrdinalIgnoreCase` ile kurulduğu için `"C#"` ve `"c#"` aynı etiket sayılıyor.
  - `?tag=` filtresindeki `Contains` çağrısı O(1) çalışıyor.
- İstemci etiketleri düz bir `List<string>` olarak gönderiyor. Temizleme (boşları atma, `Trim`) ve tekrar ayıklama serviste yapılıyor.
- Yanıtta etiketler `IReadOnlyCollection<string>` olarak dönüyor. Bu, içerideki setin kopyası (`ToArray()`). Dışarıya verilen nesne üzerinden içerideki veri değiştirilemiyor.
- `HashSet` sıra garantisi vermediği için yanıtta etiketler alfabetik sıralanıyor.
- Güncellemede eski setin içeriği değiştirilmiyor, yerine yeni bir set atanıyor. Böylece aynı anda notu okuyan başka bir istek yarım kalmış bir set görmüyor.
