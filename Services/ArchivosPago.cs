namespace ChachaWilka.Services;

public static class ArchivosPago
{
    public const int Maximo = 5 * 1024 * 1024;

    public static async Task<(byte[]? Evidencia, string? Tipo)> LeerAsync(IFormFile? archivo, CancellationToken cancellationToken)
    {
        if (archivo is null)
            return (null, null);
        if (archivo.Length is <= 0 or > Maximo)
            throw new ReglaTiendaException("Adjunta un archivo de hasta 5 MB.");

        using var memoria = new MemoryStream((int)archivo.Length);
        await archivo.CopyToAsync(memoria, cancellationToken);
        var bytes = memoria.ToArray();
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var tipo = Detectar(bytes, extension);
        if (tipo is null)
            throw new ReglaTiendaException("El comprobante debe ser un JPG, PNG o PDF válido.");
        return (bytes, tipo);
    }

    private static string? Detectar(ReadOnlySpan<byte> datos, string extension)
    {
        ReadOnlySpan<byte> firmaPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        ReadOnlySpan<byte> cierreJpg = [0xFF, 0xD9];
        ReadOnlySpan<byte> firmaJpg = [0xFF, 0xD8, 0xFF];
        if (extension == ".png" && datos.Length >= 45 && datos.StartsWith(firmaPng) &&
            datos.Slice(12, 4).SequenceEqual("IHDR"u8) && datos.Slice(datos.Length - 8, 4).SequenceEqual("IEND"u8))
            return "image/png";
        if ((extension is ".jpg" or ".jpeg") && datos.Length >= 12 && datos.StartsWith(firmaJpg) && datos.EndsWith(cierreJpg))
            return "image/jpeg";
        if (extension == ".pdf" && datos.Length >= 30 && datos.StartsWith("%PDF-"u8) &&
            datos[Math.Max(0, datos.Length - 1024)..].LastIndexOf("%%EOF"u8) >= 0)
            return "application/pdf";
        return null;
    }
}
