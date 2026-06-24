// Service Worker para FactioX PWA
const CACHE_NAME = 'factiox-v1.0';
const STATIC_CACHE = 'factiox-static-v1.0';
const DYNAMIC_CACHE = 'factiox-dynamic-v1.0';

// Archivos estáticos que se cachean al instalar
const STATIC_ASSETS = [
    '/',
    '/app.css',
    '/manifest.json',
    '/LogoFactioxFIN.png',
    '/FacioX_FIN.png',
    '/bootstrap/bootstrap.min.css',
    '/_framework/blazor.web.js'
];

// Instalación del Service Worker
self.addEventListener('install', (event) => {
    console.log('Service Worker: Instalando...');
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then((cache) => {
                console.log('Service Worker: Cacheando archivos estáticos');
                return cache.addAll(STATIC_ASSETS.map(url => new Request(url, { cache: 'reload' })));
            })
            .catch((error) => {
                console.error('Error al cachear archivos estáticos:', error);
            })
    );
    self.skipWaiting();
});

// Activación del Service Worker
self.addEventListener('activate', (event) => {
    console.log('Service Worker: Activando...');
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (cacheName !== STATIC_CACHE && cacheName !== DYNAMIC_CACHE) {
                        console.log('Service Worker: Eliminando caché antigua:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
    return self.clients.claim();
});

// Estrategia de caché: Network First con fallback a caché
self.addEventListener('fetch', (event) => {
    const { request } = event;
    
    // Ignorar solicitudes de non-GET
    if (request.method !== 'GET') {
        return;
    }

    // Ignorar solicitudes de SignalR/Blazor
    if (request.url.includes('_blazor') || 
        request.url.includes('signalr') ||
        request.url.includes('/_framework/') && request.url.includes('.hot-reload')) {
        return;
    }

    event.respondWith(
        // Intentar primero la red
        fetch(request)
            .then((response) => {
                // Si la respuesta es válida, guardar en caché dinámica
                if (response && response.status === 200) {
                    const responseClone = response.clone();
                    
                    // Solo cachear recursos GET exitosos
                    if (request.url.startsWith('http')) {
                        caches.open(DYNAMIC_CACHE).then((cache) => {
                            cache.put(request, responseClone);
                        });
                    }
                }
                return response;
            })
            .catch(() => {
                // Si la red falla, intentar obtener de caché
                return caches.match(request).then((cachedResponse) => {
                    if (cachedResponse) {
                        return cachedResponse;
                    }
                    
                    // Si tampoco está en caché, mostrar página offline
                    if (request.destination === 'document') {
                        return caches.match('/');
                    }
                    
                    // Para otros recursos, retornar respuesta vacía
                    return new Response('Recurso no disponible sin conexión', {
                        status: 503,
                        statusText: 'Service Unavailable',
                        headers: new Headers({
                            'Content-Type': 'text/plain'
                        })
                    });
                });
            })
    );
});

// Manejo de mensajes del cliente
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
    
    if (event.data && event.data.type === 'CLEAR_CACHE') {
        event.waitUntil(
            caches.keys().then((cacheNames) => {
                return Promise.all(
                    cacheNames.map((cacheName) => caches.delete(cacheName))
                );
            })
        );
    }
});

// Sincronización en segundo plano (opcional)
self.addEventListener('sync', (event) => {
    if (event.tag === 'sync-facturas') {
        event.waitUntil(syncFacturas());
    }
});

async function syncFacturas() {
    // Aquí puedes implementar lógica de sincronización
    console.log('Sincronizando facturas...');
}

// Push notifications (opcional para futuras funcionalidades)
self.addEventListener('push', (event) => {
    const options = {
        body: event.data ? event.data.text() : 'Nueva notificación de FactioX',
        icon: '/LogoFactioxFIN.png',
        badge: '/LogoFactioxFIN.png',
        vibrate: [200, 100, 200],
        tag: 'factiox-notification',
        requireInteraction: false
    };

    event.waitUntil(
        self.registration.showNotification('FactioX', options)
    );
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    event.waitUntil(
        clients.openWindow('/')
    );
});
