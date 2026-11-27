const cabecera = document.querySelector(".cabecera");
const menu = cabecera.querySelector(".menu-movil");
const navegacion = cabecera.querySelector(".navegacion");
function alternarMenu(abierto) {
    cabecera.classList.toggle("abierta", abierto);
    menu.setAttribute("aria-expanded", String(abierto));
}
menu.hidden = false;
cabecera.classList.add("con-menu");
menu.addEventListener("click", () => {
    const abrir = menu.getAttribute("aria-expanded") !== "true";
    alternarMenu(abrir);
    if (abrir) navegacion.querySelector("a")?.focus();
});
navegacion.addEventListener("click", () => alternarMenu(false));
cabecera.addEventListener("focusout", evento => {
    if (!cabecera.contains(evento.relatedTarget)) alternarMenu(false);
});
document.addEventListener("keydown", evento => {
    if (evento.key === "Escape" && cabecera.classList.contains("abierta")) {
        alternarMenu(false);
        menu.focus();
    }
});

const caja = document.querySelector("[data-combo]");
if (caja) {
    const selectores = [...caja.querySelectorAll("select")];
    const progreso = caja.querySelector("[data-progreso]");
    const puedeRepetir = caja.dataset.repeticiones === "true";
    const bandeja = caja.querySelector("[data-bandeja]");
    bandeja.hidden = false;

    function actualizarCaja() {
        const elegidos = selectores.map(selector => selector.value).filter(Boolean);
        selectores.forEach((selector, indice) => {
            for (const opcion of selector.options) {
                const limite = puedeRepetir ? Number(opcion.dataset.unidades) : 1;
                opcion.disabled = !!opcion.value && opcion.value !== selector.value &&
                    elegidos.filter(valor => valor === opcion.value).length >= limite;
            }
            const hueco = bandeja.children[indice];
            const opcion = selector.selectedOptions[0];
            if (hueco.dataset.valor === opcion.value) return;
            hueco.dataset.valor = opcion.value;
            hueco.classList.toggle("lleno", !!opcion.value);
            if (opcion.dataset.foto) {
                const imagen = new Image();
                imagen.alt = "";
                imagen.src = opcion.dataset.foto;
                hueco.replaceChildren(imagen);
            } else {
                const etiqueta = document.createElement("span");
                etiqueta.textContent = opcion.value ? "✓" : String(indice + 1).padStart(2, "0");
                hueco.replaceChildren(etiqueta);
            }
        });
        progreso.textContent = `${elegidos.length} de 4 sabores seleccionados`;
    }

    caja.addEventListener("change", actualizarCaja);
    window.addEventListener("pageshow", actualizarCaja);
    actualizarCaja();
}

const botonConfirmar = document.querySelector("[data-enviar]");
if (botonConfirmar) {
    botonConfirmar.form.addEventListener("submit", () => {
        botonConfirmar.textContent = botonConfirmar.dataset.enviar;
        botonConfirmar.disabled = true;
    });
    window.addEventListener("pageshow", event => {
        if (event.persisted) {
            botonConfirmar.disabled = false;
            botonConfirmar.textContent = "Confirmar pedido ↗";
        }
    });
}

if ("IntersectionObserver" in window && !matchMedia("(prefers-reduced-motion: reduce)").matches) {
    const observador = new IntersectionObserver((entradas, origen) => {
        for (const entrada of entradas) {
            if (entrada.isIntersecting) {
                entrada.target.classList.add("revelar");
                origen.unobserve(entrada.target);
            }
        }
    }, { threshold: .16 });
    document.querySelectorAll(".producto, .caja").forEach(elemento => observador.observe(elemento));
}
