# FurnitureStore Management API 

Este proyecto es una **API REST** de grado profesional desarrollada con **ASP.NET Core** para la gestión integral de una tienda de muebles. El sistema implementa una arquitectura robusta centrada en la seguridad, el rendimiento asíncrono y la persistencia de datos relacionales en entornos de producción.

## 🚀 Características Principales

* **Seguridad Avanzada**: Implementación de **ASP.NET Core Identity** para la gestión de identidades.
* **Autenticación JWT**: Uso de Access Tokens para asegurar la comunicación entre cliente y servidor.
* **Rotación de Refresh Tokens**: Sistema para mantener sesiones seguras y persistentes, evitando el re-login constante del usuario.
* **Confirmación de Cuenta**: Flujo de verificación de correo electrónico integrado para validar nuevos registros.
* **Programación Asíncrona**: Uso de patrones `async/await` en todos los controladores para garantizar una API no bloqueante y escalable.
* **Persistencia en MariaDB**: Configuración optimizada para motores de base de datos relacionales de alto rendimiento.

## 🛠️ Stack Tecnológico

* **Framework**: .NET 8 (ASP.NET Core Web API).
* **ORM**: Entity Framework Core (Code First).
* **Base de Datos**: MariaDB / MySQL (mediante Pomelo Entity Framework Core).
* **Seguridad**: JWT (JSON Web Tokens) e Identity Core.
* **Documentación**: Swagger/OpenAPI con soporte para esquemas de seguridad Bearer.
* **Servicios de Email**: Integración con MailKit y protocolos SMTP.

## 🗃️ Modelo de Datos y Relaciones

La lógica de negocio se divide en módulos interconectados que aseguran la integridad referencial en MariaDB:

1.  **Catálogo de Productos**: Estructura jerárquica mediante categorías (`ProductCategory`) y productos (`Product`).
2.  **Gestión de Clientes**: Registro completo de información de contacto y perfiles de usuario (`Client`).
3.  **Sistema de Órdenes**: Relación maestro-detalle entre `Order` y `OrderDetail`, permitiendo gestionar múltiples productos por pedido con trazabilidad completa.
4.  **Control de Sesiones**: Tabla especializada de `RefreshTokens` vinculada a los usuarios para el control de acceso seguro.

## 🔧 Configuración Local

Sigue estos pasos para poner en marcha el proyecto:

### Preparar la Base de Datos
Asegúrate de tener un servidor MariaDB activo. Genera la estructura de tablas ejecutando:
- Para crear la structura correspondiente de la base de datos.
```powershell
add-migration
```

- Ejecuta la estructura y crea la base de datos
```powershell
database-update
```
Para ello, en este caso lo he hecho directamente en Visual Studio, usando la `Consola del Administrador de paquetes`.
![Ventana de Administrado de Paquetes](https://github.com/JuanMonta/FurnitureStore-Backend-API/blob/main/imgs/VentanaAdministradorDePaquetes.png?raw=true)

Asegúrate de elegir `API.FornitureStore.Data` para dar comienzo con la structura y creación de la base de datos.
![Consola de Administrado de Paquetes](https://github.com/JuanMonta/FurnitureStore-Backend-API/blob/main/imgs/ConsolaAdministradorDePaquetes.png?raw=true)

Una vez ejecutado exitosamente los comandos, verás como en tu `MariaDB` se ha creado automáticamente la base datos con toda su estructura. 

![MariaDb FurnitureStore](https://github.com/JuanMonta/FurnitureStore-Backend-API/blob/main/imgs/MariaDBFurnitureStore.png?raw=true)

## 📨 Configuración Local SMTP
Pone en marcha la verificacion de email.
* **Servidor SMTP**: Se recomienda usar [Papercut SMTP](https://papercut.codeplex.com/) para capturar los correos de verificación localmente, así no depender y configurar los servicios de correo reales que tengas.
* **Correos Temporales**: [Yopmail](https://yopmail.com) Utilizado para generar direcciones de correo rápidas y validar el flujo de confirmación de cuenta.
* **Configuración necesaria**: No olvides configurar lo necesario en `appsettings.json`, se recomienda encarecidamente que uses los `secrets`.

![Configuraciones de secrets](https://github.com/JuanMonta/FurnitureStore-Backend-API/blob/main/imgs/ConfSecrets.png?raw=true)

![Menu para acceder a secrets](https://github.com/JuanMonta/FurnitureStore-Backend-API/blob/main/imgs/MenuSecrets.png?raw=true)

### 🧪 Guía de Pruebas Rápidas
Para probar el sistema de autenticación sin usar correos reales:
1. Tener corriendo localmente [Papercut SMTP](https://papercut.codeplex.com/)
2. Genera un correo desechable en [YOPmail](https://yopmail.com).
3. Úsalo en el endpoint `/api/Authentication/Register`.
4. Ahora `Papercut SMTP` capturará interceptará toda salida SMTP y la mostrará en su bandeja, por lo que nunca te llegará a `YOPmail`, revisa la bandeja de entrada de papercut para obtener el enlace de confirmación generado por la API.
![SMT local](https://github.com/JuanMonta/FurnitureStore-Backend-API/blob/main/imgs/MailTest.png?raw=true)
5. Ahora en `/api/Authentication/Login` inicia sesión y copia el `token` que se generó y pégalo en swagger en el apartado de autorize, y podrás usar el resto de endpoints de la API. Si no te funciona, intenta primero anteponer la palabra `Bearer + [un espcio] + [tu token]`, ejemplo: `Bearer askljdkfljaslkdjakd`.
![Auth](https://github.com/JuanMonta/FurnitureStore-Backend-API/blob/main/imgs/MailTestAuth.png?raw=true)
