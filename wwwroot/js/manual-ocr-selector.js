// Script para selección manual de áreas en imágenes para OCR
window.ManualOcrSelector = {
    canvas: null,
    ctx: null,
    image: null,
    areas: {},
    currentArea: null,
    isDrawing: false,
    startX: 0,
    startY: 0,
    scale: 1,
    currentField: null,
    
    // Campos disponibles para seleccionar
    fields: [
        { id: 'numero', label: 'Número de Factura', color: '#FF6B6B', required: true },
        { id: 'fecha', label: 'Fecha de Emisión', color: '#4ECDC4', required: true },
        { id: 'proveedor', label: 'Nombre Proveedor', color: '#45B7D1', required: true },
        { id: 'nif', label: 'NIF/CIF', color: '#96CEB4', required: false },
        { id: 'base', label: 'Base Imponible', color: '#FFEAA7', required: true },
        { id: 'iva', label: '% IVA', color: '#DFE6E9', required: false },
        { id: 'importeiva', label: 'Importe IVA', color: '#74B9FF', required: false },
        { id: 'total', label: 'Total', color: '#00B894', required: true },
        { id: 'fechavencimiento', label: 'Fecha Vencimiento', color: '#FDCB6E', required: false },
        { id: 'direccion', label: 'Dirección', color: '#E17055', required: false },
        { id: 'ciudad', label: 'Ciudad', color: '#A29BFE', required: false },
        { id: 'cp', label: 'Código Postal', color: '#FD79A8', required: false }
    ],

    // Inicializar el selector
    init: function(imageElement, canvasId) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) {
            console.error('Canvas no encontrado');
            return false;
        }

        this.ctx = this.canvas.getContext('2d');
        this.image = imageElement;
        
        // Configurar canvas con el tamaño de la imagen
        const maxWidth = this.canvas.parentElement.clientWidth - 20;
        const maxHeight = 600;
        
        this.scale = Math.min(
            maxWidth / this.image.naturalWidth,
            maxHeight / this.image.naturalHeight,
            1
        );
        
        this.canvas.width = this.image.naturalWidth * this.scale;
        this.canvas.height = this.image.naturalHeight * this.scale;
        
        // Dibujar imagen en canvas
        this.ctx.drawImage(this.image, 0, 0, this.canvas.width, this.canvas.height);
        
        // Event listeners
        this.canvas.addEventListener('mousedown', this.handleMouseDown.bind(this));
        this.canvas.addEventListener('mousemove', this.handleMouseMove.bind(this));
        this.canvas.addEventListener('mouseup', this.handleMouseUp.bind(this));
        
        // Touch events para móviles
        this.canvas.addEventListener('touchstart', this.handleTouchStart.bind(this));
        this.canvas.addEventListener('touchmove', this.handleTouchMove.bind(this));
        this.canvas.addEventListener('touchend', this.handleTouchEnd.bind(this));
        
        console.log('Selector inicializado correctamente');
        return true;
    },

    // Seleccionar campo activo para dibujar
    selectField: function(fieldId) {
        this.currentField = this.fields.find(f => f.id === fieldId);
        if (this.currentField) {
            console.log('Campo seleccionado:', this.currentField.label);
            // Actualizar cursor
            this.canvas.style.cursor = 'crosshair';
        }
    },

    // Eventos de mouse
    handleMouseDown: function(e) {
        if (!this.currentField) {
            alert('Selecciona primero un campo para marcar');
            return;
        }

        const rect = this.canvas.getBoundingClientRect();
        this.startX = e.clientX - rect.left;
        this.startY = e.clientY - rect.top;
        this.isDrawing = true;
    },

    handleMouseMove: function(e) {
        if (!this.isDrawing) return;

        const rect = this.canvas.getBoundingClientRect();
        const currentX = e.clientX - rect.left;
        const currentY = e.clientY - rect.top;

        // Redibujar todo
        this.redraw();

        // Dibujar rectángulo actual
        this.ctx.strokeStyle = this.currentField.color;
        this.ctx.lineWidth = 2;
        this.ctx.strokeRect(
            this.startX,
            this.startY,
            currentX - this.startX,
            currentY - this.startY
        );
    },

    handleMouseUp: function(e) {
        if (!this.isDrawing) return;

        const rect = this.canvas.getBoundingClientRect();
        const endX = e.clientX - rect.left;
        const endY = e.clientY - rect.top;

        // Validar que el área no sea demasiado pequeña
        const width = Math.abs(endX - this.startX);
        const height = Math.abs(endY - this.startY);

        if (width < 10 || height < 10) {
            alert('El área seleccionada es demasiado pequeña');
            this.isDrawing = false;
            this.redraw();
            return;
        }

        // Guardar área (convertir a coordenadas originales de la imagen)
        const area = {
            x: Math.round(Math.min(this.startX, endX) / this.scale),
            y: Math.round(Math.min(this.startY, endY) / this.scale),
            width: Math.round(width / this.scale),
            height: Math.round(height / this.scale),
            field: this.currentField.id,
            label: this.currentField.label,
            color: this.currentField.color
        };

        this.areas[this.currentField.id] = area;
        console.log('Área guardada:', area);

        this.isDrawing = false;
        this.redraw();

        // Actualizar UI
        this.updateFieldsList();
        
        // Auto-seleccionar siguiente campo requerido
        this.selectNextField();
    },

    // Eventos táctiles (móvil)
    handleTouchStart: function(e) {
        e.preventDefault();
        const touch = e.touches[0];
        const mouseEvent = new MouseEvent('mousedown', {
            clientX: touch.clientX,
            clientY: touch.clientY
        });
        this.canvas.dispatchEvent(mouseEvent);
    },

    handleTouchMove: function(e) {
        e.preventDefault();
        const touch = e.touches[0];
        const mouseEvent = new MouseEvent('mousemove', {
            clientX: touch.clientX,
            clientY: touch.clientY
        });
        this.canvas.dispatchEvent(mouseEvent);
    },

    handleTouchEnd: function(e) {
        e.preventDefault();
        const mouseEvent = new MouseEvent('mouseup', {});
        this.canvas.dispatchEvent(mouseEvent);
    },

    // Redibujar canvas
    redraw: function() {
        // Limpiar canvas
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
        // Redibujar imagen
        this.ctx.drawImage(this.image, 0, 0, this.canvas.width, this.canvas.height);
        
        // Redibujar áreas guardadas
        for (const [fieldId, area] of Object.entries(this.areas)) {
            const x = area.x * this.scale;
            const y = area.y * this.scale;
            const w = area.width * this.scale;
            const h = area.height * this.scale;

            // Rectángulo
            this.ctx.strokeStyle = area.color;
            this.ctx.lineWidth = 2;
            this.ctx.strokeRect(x, y, w, h);

            // Fondo semi-transparente para la etiqueta
            this.ctx.fillStyle = area.color;
            this.ctx.globalAlpha = 0.8;
            this.ctx.fillRect(x, y - 20, 150, 20);
            
            // Texto de la etiqueta
            this.ctx.globalAlpha = 1;
            this.ctx.fillStyle = '#FFFFFF';
            this.ctx.font = 'bold 12px Arial';
            this.ctx.fillText(area.label, x + 5, y - 6);
        }
    },

    // Actualizar lista de campos
    updateFieldsList: function() {
        const listElement = document.getElementById('selected-fields-list');
        if (!listElement) return;

        listElement.innerHTML = '';
        
        for (const [fieldId, area] of Object.entries(this.areas)) {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <span>
                    <span class="badge" style="background-color: ${area.color};">&nbsp;&nbsp;</span>
                    ${area.label}
                </span>
                <button class="btn btn-sm btn-outline-danger" onclick="ManualOcrSelector.removeArea('${fieldId}')">
                    <i class="bi bi-trash"></i>
                </button>
            `;
            listElement.appendChild(li);
        }
    },

    // Seleccionar siguiente campo requerido
    selectNextField: function() {
        const nextField = this.fields.find(f => 
            f.required && !this.areas.hasOwnProperty(f.id)
        );
        
        if (nextField) {
            this.selectField(nextField.id);
            
            // Actualizar UI del botón
            const button = document.querySelector(`button[data-field="${nextField.id}"]`);
            if (button) {
                button.classList.add('active');
                // Desactivar otros
                document.querySelectorAll('.field-button').forEach(btn => {
                    if (btn !== button) btn.classList.remove('active');
                });
            }
        } else {
            this.currentField = null;
            this.canvas.style.cursor = 'default';
        }
    },

    // Eliminar área
    removeArea: function(fieldId) {
        delete this.areas[fieldId];
        this.redraw();
        this.updateFieldsList();
    },

    // Limpiar todas las áreas
    clearAll: function() {
        if (confirm('¿Deseas eliminar todas las áreas seleccionadas?')) {
            this.areas = {};
            this.redraw();
            this.updateFieldsList();
        }
    },

    // Obtener áreas en formato para el servidor
    getAreas: function() {
        const areasForServer = {};
        for (const [fieldId, area] of Object.entries(this.areas)) {
            areasForServer[fieldId] = {
                x: area.x,
                y: area.y,
                width: area.width,
                height: area.height
            };
        }
        return areasForServer;
    },

    // Verificar si hay áreas requeridas seleccionadas
    hasRequiredFields: function() {
        const requiredFields = this.fields.filter(f => f.required);
        return requiredFields.every(field => this.areas.hasOwnProperty(field.id));
    },

    // Obtener campos faltantes
    getMissingFields: function() {
        const requiredFields = this.fields.filter(f => f.required);
        return requiredFields
            .filter(field => !this.areas.hasOwnProperty(field.id))
            .map(field => field.label);
    },

    // Inicializar toda la UI (canvas + botones)
    initializeUI: function(imageId, canvasId, buttonsContainerId) {
        console.log('Iniciando inicialización de UI...');
        
        const img = document.getElementById(imageId);
        const canvas = document.getElementById(canvasId);
        const container = document.getElementById(buttonsContainerId);
        
        if (!img || !canvas || !container) {
            console.error('Elementos no encontrados:', { img, canvas, container });
            return false;
        }

        // Función para inicializar cuando la imagen esté lista
        const initializeWhenReady = () => {
            console.log('Imagen lista, inicializando...');
            
            // Inicializar canvas
            const success = this.init(img, canvasId);
            if (!success) {
                console.error('Error al inicializar canvas');
                return false;
            }

            // Crear botones de campos
            container.innerHTML = '';
            this.fields.forEach(field => {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'btn btn-sm btn-outline-primary field-button w-100 text-start mb-1';
                btn.setAttribute('data-field', field.id);
                btn.innerHTML = `
                    <span class='badge me-2' style='background-color: ${field.color};'>&nbsp;&nbsp;</span>
                    ${field.label} ${field.required ? '<span class="text-danger">*</span>' : ''}
                `;
                btn.onclick = () => {
                    this.selectField(field.id);
                    document.querySelectorAll('.field-button').forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');
                };
                container.appendChild(btn);
            });

            console.log('Inicialización completada con éxito');
            return true;
        };

        // Si la imagen ya está cargada, inicializar inmediatamente
        if (img.complete && img.naturalHeight !== 0) {
            console.log('Imagen ya cargada');
            return initializeWhenReady();
        } else {
            // Esperar a que la imagen se cargue
            console.log('Esperando carga de imagen...');
            img.onload = () => {
                console.log('Evento onload disparado');
                initializeWhenReady();
            };
            img.onerror = () => {
                console.error('Error al cargar la imagen');
            };
            return true;
        }
    }
};
