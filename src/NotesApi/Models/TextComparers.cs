using System.Globalization;

namespace NotesApi.Models;

// Projede birden fazla yerde kullanılan metin karşılaştırıcıları.
public static class TextComparers
{
    // Türkçe alfabe kurallarıyla sıralar: C < Ç < D, I < İ, O < Ö, S < Ş, U < Ü.
    // Ordinal karşılaştırma harfin kod numarasına bakar ve "Çiçek"i "Zeytin"den sonraya koyar.
    // ignoreCase: true -> "elma" ve "Elma" sıralamada yan yana gelir.
    // static readonly: bir kez oluşturulur, her istekte yeniden yaratılmaz.
    public static readonly StringComparer Turkish =
        StringComparer.Create(new CultureInfo("tr-TR"), ignoreCase: true);
}
