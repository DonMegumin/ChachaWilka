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
navegacion.addEventListener("click", evento => {
    if (!evento.target.closest("a")) return;
    if (cabecera.classList.contains("abierta")) menu.focus({ preventScroll: true });
    alternarMenu(false);
});
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
    const selectores = [...caja.querySelectorAll("select[name=seleccion]")];
    const progreso = caja.querySelector("[data-progreso]");
    const puedeRepetir = caja.dataset.repeticiones === "true";
    const bandeja = caja.querySelector("[data-bandeja]");
    const selectorSabores = caja.querySelector("[data-selector-sabores]");
    const botones = [...selectorSabores.querySelectorAll("[data-elegir]")];
    const botonAgregar = caja.querySelector("[data-agregar-caja]");
    const opciones = new Map([...selectores[0].options].filter(opcion => opcion.value).map(opcion => [opcion.value, opcion]));
    const cantidad = selectores.length;

    function actualizarCaja() {
        const elegidos = selectores.map(selector => selector.value).filter(Boolean);
        for (const boton of botones) {
            const valor = boton.dataset.elegir;
            const total = elegidos.filter(elegido => elegido === valor).length;
            const limite = puedeRepetir ? Number(opciones.get(valor).dataset.unidades) : 1;
            boton.classList.toggle("elegido", total > 0);
            boton.querySelector(".elegir-signo").textContent = total ? (puedeRepetir ? total : "✓") : "+";
            boton.querySelector("[data-seleccionadas]").textContent = total ? ` (${total} en tu caja)` : "";
            boton.setAttribute("aria-disabled", String(total >= limite || elegidos.length >= cantidad));
        }
        selectores.forEach((selector, indice) => {
            const hueco = bandeja.children[indice];
            const opcion = opciones.get(selector.value);
            const quitar = hueco.querySelector("[data-quitar]");
            if (hueco.dataset.valor === selector.value) return;
            hueco.dataset.valor = selector.value;
            hueco.classList.toggle("lleno", !!opcion);
            hueco.querySelector("[data-posicion]").hidden = !!opcion;
            hueco.querySelector("img")?.remove();
            quitar.hidden = !opcion;
            if (opcion) {
                quitar.setAttribute("aria-label", `Quitar ${opcion.textContent} de la posición ${indice + 1}`);
                quitar.title = `Quitar ${opcion.textContent}`;
                if (opcion.dataset.foto) {
                    const imagen = new Image();
                    imagen.alt = "";
                    imagen.src = opcion.dataset.foto;
                    hueco.prepend(imagen);
                } else {
                    hueco.querySelector("[data-posicion]").hidden = false;
                }
            }
        });
        botonAgregar.disabled = elegidos.length !== cantidad;
        progreso.textContent = elegidos.length === cantidad ? "Caja completa · 4 botellas" : `${elegidos.length} de ${cantidad} botellas`;
    }

    selectorSabores.addEventListener("click", evento => {
        const boton = evento.target.closest("[data-elegir]");
        if (!boton || boton.getAttribute("aria-disabled") === "true") return;
        const libre = selectores.find(selector => !selector.value);
        if (libre) {
            libre.value = boton.dataset.elegir;
            actualizarCaja();
        }
    });
    bandeja.addEventListener("click", evento => {
        const quitar = evento.target.closest("[data-quitar]");
        if (!quitar) return;
        const selector = selectores[Number(quitar.dataset.quitar) - 1];
        const valor = selector.value;
        selector.value = "";
        actualizarCaja();
        botones.find(boton => boton.dataset.elegir === valor)?.focus();
    });

    actualizarCaja();
    caja.querySelector("[data-seleccion-nativa]").hidden = true;
    selectorSabores.hidden = false;
    bandeja.hidden = false;
    progreso.hidden = false;
    window.addEventListener("pageshow", actualizarCaja);
}

const botonConfirmar = document.querySelector("[data-enviar]");
if (botonConfirmar) {
    const textoInicial = botonConfirmar.textContent;
    botonConfirmar.form.addEventListener("submit", () => {
        botonConfirmar.textContent = botonConfirmar.dataset.enviar;
        botonConfirmar.disabled = true;
    });
    window.addEventListener("pageshow", event => {
        if (event.persisted) {
            botonConfirmar.disabled = false;
            botonConfirmar.textContent = textoInicial;
        }
    });
}
