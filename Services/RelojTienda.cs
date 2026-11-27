namespace ChachaWilka.Services;

public static class RelojTienda
{
    private static readonly TimeZoneInfo Zona = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");

    public static DateOnly Hoy => DateOnly.FromDateTime(EnLima(DateTimeOffset.UtcNow).DateTime);
    public static DateTimeOffset EnLima(DateTimeOffset fecha) => TimeZoneInfo.ConvertTime(fecha, Zona);
    public static DateTimeOffset Medianoche(DateOnly fecha)
    {
        var hora = fecha.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(hora, Zona.GetUtcOffset(hora));
    }
}
