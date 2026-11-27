const escena = document.querySelector("[data-escena]");
const muestra = escena.querySelector("[data-muestra]");
const laminas = [...muestra.querySelectorAll("[data-lamina]")];
const sabores = [...escena.querySelectorAll("[data-sabor]")];
let saborActual = 0;

function mostrarSabor(indice) {
    saborActual = (indice + sabores.length) % sabores.length;
    const activo = sabores[saborActual];
    escena.dataset.tono = activo.dataset.sabor;
    for (const boton of sabores) boton.setAttribute("aria-pressed", String(boton === activo));
    for (const lamina of laminas) lamina.setAttribute("aria-hidden", String(lamina.dataset.lamina !== activo.dataset.sabor));
    const nota = laminas[saborActual].querySelector("figcaption span").textContent;
    escena.querySelector("[data-lectura-escena]").textContent = activo.textContent.trim() + ". " + nota;
}

sabores.forEach((boton, indice) => {
    boton.addEventListener("click", () => mostrarSabor(indice));
    boton.addEventListener("keydown", evento => {
        if (!["ArrowLeft", "ArrowRight"].includes(evento.key)) return;
        evento.preventDefault();
        mostrarSabor(indice + (evento.key === "ArrowRight" ? 1 : -1));
        sabores[saborActual].focus();
    });
});

let inicioGesto;
muestra.addEventListener("pointerdown", evento => {
    if (!evento.isPrimary || evento.button !== 0) return;
    inicioGesto = { x: evento.clientX, y: evento.clientY, id: evento.pointerId };
    muestra.setPointerCapture(evento.pointerId);
});
muestra.addEventListener("pointercancel", () => { inicioGesto = null; });
muestra.addEventListener("pointerup", evento => {
    if (inicioGesto?.id !== evento.pointerId) return;
    const avance = evento.clientX - inicioGesto.x;
    const vertical = evento.clientY - inicioGesto.y;
    inicioGesto = null;
    if (Math.abs(avance) > 38 && Math.abs(avance) > Math.abs(vertical) * 1.3)
        mostrarSabor(saborActual + (avance < 0 ? 1 : -1));
});
escena.classList.add("interactiva");

const ficha = document.querySelector("[data-ficha]");
const admiteFicha = typeof ficha.showModal === "function";
let coleccion = document.getElementById("coleccion");
let leyendo;
let categoriaMostrada = new URL(location.href).searchParams.get("categoria");

function prepararColeccion() {
    if (admiteFicha) {
        for (const boton of coleccion.querySelectorAll("[data-abrir-ficha]")) boton.hidden = false;
    }
}
prepararColeccion();

function abrirFicha(producto) {
    const foto = producto.querySelector(".producto-foto img, .marca-sola").cloneNode(true);
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
    leyendo?.abort();
    const solicitud = new AbortController();
    leyendo = solicitud;
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
        coleccion.replaceWith(nueva);
        coleccion = nueva;
        prepararColeccion();
        categoriaMostrada = destino.searchParams.get("categoria");
        if (agregarHistorial) {
            history.pushState(null, "", destino);
            coleccion.querySelector(".filtros [aria-current]").focus({ preventScroll: true });
        }
        document.querySelector("[data-resultado]").textContent =
            coleccion.querySelector(".filtros [aria-current]").textContent + " · " + coleccion.querySelectorAll(".producto").length + " productos.";
    } catch (error) {
        if (!solicitud.signal.aborted) location.assign(destino);
    } finally {
        if (leyendo === solicitud) coleccion.removeAttribute("aria-busy");
    }
}

document.addEventListener("click", evento => {
    const abrir = evento.target.closest("[data-abrir-ficha]");
    if (abrir && admiteFicha) abrirFicha(abrir.closest(".producto"));
    const filtro = evento.target.closest(".filtros a");
    if (!filtro || evento.button !== 0 || evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey) return;
    evento.preventDefault();
    cargarColeccion(new URL(filtro.href), true);
});
window.addEventListener("popstate", () => {
    leyendo?.abort();
    const destino = new URL(location.href);
    if (destino.searchParams.get("categoria") !== categoriaMostrada) cargarColeccion(destino, false);
});

const enlacesCajas = [...document.querySelectorAll(".indice-cajas a")];
if (enlacesCajas.length) {
    const lista = document.querySelector(".cajas-lista");
    const tarjetas = enlacesCajas.map(enlace => document.getElementById(enlace.hash.slice(1)));
    function marcarCaja(indice) {
        enlacesCajas.forEach((enlace, posicion) => {
            if (indice === posicion) enlace.setAttribute("aria-current", "true");
            else enlace.removeAttribute("aria-current");
        });
    }
    marcarCaja(0);
    if ("IntersectionObserver" in window) {
        const proporciones = new Map();
        const observarCajas = new IntersectionObserver(entradas => {
            for (const entrada of entradas) proporciones.set(entrada.target, entrada.intersectionRatio);
            const principal = tarjetas.reduce((a, b) => (proporciones.get(a) || 0) >= (proporciones.get(b) || 0) ? a : b);
            if (proporciones.get(principal) > 0) marcarCaja(tarjetas.indexOf(principal));
        }, { threshold: [0, .25, .5, .75, 1] });
        tarjetas.forEach(tarjeta => observarCajas.observe(tarjeta));
    }
    enlacesCajas.forEach((enlace, indice) => enlace.addEventListener("click", evento => {
        if (!matchMedia("(max-width: 760px)").matches || evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey) return;
        evento.preventDefault();
        lista.scrollTo({ left: tarjetas[indice].offsetLeft - tarjetas[0].offsetLeft,
            behavior: matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth" });
        marcarCaja(indice);
    }));
}
