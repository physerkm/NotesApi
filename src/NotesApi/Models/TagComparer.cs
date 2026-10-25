namespace NotesApi.Models;

// Etiketlerin "aynı mı?" sorusunu cevaplar.
// Sorun: Türkçede I -> ı ve İ -> i, İngilizcede I -> i.
// Etiketler iki dilde de olabildiği için (İstanbul, LINQ) tek bir kültürün kuralı yetmez:
//   OrdinalIgnoreCase: "İstanbul" != "istanbul"
//   tr-TR ignoreCase:  "LINQ" != "linq"
// Çözüm: karşılaştırmadan önce I, ı, İ, i harflerinin hepsini "i" yapıyoruz.
// Bedeli: "ılık" ve "ilik" aynı etiket sayılır (etiketlerde nadir bir durum).
public sealed class TagComparer : EqualityComparer<string>
{
    // Durumu (state) olmadığı için tek bir örnek yeterli.
    public static readonly TagComparer Instance = new();

    private TagComparer() { }

    public override bool Equals(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return string.Equals(Normalize(x), Normalize(y), StringComparison.Ordinal);
    }

    // Kural: Equals'a göre eşit olan iki etiketin hash kodu da aynı olmalı.
    // Bu yüzden ikisi de aynı Normalize'dan geçiyor.
    // Kural bozulursa HashSet aynı etiketi iki farklı "kova"ya koyar ve tekrarları yakalayamaz.
    public override int GetHashCode(string obj) =>
        Normalize(obj).GetHashCode(StringComparison.Ordinal);

    private static string Normalize(string s) =>
        s.Replace('İ', 'i')
         .Replace('ı', 'i')
         .ToLowerInvariant(); // Geriye kalan I -> i ve diğer tüm büyük harfler.
}
