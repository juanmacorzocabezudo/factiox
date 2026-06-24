// Mapa geográfico con Leaflet
window.mapInstances = window.mapInstances || {};
window.mapMarkers = window.mapMarkers || {};

window.initializeMap = function (elementId, locations) {
    try {
        // Limpiar mapa existente si lo hay para este elemento específico
        if (window.mapInstances[elementId]) {
            window.mapInstances[elementId].remove();
            delete window.mapInstances[elementId];
        }

        // Limpiar marcadores de este mapa específico
        window.mapMarkers[elementId] = [];

        // Verificar si hay ubicaciones
        if (!locations || locations.length === 0) {
            console.log('No hay ubicaciones para mostrar en el mapa');
            return;
        }

        // Crear el mapa centrado en España
        const map = L.map(elementId).setView([40.4168, -3.7038], 6);

        // Añadir capa de OpenStreetMap
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        }).addTo(map);

        // Geocodificar ubicaciones y añadir marcadores
        const bounds = [];
        let markersAdded = 0;

        locations.forEach((location, index) => {
            // Usar servicio de geocodificación de Nominatim (OpenStreetMap)
            const query = encodeURIComponent(`${location.ciudad}, ${location.provincia}, ${location.codigoPostal}, España`);
            
            fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${query}&limit=1`)
                .then(response => response.json())
                .then(data => {
                    if (data && data.length > 0) {
                        const lat = parseFloat(data[0].lat);
                        const lon = parseFloat(data[0].lon);

                        // Crear icono personalizado basado en el tipo
                        let iconColor;
                        let tipoTexto;
                        let icono;
                        
                        if (location.tipo === 'factura_venta') {
                            iconColor = '#4caf50'; // Verde
                            tipoTexto = '💰 Facturas Venta';
                            icono = '💰';
                        } else if (location.tipo === 'factura_compra') {
                            iconColor = '#d32f2f'; // Rojo
                            tipoTexto = '🧾 Facturas Compra';
                            icono = '🧾';
                        } else {
                            iconColor = '#f57c00'; // Naranja
                            tipoTexto = '📋 Presupuestos';
                            icono = '📋';
                        }
                        
                        const icon = L.divIcon({
                            className: 'custom-map-marker',
                            html: `<div style="background-color: ${iconColor}; width: 30px; height: 30px; border-radius: 50% 50% 50% 0; transform: rotate(-45deg); border: 2px solid white; box-shadow: 0 2px 5px rgba(0,0,0,0.3);"><div style="width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; transform: rotate(45deg); color: white; font-weight: bold; font-size: 10px;">${location.cantidad}</div></div>`,
                            iconSize: [30, 30],
                            iconAnchor: [15, 30],
                            popupAnchor: [0, -30]
                        });

                        // Añadir marcador
                        const marker = L.marker([lat, lon], { icon: icon }).addTo(map);
                        
                        // Añadir popup
                        const popupContent = `
                            <div style="min-width: 200px;">
                                <h6 style="margin: 0 0 8px 0; color: ${iconColor}; font-weight: bold;">
                                    ${tipoTexto}
                                </h6>
                                <div style="font-size: 14px;">
                                    <p style="margin: 4px 0;"><strong>${location.ciudad}</strong></p>
                                    <p style="margin: 4px 0; color: #666;">Provincia: ${location.provincia}</p>
                                    <p style="margin: 4px 0; color: #666;">CP: ${location.codigoPostal}</p>
                                    <hr style="margin: 8px 0;">
                                    <p style="margin: 4px 0;"><strong>Cantidad:</strong> ${location.cantidad}</p>
                                    <p style="margin: 4px 0;"><strong>Total:</strong> ${location.totalFormateado}</p>
                                </div>
                            </div>
                        `;
                        marker.bindPopup(popupContent);

                        window.mapMarkers[elementId].push(marker);
                        bounds.push([lat, lon]);
                        markersAdded++;

                        // Ajustar vista del mapa cuando se hayan añadido todos los marcadores
                        if (markersAdded === locations.length && bounds.length > 0) {
                            map.fitBounds(bounds, { padding: [50, 50] });
                        }
                    }
                })
                .catch(error => {
                    console.error('Error geocodificando ubicación:', location, error);
                });
        });

        window.mapInstances[elementId] = map;

        // Ajustar el tamaño del mapa después de un breve retraso
        setTimeout(() => {
            map.invalidateSize();
        }, 100);

        return true;
    } catch (error) {
        console.error('Error inicializando el mapa:', error);
        return false;
    }
};

// Función para actualizar el tamaño del mapa
window.resizeMap = function (elementId) {
    if (elementId && window.mapInstances[elementId]) {
        window.mapInstances[elementId].invalidateSize();
    } else {
        // Si no se especifica elementId, redimensionar todos los mapas
        Object.values(window.mapInstances).forEach(map => {
            map.invalidateSize();
        });
    }
};
