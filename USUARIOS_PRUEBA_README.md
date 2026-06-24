# 👤 Usuarios de Prueba con Fecha de Caducidad

## 📋 Descripción General

FactioX ahora permite al **SuperAdministrador** crear usuarios con **fecha de caducidad automática**. Esta funcionalidad es ideal para:

- 🎯 Ofrecer accesos de prueba temporales a clientes potenciales
- 📅 Crear demos con duración limitada
- 🔒 Gestionar accesos temporales sin necesidad de desactivar manualmente
- ⏰ Controlar automáticamente el vencimiento de accesos

## ✨ Características Principales

### 1. Control Automático de Acceso
- Los usuarios con fecha de caducidad **no pueden iniciar sesión** una vez que la fecha expira
- La validación se realiza automáticamente en cada intento de inicio de sesión
- No requiere intervención manual para desactivar usuarios caducados

### 2. Gestión Desde la Interfaz
Solo el **SuperAdministrador** puede:
- ✅ Marcar usuarios como "Acceso de Prueba"
- ✅ Establecer fechas de caducidad específicas
- ✅ Configurar días de prueba predefinidos (ej: 30 días)
- ✅ Ver el estado de caducidad en tiempo real
- ✅ Extender o modificar fechas de caducidad

### 3. Indicadores Visuales
La interfaz muestra claramente:
- 🟢 **Verde**: Usuarios con más de 7 días restantes
- 🟡 **Amarillo**: Usuarios que caducan en menos de 7 días
- 🔴 **Rojo**: Usuarios ya caducados
- ⏱️ Contador de días restantes

## 🚀 Cómo Usar

### Crear un Usuario de Prueba

1. **Acceder a Gestión de Usuarios**
   - Ir a la sección "Usuarios"
   - Hacer clic en "Nuevo Usuario"

2. **Configurar como Acceso de Prueba**
   - Completar los datos básicos del usuario
   - En la sección "Permisos y Estado", marcar la opción: ☑️ **Acceso de Prueba (Temporal)**

3. **Establecer Duración**
   
   **Opción A - Días predefinidos:**
   - Ingresar el número de días (ej: 30)
   - Hacer clic en "Aplicar"
   - La fecha de caducidad se calculará automáticamente

   **Opción B - Fecha específica:**
   - Seleccionar directamente la fecha de caducidad del calendario

4. **Guardar**
   - Hacer clic en "Crear" para guardar el usuario
   - El usuario podrá acceder hasta la fecha de caducidad establecida

### Editar la Fecha de Caducidad

1. Abrir el usuario existente en modo edición
2. Si no está marcado como "Acceso de Prueba", activar la opción
3. Modificar la fecha de caducidad según necesites
4. Guardar cambios

### Convertir a Usuario Permanente

Para eliminar la fecha de caducidad de un usuario:
1. Editar el usuario
2. Desmarcar la opción "Acceso de Prueba"
3. Guardar cambios
4. La fecha de caducidad se eliminará automáticamente

## 📊 Visualización en la Lista de Usuarios

En la tabla de usuarios, el SuperAdministrador verá una nueva columna **"Caducidad"** que muestra:

| Estado | Indicador | Significado |
|--------|-----------|-------------|
| 🟢 Activo | `30 días` | Usuario con más de 7 días restantes |
| 🟡 Caduca pronto | `5 días` | Usuario con 7 días o menos |
| 🔴 Caducado | `CADUCADO` | Usuario que ya no puede acceder |
| — | `—` | Usuario permanente (sin caducidad) |

## 🔒 Validación de Acceso

Cuando un usuario intenta iniciar sesión:

```
1. Se validan credenciales (usuario/contraseña) ✓
2. Se verifica si el usuario está activo ✓
3. Si tiene fecha de caducidad:
   - Si FechaCaducidad >= FechaActual → ✅ Permite acceso
   - Si FechaCaducidad < FechaActual → ❌ Deniega acceso
4. Si NO tiene fecha de caducidad → ✅ Permite acceso (usuario permanente)
```

### Mensajes de Error

Si un usuario caducado intenta entrar:
- **Mensaje mostrado**: "Usuario o contraseña incorrectos, o la empresa está desactivada"
- Por seguridad, no se revela que el usuario está caducado

## 💾 Base de Datos

### Campos Agregados al Modelo Usuario

```csharp
public DateTime? FechaCaducidad { get; set; }  // Fecha límite de acceso (nullable)
public bool EsAccesoPrueba { get; set; }        // Indica si es temporal
```

### Migración Aplicada

```bash
Migración: 20260310130512_AddFechaCaducidadUsuarios
Estado: ✅ Aplicada
```

### Scripts SQL Disponibles

En `/Database/ConfigurarUsuariosPrueba.sql` encontrarás scripts para:
- ✅ Configurar usuarios existentes como prueba
- ✅ Crear nuevos usuarios de prueba
- ✅ Consultar usuarios por caducar
- ✅ Extender fechas de caducidad masivamente
- ✅ Desactivar usuarios caducados automáticamente

## 📝 Ejemplos de Uso

### Ejemplo 1: Demo de 7 Días
```
Cliente: "Queremos probar FactioX"
Acción: Crear usuario con 7 días de prueba
Resultado: Automáticamente caduca después de 7 días
```

### Ejemplo 2: Acceso Temporal para Formación
```
Situación: Curso de formación de 15 días
Acción: Crear usuarios con 15 días de caducidad
Resultado: Los usuarios no podrán acceder después del curso
```

### Ejemplo 3: Extender Prueba
```
Cliente: "Necesitamos 15 días más"
Acción: Editar usuario y agregar 15 días a la fecha
Resultado: Acceso extendido automáticamente
```

### Ejemplo 4: Convertir a Cliente Permanente
```
Cliente: "Queremos contratar FactioX"
Acción: Desmarcar "Acceso de Prueba"
Resultado: Usuario sin límite de tiempo
```

## 🔧 Mantenimiento

### Revisar Usuarios Próximos a Caducar

En la interfaz de usuarios, los usuarios con menos de 7 días aparecen en **amarillo**.

### Limpiar Usuarios Caducados

Opción 1 - **Desactivar** (recomendado):
```sql
UPDATE Usuarios 
SET Activo = FALSE
WHERE EsAccesoPrueba = TRUE 
  AND FechaCaducidad < NOW();
```

Opción 2 - **Eliminar** (cuidado):
```sql
-- Solo usuarios caducados hace más de 30 días
DELETE FROM Usuarios
WHERE EsAccesoPrueba = TRUE 
  AND FechaCaducidad < DATE_SUB(NOW(), INTERVAL 30 DAY);
```

## ⚠️ Consideraciones Importantes

### Seguridad
- ✅ Solo el SuperAdministrador puede gestionar usuarios de prueba
- ✅ La validación de caducidad es automática y no puede evitarse
- ✅ No se revela al usuario que su cuenta está caducada (seguridad)

### Buenas Prácticas
- 📅 Establece fechas realistas de caducidad
- 📧 Notifica al cliente antes de que caduque (manualmente)
- 🔄 Revisa periódicamente usuarios próximos a caducar
- 💾 Considera exportar datos antes de eliminar usuarios caducados

### Limitaciones
- ⚠️ Los usuarios regulares no pueden ver información de caducidad
- ⚠️ No hay notificaciones automáticas de caducidad próxima
- ⚠️ La fecha de caducidad se verifica solo al iniciar sesión

## 🎯 Casos de Uso Recomendados

| Escenario | Días Sugeridos |
|-----------|----------------|
| Demo rápida | 7 días |
| Prueba estándar | 15-30 días |
| Evaluación extendida | 60 días |
| Formación/Curso | Duración del curso |
| Acceso temporal contratista | Duración del proyecto |

## 📞 Preguntas Frecuentes

**P: ¿Qué pasa con los datos del usuario cuando caduca?**  
R: Los datos se mantienen. El usuario simplemente no puede iniciar sesión. Puedes reactivarlo extendiendo la fecha.

**P: ¿Puede un usuario de prueba convertirse en permanente?**  
R: Sí, simplemente desmarca "Acceso de Prueba" al editarlo.

**P: ¿Se notifica al usuario antes de caducar?**  
R: No automáticamente. Debes notificarle manualmente antes de la fecha.

**P: ¿Qué pasa si no marco "Acceso de Prueba"?**  
R: El usuario será permanente y nunca caducará.

**P: ¿Puede un usuario ver su propia fecha de caducidad?**  
R: No, solo el SuperAdministrador puede ver esta información.

---

✅ **Funcionalidad implementada el:** 10 de marzo de 2026  
📝 **Versión:** 1.0  
👤 **Disponible para:** SuperAdministrador únicamente
