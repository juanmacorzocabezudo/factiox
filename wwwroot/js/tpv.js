window.tpv = {
    _handler: null,

    registrarAtajos(dotnetRef) {
        this.desregistrarAtajos();
        const teclas = ['F2', 'F4', 'F8', 'Escape'];
        this._handler = (e) => {
            if (teclas.includes(e.key)) {
                e.preventDefault();
                dotnetRef.invokeMethodAsync('Atajo', e.key);
            }
        };
        document.addEventListener('keydown', this._handler);
    },

    desregistrarAtajos() {
        if (this._handler) document.removeEventListener('keydown', this._handler);
        this._handler = null;
    },

    enfocar(id) {
        const el = document.getElementById(id);
        if (el) {
            el.focus();
            if (el.select) el.select();
        }
    },

    obtener(clave) {
        return localStorage.getItem(clave);
    },

    guardar(clave, valor) {
        localStorage.setItem(clave, valor);
    }
};
