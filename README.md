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

### 1. Preparar la Base de Datos
Asegúrate de tener un servidor MariaDB activo. Genera la estructura de tablas ejecutando:
```powershell
dotnet ef database update