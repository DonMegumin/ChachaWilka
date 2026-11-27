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

// El enlace y los formularios conservan su navegación normal sin JavaScript.
const bolsa = document.querySelector("[data-bolsa]");
const enlaceBolsa = document.querySelector("[data-bolsa-url]");
if (bolsa && enlaceBolsa && typeof bolsa.showModal === "function") {
    const contenido = bolsa.querySelector("[data-bolsa-contenido]");
    const aviso = bolsa.querySelector(".bolsa-aviso");
    let enviando = false;
    let lectura;
    enlaceBolsa.setAttribute("aria-haspopup", "dialog");

    function mostrarBolsa(mensaje) {
        aviso.textContent = mensaje;
        contenido.replaceChildren();
        if (!bolsa.open) bolsa.showModal();
    }
    function actualizarBolsa(html, mensaje) {
        const doc = new DOMParser().parseFromString(html, "text/html");
        const resumen = doc.querySelector("[data-total-botellas]");
        if (!resumen) throw new Error("Resumen no disponible");
        contenido.replaceChildren(resumen);
        enlaceBolsa.querySelector("span").textContent = resumen.dataset.totalBotellas;
        aviso.textContent = mensaje;
    }
    enlaceBolsa.addEventListener("click", async evento => {
        if (evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey) return;
        evento.preventDefault();
        if (enviando) return;
        lectura?.abort();
        const solicitud = new AbortController();
        lectura = solicitud;
        mostrarBolsa("Consultando tu cesta…");
        try {
            const respuesta = await fetch(enlaceBolsa.dataset.bolsaUrl, { signal: solicitud.signal, cache: "no-store" });
            if (!respuesta.ok) throw new Error();
            actualizarBolsa(await respuesta.text(), "");
        } catch {
            if (!solicitud.signal.aborted) aviso.textContent = "No pudimos cargar el resumen. Abre la página de la cesta para revisarla.";
        }
    });
    bolsa.addEventListener("close", () => lectura?.abort());
    bolsa.addEventListener("click", evento => {
        const limites = bolsa.getBoundingClientRect();
        if (evento.target === bolsa && (evento.clientX < limites.left || evento.clientY < limites.top || evento.clientX > limites.right || evento.clientY > limites.bottom)) bolsa.close();
    });
    document.addEventListener("submit", async evento => {
        const form = evento.target;
        if (!form.matches(".producto-acciones form")) return;
        evento.preventDefault();
        if (enviando) return;
        enviando = true;
        lectura?.abort();
        const datos = new FormData(form);
        datos.set("vistaRapida", "true");
        form.closest("dialog")?.close();
        mostrarBolsa("Añadiendo a tu cesta…");
        try {
            const respuesta = await fetch(form.action, { method: "POST", body: datos });
            if (respuesta.status === 409) {
                aviso.textContent = (await respuesta.json()).mensaje;
            } else {
                if (!respuesta.ok) throw new Error();
                actualizarBolsa(await respuesta.text(), "Añadido a tu cesta.");
            }
        } catch {
            aviso.textContent = "Revisa tu cesta antes de volver a añadir la botella.";
        } finally {
            enviando = false;
        }
    });
}
for (const enlace of document.querySelectorAll(".nav-personal a")) {
    if (new URL(enlace.href).pathname === location.pathname) enlace.setAttribute("aria-current", "page");
}
