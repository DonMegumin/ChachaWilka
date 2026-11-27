const caja = document.querySelector("[data-combo]");
if (caja) {
    const selectores = [...caja.querySelectorAll("select")];
    const progreso = caja.querySelector("[data-progreso]");
    const puedeRepetir = caja.dataset.repeticiones === "true";

    function actualizarCaja() {
        const elegidos = selectores.map(selector => selector.value).filter(Boolean);
        for (const selector of selectores) {
            for (const opcion of selector.options) {
                opcion.disabled = !puedeRepetir && opcion.value !== "" &&
                    opcion.value !== selector.value && elegidos.includes(opcion.value);
            }
        }
        progreso.textContent = `${elegidos.length} de 4 sabores seleccionados`;
    }

    caja.addEventListener("change", actualizarCaja);
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
