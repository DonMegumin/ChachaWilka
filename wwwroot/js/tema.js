(() => {
    let tema = 'claro';
    try { tema = localStorage.getItem('chachawillka-tema') || (matchMedia('(prefers-color-scheme: dark)').matches ? 'oscuro' : 'claro'); } catch { }
    document.documentElement.dataset.tema = tema;
    document.addEventListener('DOMContentLoaded', () => {
        const boton = document.querySelector('[data-tema]');
        if (!boton) return;
        const actualizar = () => {
            const oscuro = document.documentElement.dataset.tema === 'oscuro';
            boton.textContent = oscuro ? 'Modo claro' : 'Modo oscuro';
            boton.setAttribute('aria-pressed', String(oscuro));
        };
        actualizar();
        boton.addEventListener('click', () => {
            document.documentElement.dataset.tema = document.documentElement.dataset.tema === 'oscuro' ? 'claro' : 'oscuro';
            try { localStorage.setItem('chachawillka-tema', document.documentElement.dataset.tema); } catch { }
            actualizar();
        });
    });
})();
