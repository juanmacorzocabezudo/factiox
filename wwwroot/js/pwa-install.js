// Script para registro de Service Worker y funcionalidades PWA
let deferredPrompt;
let swRegistration = null;

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

// Capturar el evento beforeinstallprompt
window.addEventListener('beforeinstallprompt', (e) => {
    console.log('beforeinstallprompt event capturado');
    // Prevenir el mini-infobar de Chrome en móvil
    e.preventDefault();
    // Guardar el evento para usarlo después
    deferredPrompt = e;
    
    // Mostrar botón de instalación personalizado
    showInstallPromotion();
});

// Función para mostrar la promoción de instalación
function showInstallPromotion() {
    // Crear un banner de instalación personalizado
    const installBanner = document.createElement('div');
    installBanner.id = 'install-banner';
    installBanner.innerHTML = `
        <div style="position: fixed; bottom: 20px; right: 20px; background: #1a3a1a; color: white; padding: 15px 20px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.3); z-index: 9999; max-width: 350px; display: flex; align-items: center; gap: 15px; animation: slideIn 0.3s ease-out;">
            <img src="/LogoFactioxFIN.png" alt="FactioX" style="width: 40px; height: 40px; border-radius: 8px;">
            <div style="flex: 1;">
                <div style="font-weight: bold; margin-bottom: 5px;">Instalar FactioX</div>
                <div style="font-size: 0.85em; opacity: 0.9;">Instala la aplicación para acceso rápido</div>
            </div>
            <button id="install-button" style="background: #4caf50; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer; font-weight: bold; white-space: nowrap;">
                Instalar
            </button>
            <button id="close-install-banner" style="background: transparent; color: white; border: none; padding: 4px; cursor: pointer; font-size: 1.2em; opacity: 0.7;">
                ×
            </button>
        </div>
    `;

    // Agregar estilos de animación
    if (!document.getElementById('pwa-install-styles')) {
        const style = document.createElement('style');
        style.id = 'pwa-install-styles';
        style.textContent = `
            @keyframes slideIn {
                from {
                    transform: translateX(400px);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
            #install-button:hover {
                background: #45a049 !important;
            }
            #close-install-banner:hover {
                opacity: 1 !important;
            }
            @media (max-width: 576px) {
                #install-banner > div {
                    bottom: 10px !important;
                    right: 10px !important;
                    left: 10px !important;
                    max-width: none !important;
                }
            }
        `;
        document.head.appendChild(style);
    }

    document.body.appendChild(installBanner);

    // Manejar clic en el botón de instalación
    document.getElementById('install-button').addEventListener('click', installApp);

    // Manejar clic en el botón de cerrar
    document.getElementById('close-install-banner').addEventListener('click', () => {
        installBanner.remove();
        // Guardar en localStorage que el usuario cerró el banner
        localStorage.setItem('install-banner-dismissed', Date.now());
    });
}

// Función para instalar la aplicación
async function installApp() {
    if (!deferredPrompt) {
        console.log('deferredPrompt no está disponible');
        return;
    }

    // Mostrar el prompt de instalación
    deferredPrompt.prompt();

    // Esperar a que el usuario responda
    const { outcome } = await deferredPrompt.userChoice;
    console.log(`User response to the install prompt: ${outcome}`);

    // Ocultar el banner
    const banner = document.getElementById('install-banner');
    if (banner) {
        banner.remove();
    }

    // Limpiar el deferredPrompt
    deferredPrompt = null;
}

// Detectar cuando la app fue instalada
window.addEventListener('appinstalled', () => {
    console.log('FactioX ha sido instalada');
    deferredPrompt = null;
    
    // Mostrar notificación de éxito
    if ('Notification' in window && Notification.permission === 'granted') {
        new Notification('¡FactioX instalada!', {
            body: 'La aplicación se ha instalado correctamente',
            icon: '/LogoFactioxFIN.png'
        });
    }
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

// Si la app ya está instalada, no mostrar el banner
if (isAppInstalled()) {
    console.log('La app ya está instalada');
} else {
    // Verificar si el banner fue cerrado recientemente (últimas 24 horas)
    const dismissedTime = localStorage.getItem('install-banner-dismissed');
    if (dismissedTime) {
        const hoursSinceDismissed = (Date.now() - parseInt(dismissedTime)) / (1000 * 60 * 60);
        if (hoursSinceDismissed < 24) {
            console.log('Banner de instalación cerrado recientemente');
        }
    }
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
    install: installApp,
    isInstalled: isAppInstalled,
    requestNotifications: requestNotificationPermission
};
