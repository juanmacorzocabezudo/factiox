// Script para registro de Service Worker y funcionalidades PWA
let swRegistration = null;

document.getElementById('install-banner')?.remove();
document.getElementById('pwa-install-styles')?.remove();

// Registrar Service Worker
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/service-worker.js')
            .then((registration) => {
                console.log('Service Worker registrado con éxito:', registration.scope);
                swRegistration = registration;

                // Verificar actualizaciones periódicamente
                setInterval(() => {
                    registration.update();
                }, 60000); // Cada minuto

                // Escuchar cambios en el Service Worker
                registration.addEventListener('updatefound', () => {
                    const newWorker = registration.installing;
                    console.log('Nueva versión del Service Worker encontrada');

                    newWorker.addEventListener('statechange', () => {
                        if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                            // Hay una nueva versión disponible
                            if (confirm('Nueva versión disponible. ¿Desea actualizar?')) {
                                newWorker.postMessage({ type: 'SKIP_WAITING' });
                                window.location.reload();
                            }
                        }
                    });
                });
            })
            .catch((error) => {
                console.error('Error al registrar Service Worker:', error);
            });

        // Recargar cuando el Service Worker tome el control
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            window.location.reload();
        });
    });
}

window.addEventListener('beforeinstallprompt', (event) => {
    event.preventDefault();
});

// Detectar cuando la app fue instalada
window.addEventListener('appinstalled', () => {
    console.log('FactioX ha sido instalada');
});

// Verificar si ya está instalada
function isAppInstalled() {
    // En Chrome/Edge
    if (window.matchMedia('(display-mode: standalone)').matches) {
        return true;
    }
    // En iOS Safari
    if (window.navigator.standalone === true) {
        return true;
    }
    return false;
}

// Función para solicitar permisos de notificaciones (opcional)
function requestNotificationPermission() {
    if ('Notification' in window && Notification.permission === 'default') {
        Notification.requestPermission().then((permission) => {
            console.log('Permiso de notificaciones:', permission);
        });
    }
}

// Exportar funciones para uso global
window.pwaInstall = {
    isInstalled: isAppInstalled,
    requestNotifications: requestNotificationPermission
};
