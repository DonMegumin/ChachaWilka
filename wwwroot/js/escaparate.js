const ficha = document.querySelector("[data-ficha]");
const admiteFicha = typeof ficha.showModal === "function";
const resultado = document.querySelector("[data-resultado]");
let coleccion = document.getElementById("coleccion");
let solicitudActual;
let categoriaMostrada = new URL(location.href).searchParams.get("categoria");
let consulta = "";
let anunciarBusqueda;

function normalizar(texto) {
    return texto.toLowerCase().normalize("NFD").replace(/\p{M}/gu, "").trim();
}

function prepararColeccion() {
    if (admiteFicha) {
        for (const boton of coleccion.querySelectorAll("[data-abrir-ficha]")) boton.hidden = false;
    }
    const control = coleccion.querySelector("[data-busqueda]");
    const productos = [...coleccion.querySelectorAll(".producto")];
    const vacio = coleccion.querySelector("[data-sin-resultados]");
    const aplicar = () => {
        const terminos = normalizar(consulta).split(/\s+/);
        let visibles = 0;
        for (const producto of productos) {
            const texto = normalizar(producto.querySelector(".producto-texto").textContent);
            producto.hidden = !terminos.every(termino => texto.includes(termino));
            if (!producto.hidden) visibles++;
        }
        vacio.hidden = visibles > 0 || productos.length === 0;
        return visibles;
    };
    if (control) {
        const campo = control.querySelector("input");
        control.hidden = false;
        campo.value = consulta;
        campo.addEventListener("input", () => {
            consulta = campo.value;
            const visibles = aplicar();
            clearTimeout(anunciarBusqueda);
            anunciarBusqueda = setTimeout(() => {
                resultado.textContent = visibles === 1 ? "1 sabor encontrado." : `${visibles} sabores encontrados.`;
            }, 220);
        });
    }
    return aplicar();
}
prepararColeccion();

function abrirFicha(producto) {
    const foto = producto.querySelector(".producto-foto img, .marca-sola").cloneNode(true);
    if (foto instanceof HTMLImageElement) foto.loading = "eager";
    ficha.querySelector("[data-ficha-foto]").replaceChildren(foto);
    ficha.querySelector("#ficha-nombre").textContent = producto.querySelector("h3").textContent;
    ficha.querySelector("[data-ficha-categoria]").textContent = producto.querySelector(".detalle-categoria").textContent;
    ficha.querySelector("[data-ficha-descripcion]").textContent = producto.querySelector("h3 + p").textContent;
    const acciones = producto.querySelector(".producto-acciones").cloneNode(true);
    const comprar = acciones.querySelector("button");
    if (comprar) {
        comprar.className = "boton boton-primario";
        comprar.textContent = "Añadir a la cesta";
    }
    ficha.querySelector("[data-ficha-acciones]").replaceChildren(...acciones.children);
    ficha.showModal();
}
ficha.addEventListener("click", evento => {
    if (evento.target === ficha) ficha.close();
});

async function cargarColeccion(destino, agregarHistorial) {
    solicitudActual?.abort();
    const solicitud = new AbortController();
    solicitudActual = solicitud;
    coleccion.setAttribute("aria-busy", "true");
    const pagina = new URL(destino);
    pagina.searchParams.set("soloColeccion", "true");
    try {
        const respuesta = await fetch(pagina, { signal: solicitud.signal });
        if (!respuesta.ok) throw new Error();
        const html = await respuesta.text();
        const nueva = new DOMParser().parseFromString(html, "text/html").getElementById("coleccion");
        if (!nueva) throw new Error();
        if (solicitud.signal.aborted) return;
        clearTimeout(anunciarBusqueda);
        coleccion.replaceWith(nueva);
        coleccion = nueva;
        const visibles = prepararColeccion();
        categoriaMostrada = destino.searchParams.get("categoria");
        const filtroActivo = coleccion.querySelector(".filtros [aria-current]");
        if (agregarHistorial) {
            history.pushState(null, "", destino);
            filtroActivo.focus({ preventScroll: true });
        }
        resultado.textContent = `${filtroActivo.textContent} · ${visibles} ${visibles === 1 ? "sabor" : "sabores"}.`;
    } catch {
        if (!solicitud.signal.aborted) location.assign(destino);
    } finally {
        if (solicitudActual === solicitud) {
            solicitudActual = null;
            coleccion.removeAttribute("aria-busy");
        }
    }
}

document.addEventListener("click", evento => {
    const abrir = evento.target.closest("[data-abrir-ficha]");
    if (abrir && admiteFicha) abrirFicha(abrir.closest(".producto"));
    const filtro = evento.target.closest(".filtros a");
    if (!filtro || evento.button !== 0 || evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey) return;
    evento.preventDefault();
    if (filtro.hasAttribute("aria-current") && !solicitudActual) return;
    cargarColeccion(new URL(filtro.href), true);
});
window.addEventListener("popstate", () => {
    solicitudActual?.abort();
    const destino = new URL(location.href);
    if (destino.searchParams.get("categoria") !== categoriaMostrada) cargarColeccion(destino, false);
});

const momento = document.querySelector("[data-momento-cajas]");
if ("IntersectionObserver" in window && !matchMedia("(prefers-reduced-motion: reduce)").matches) {
    const observar = new IntersectionObserver((entradas, observador) => {
        if (!entradas.some(entrada => entrada.isIntersecting)) return;
        momento.classList.add("momento-visible");
        observador.disconnect();
    }, { threshold: .18 });
    observar.observe(momento);
}
