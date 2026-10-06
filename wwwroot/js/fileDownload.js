// Función para descargar archivos desde el navegador
window.downloadFile = function (filename, base64Content, contentType) {
    // Convertir el contenido base64 a un array de bytes
    const byteCharacters = atob(base64Content);
    const byteNumbers = new Array(byteCharacters.length);
    
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });
    
    // Crear un enlace temporal y hacer clic en él
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Limpiar el objeto URL
    window.URL.revokeObjectURL(link.href);
};

window.printPdfFromStream = async function (streamReference) {
    const buffer = await streamReference.arrayBuffer();
    const blob = new Blob([buffer], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
    const iframe = document.createElement('iframe');
    iframe.style.position = 'fixed';
    iframe.style.width = '0';
    iframe.style.height = '0';
    iframe.style.border = 'none';
    iframe.title = 'Facturas de venta para imprimir';

    const cleanup = () => {
        iframe.remove();
        window.URL.revokeObjectURL(url);
    };

    return new Promise((resolve, reject) => {
        iframe.onerror = () => {
            cleanup();
            reject(new Error('No se pudo cargar el PDF para imprimir.'));
        };
        iframe.onload = () => {
            setTimeout(() => {
                try {
                    iframe.contentWindow.addEventListener('afterprint', cleanup, { once: true });
                    iframe.contentWindow.focus();
                    iframe.contentWindow.print();
                    resolve();
                } catch (error) {
                    cleanup();
                    reject(error);
                }
            }, 250);
        };
        iframe.src = url;
        document.body.appendChild(iframe);
    });
};

window.downloadBinaryFile = async function (filename, streamReference, contentType) {
    const buffer = await streamReference.arrayBuffer();
    const blob = new Blob([buffer], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    try {
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.click();
    } finally {
        window.URL.revokeObjectURL(url);
    }
};

// Función para descargar archivos de texto directamente
window.descargarArchivo = function (filename, textContent, contentType) {
    const blob = new Blob([textContent], { type: contentType });
    
    // Crear un enlace temporal y hacer clic en él
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Limpiar el objeto URL
    window.URL.revokeObjectURL(link.href);
};
