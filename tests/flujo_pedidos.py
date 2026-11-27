"""Prueba de compra con cuentas ficticias. Ejecutar únicamente contra una base aislada."""
import os
import re
from base64 import b64decode
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timedelta, timezone
from html import unescape
from html.parser import HTMLParser
from http.cookiejar import CookieJar
from urllib.error import HTTPError
from urllib.parse import urlencode, urlsplit
from urllib.request import HTTPCookieProcessor, Request, build_opener
from uuid import uuid4

URL = os.environ.get("TIENDA_QA_URL", "http://127.0.0.1:5283").rstrip("/")
if urlsplit(URL).hostname not in {"127.0.0.1", "localhost", "::1"}:
    raise SystemExit("La prueba modifica datos ficticios y solo admite una aplicación en loopback.")
CLAVE = "Tienda-Prueba-Automatizada!041"
LOTE = uuid4().hex[:10]
HOY = datetime.now(timezone(timedelta(hours=-5))).date().isoformat()


class Campos(HTMLParser):
    def __init__(self, html):
        super().__init__()
        self.valores = {}
        self.feed(html)

    def handle_starttag(self, tag, attrs):
        campo = dict(attrs)
        if tag == "input" and campo.get("name"):
            self.valores[campo["name"]] = campo.get("value", "")


def navegador():
    return build_opener(HTTPCookieProcessor(CookieJar()))


def pedir(cliente, ruta, datos=None):
    cuerpo = urlencode(datos, doseq=True).encode() if datos is not None else None
    try:
        with cliente.open(URL + ruta, cuerpo, timeout=15) as respuesta:
            return respuesta.status, urlsplit(respuesta.url).path, respuesta.read().decode()
    except HTTPError as error:
        return error.code, urlsplit(error.url).path, error.read().decode()


def pagina(cliente, ruta):
    estado, _, html = pedir(cliente, ruta)
    if estado != 200:
        raise AssertionError(f"GET {ruta}: {estado}")
    return html


def enviar(cliente, pagina_origen, ruta, datos):
    campos = Campos(pagina_origen).valores
    return pedir(cliente, ruta, {"__RequestVerificationToken": campos["__RequestVerificationToken"], **datos})


def enviar_imagen(cliente, pagina_origen, ruta, datos):
    token = Campos(pagina_origen).valores["__RequestVerificationToken"]
    firma = "form-" + uuid4().hex
    partes = [f'--{firma}\r\nContent-Disposition: form-data; name="{k}"\r\n\r\n{v}\r\n'.encode()
              for k, v in {"__RequestVerificationToken": token, **datos}.items()]
    partes.append(f'--{firma}\r\nContent-Disposition: form-data; name="Evidencia"; filename="prueba.png"\r\n'
                  'Content-Type: image/png\r\n\r\n'.encode())
    partes.append(b64decode("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a7kcAAAAASUVORK5CYII="))
    partes.append(f"\r\n--{firma}--\r\n".encode())
    peticion = Request(URL + ruta, b"".join(partes), {"Content-Type": f"multipart/form-data; boundary={firma}"})
    with cliente.open(peticion, timeout=15) as respuesta:
        return respuesta.status, urlsplit(respuesta.url).path, respuesta.read().decode()


def verificar(resultado, *textos):
    estado, ruta, html = resultado
    assert estado == 200, f"{ruta}: HTTP {estado}"
    for texto in textos:
        assert texto in unescape(html), f"{ruta}: falta {texto!r}"
    return ruta, html


def ingresar(cliente, correo, clave):
    return enviar(cliente, pagina(cliente, "/Login/Login"), "/Login/Login", {"Correo": correo, "Contraseña": clave})


def registrar(cliente, correo, nacimiento):
    return enviar(cliente, pagina(cliente, "/Login/Registro"), "/Login/Registro", {
        "Nombre": "Cliente de prueba", "Correo": correo, "Telefono": "999000000", "Nacimiento": nacimiento,
        "Contraseña": CLAVE, "RepiteContraseña": CLAVE, "Consentimiento": "true", "Id_Rol": "1",
    })


def comprobar_ultima_unidad(admin, empleado, clientes):
    formulario = pagina(admin, "/Administracion/Producto")
    categoria = re.search(r'<option value="(\d+)">Licores</option>', formulario)[1]
    nombre = "Licor de prueba QA-" + LOTE
    _, catalogo = verificar(enviar(admin, formulario, "/Administracion/Producto", {
        "Nombre": nombre, "Descripcion": "Producto ficticio para carreras de stock.",
        "CategoriaId": categoria, "Precio": "25.00", "Activo": "true",
    }), "Producto guardado")
    producto = re.search(r'href="/Administracion/Producto/(\d+)">' + nombre, catalogo)[1]
    verificar(enviar(admin, catalogo, f"/Administracion/Stock/{producto}", {
        "cantidad": "1", "motivo": "Última unidad para prueba " + LOTE,
    }), "Existencias actualizadas")

    confirmaciones = []
    for cliente in clientes:
        verificar(enviar(cliente, pagina(cliente, "/"), "/Cesta/Agregar", {"id": producto}), "Añadido a tu cesta")
        cesta = pagina(cliente, "/Cesta")
        campos = Campos(cesta).valores
        confirmaciones.append((cliente, cesta, {k: campos[k] for k in ("clave", "totalCentimos")}))

    def confirmar(datos):
        cliente, cesta, valores = datos
        return enviar(cliente, cesta, "/Cesta/Confirmar", valores)

    with ThreadPoolExecutor(max_workers=2) as ejecutor:
        resultados = list(ejecutor.map(confirmar, confirmaciones))
    pedidos = [ruta for estado, ruta, _ in resultados if estado == 200 and ruta.startswith("/Pedidos/Detalle/")]
    assert len(pedidos) == 1 and all(estado == 200 for estado, _, _ in resultados), "La última unidad solo admite un pedido."
    pedido = pedidos[0].rsplit("/", 1)[-1]
    ruta_avance = f"/Operacion/Avanzar/{pedido}"
    detalle = pagina(empleado, pedidos[0])
    _, detalle = verificar(enviar(empleado, detalle, ruta_avance, {
        "estado": "Cancelado", "motivo": "Fin del escenario de prueba.",
    }), "Pedido cancelado")
    verificar(enviar(empleado, detalle, ruta_avance, {
        "estado": "Cancelado", "motivo": "Segundo intento de cancelación.",
    }), "No se puede aplicar ese cambio")


def ejecutar():
    admin, cliente, empleado, otro = [navegador() for _ in range(4)]
    assert "Licor de Aguaymanto" in pagina(cliente, "/")
    assert "Crema de Lúcuma" in unescape(pagina(cliente, "/?categoria=Cremas"))
    assert "Licor de Aguaymanto" not in pagina(cliente, "/?categoria=Cremas")

    verificar(ingresar(admin, os.environ["TIENDA_QA_ADMIN_CORREO"], os.environ["TIENDA_QA_ADMIN_CLAVE"]), "Catálogo e inventario")
    catalogo = pagina(admin, "/Administracion/Catalogo")
    licores = re.findall(r'href="/Administracion/Producto/(\d+)">Licor de ', catalogo)
    assert len(licores) >= 4, "Faltan licores para probar la caja."
    for producto in licores[:4]:
        verificar(enviar(admin, catalogo, f"/Administracion/Stock/{producto}", {
            "cantidad": "6", "motivo": "Prueba automatizada " + LOTE,
        }), "Existencias actualizadas")

    cuentas = pagina(admin, "/Administracion/Usuarios")
    correo_empleado = f"empleado-{LOTE}@example.invalid"
    verificar(enviar(admin, cuentas, "/Administracion/CrearPersonal", {
        "Nombre": "Empleado de prueba", "Correo": correo_empleado, "Clave": CLAVE, "Rol": "Empleado",
    }), "Cuenta creada.")
    verificar(ingresar(empleado, correo_empleado, CLAVE), "Operación")
    assert pedir(empleado, "/Administracion/Usuarios")[0] == 403

    correo = f"cliente-{LOTE}@example.invalid"
    verificar(registrar(cliente, correo, "2015-01-01"), "Debes tener al menos 18 años")
    verificar(registrar(cliente, correo, "1994-01-01"), "Tu cuenta está lista")
    verificar(ingresar(cliente, correo, CLAVE), "Mis pedidos")
    assert pedir(cliente, "/Administracion/Usuarios")[0] == 403

    caja = pagina(cliente, "/Cesta/Combo?categoria=Licores")
    verificar(enviar(cliente, caja, "/Cesta/AgregarCombo", {
        "categoria": "Licores", "seleccion": [licores[0]] * 4,
    }), "Una caja ya no cumple")
    verificar(enviar(cliente, caja, "/Cesta/AgregarCombo", {
        "categoria": "Licores", "seleccion": licores[:4],
    }), "La caja está en tu cesta")

    cesta = pagina(cliente, "/Cesta")
    campos = Campos(cesta).valores
    confirmacion = {nombre: campos[nombre] for nombre in ("clave", "totalCentimos")}
    ruta, detalle = verificar(enviar(cliente, cesta, "/Cesta/Confirmar", confirmacion), "Pendiente de pago", "994040462")
    pedido = ruta.rsplit("/", maxsplit=1)[-1]
    assert pedido.isdigit(), f"Ruta de pedido inesperada: {ruta}"
    assert enviar(cliente, cesta, "/Cesta/Confirmar", confirmacion)[1] == ruta, "Se duplicó la confirmación."

    correo_otro = f"otro-{LOTE}@example.invalid"
    verificar(registrar(otro, correo_otro, "1990-01-01"), "Tu cuenta está lista")
    verificar(ingresar(otro, correo_otro, CLAVE), "Mis pedidos")
    assert pedir(otro, ruta)[0] == 404

    _, detalle = verificar(enviar(cliente, detalle, f"/Pedidos/Pago/{pedido}", {
        "Referencia": f"TEST-MAL-{LOTE}", "Monto": "50.00", "Fecha": HOY,
    }), "Pago en revisión")
    revision = pagina(empleado, ruta)
    pago = Campos(revision).valores["pago"]
    verificar(enviar(empleado, revision, f"/Operacion/Revisar/{pedido}", {
        "pago": pago, "aprobar": "true", "recepcionConfirmada": "true",
    }), "El importe informado no coincide")
    verificar(enviar(empleado, revision, f"/Operacion/Revisar/{pedido}", {
        "pago": pago, "aprobar": "false", "motivo": "Monto diferente en el ejemplo.",
    }), "Pendiente de pago")

    detalle = pagina(cliente, ruta)
    _, detalle = verificar(enviar_imagen(cliente, detalle, f"/Pedidos/Pago/{pedido}", {
        "Referencia": f"TEST-BIEN-{LOTE}", "Monto": "100.00", "Fecha": HOY,
    }), "Pago en revisión", "Descargar comprobante")
    ruta_evidencia = re.search(r'href="(/Pedidos/Evidencia/\d+)"', detalle)[1]
    assert pedir(otro, ruta_evidencia)[0] == 404
    with empleado.open(URL + ruta_evidencia, timeout=15) as archivo:
        assert archivo.headers.get_content_type() == "application/octet-stream"
        assert archivo.read().startswith(b"\x89PNG\r\n\x1a\n")
    revision = pagina(empleado, ruta)
    pago = Campos(revision).valores["pago"]
    _, revision = verificar(enviar(empleado, revision, f"/Operacion/Revisar/{pedido}", {
        "pago": pago, "aprobar": "true", "recepcionConfirmada": "true",
    }), "Pago verificado en Yape")
    for estado, texto in [
        ("EnPreparacion", "En preparación"), ("ListoRecojo", "Listo para recoger"), ("Retirado", "Retirado"),
    ]:
        _, revision = verificar(enviar(empleado, revision, f"/Operacion/Avanzar/{pedido}", {
            "estado": estado, "identidadVerificada": "true",
        }), texto)

    verificar(enviar(cliente, detalle, f"/Pedidos/Consultar/{pedido}", {
        "pregunta": "Consulta ficticia de recojo",
    }), "Consulta enviada")
    revision = pagina(empleado, ruta)
    consulta = Campos(revision).valores["consulta"]
    verificar(enviar(empleado, revision, f"/Operacion/Responder/{pedido}", {
        "consulta": consulta, "respuesta": "Respuesta ficticia de recojo",
    }), "Respuesta ficticia de recojo")
    assert "Respuesta ficticia de recojo" in unescape(pagina(cliente, ruta))
    assert "Ventas retiradas" in pagina(admin, "/Administracion/Reportes")
    comprobar_ultima_unidad(admin, empleado, (cliente, otro))
    print("Compra verificada: roles, catálogo, combos, reserva concurrente, confirmación única, Yape, recojo, consultas, cancelación y reportes.")


if __name__ == "__main__":
    ejecutar()
