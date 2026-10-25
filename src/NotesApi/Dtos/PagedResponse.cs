namespace NotesApi.Dtos;

// Generic (<T>): sadece notlar için değil, ileride her tür liste için kullanılabilir.

/// <summary>
/// Sayfalara bölünmüş liste.
/// </summary>
/// <param name="Items">Bu sayfadaki kayıtlar. Sayfa numarası son sayfayı aşarsa boş liste döner.</param>
/// <param name="Page">Şu anki sayfa numarası (1'den başlar).</param>
/// <param name="PageSize">Sayfa başına kayıt sayısı.</param>
/// <param name="TotalCount">Filtreye uyan toplam kayıt sayısı (bütün sayfalar).</param>
/// <param name="TotalPages">Toplam sayfa sayısı. Hiç kayıt yoksa 0.</param>
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
