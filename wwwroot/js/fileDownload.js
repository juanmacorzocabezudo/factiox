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
