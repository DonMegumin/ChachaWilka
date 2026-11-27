using ChachaWilka.Models;

namespace ChachaWilka.Services;

public static class CatalogoMarca
{
    public const string Licores = "Licores";
    public const string Cremas = "Cremas";

    public static IReadOnlyList<Producto> Productos() =>
    [
        Producto("licor-aguaymanto", "Licor de Aguaymanto", Licores, 25, "Un sabor frutal con el punto ácido del aguaymanto."),
        Producto("licor-cafe", "Licor de Café", Licores, 25, "El aroma y el carácter del café."),
        Producto("licor-chuchuwasi", "Licor de Chuchuwasi", Licores, 25, "El sabor de una corteza tradicional amazónica."),
        Producto("licor-coco", "Licor de Coco", Licores, 25, "Coco con un perfil suave y tropical."),
        Producto("licor-maracuya", "Licor de Maracuyá", Licores, 25, "Maracuyá de sabor vivo y frutal."),
        Producto("licor-menta", "Licor de Menta", Licores, 25, "La frescura herbal de la menta."),
        Producto("licor-mora", "Licor de Mora", Licores, 25, "Mora con un perfil frutal."),
        Producto("crema-cafe", "Crema de Café", Cremas, 30, "La intensidad del café en una crema."),
        Producto("crema-coco", "Crema de Coco", Cremas, 30, "Coco y textura cremosa."),
        Producto("crema-durazno", "Crema de Durazno", Cremas, 30, "El perfil frutal del durazno en una crema."),
        Producto("crema-lucuma", "Crema de Lúcuma", Cremas, 30, "Una crema de sabor a lúcuma."),
        Producto("crema-menta", "Crema de Menta", Cremas, 30, "Menta fresca en una crema.")
    ];

    private static Producto Producto(string referencia, string nombre, string categoria, decimal precio, string descripcion) => new()
    {
        ReferenciaCatalogo = referencia,
        Nombre = nombre,
        Descripcion = descripcion,
        Categoria = categoria,
        Precio = precio,
        Imagen = referencia + ".jpg"
    };
}
