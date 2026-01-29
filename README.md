# 🎣 Sistema de Gestión de Competiciones de Pesca

Una plataforma robusta diseñada para la gestión integral de federaciones y clubes de pesca. Este proyecto implementa una arquitectura **Modular Monolith** basada en principios de **Clean Architecture**, asegurando mantenibilidad, escalabilidad y un alto rendimiento.

El sistema permite la administración de pescadores, licencias federativas, creación de ligas anuales y gestión de torneos, con una separación clara entre el panel de administración y el portal del pescador.

-----

## 🚀 Características Principales

### Arquitectura y Backend (.NET 8)
* **Clean Architecture:** Separación estricta de responsabilidades (Domain, Application, Infrastructure, API).
* **Seguridad Avanzada:** Sistema de Autenticación y Autorización basado en **ASP.NET Core Identity** y **JWT (JSON Web Tokens)**.
* **Roles y Permisos:** Gestión diferenciada para Administradores (Gestión total) y Pescadores (Lectura de perfil/rankings).
* **Patrones de Diseño:** Implementación de *Generic Repository*, *Unit of Work* y *Specification Pattern*.
* **Modelado de Dominio Rico:** Uso de Entidades, *Value Objects* (ej. Direcciones) y validaciones de dominio.
* **Data Seeding:** Inicialización automática de datos maestros (ej. Municipios de Valencia) y usuarios administrador.

### Frontend (Blazor WebAssembly)
* **SPA Reactiva:** Interfaz de usuario construida con **Blazor WebAssembly** para una experiencia fluida.
* **Componentes UI:** Utiliza la librería **Radzen** para grids, diálogos y notificaciones visuales.
* **Arquitectura de UI:** Diseño de "Muro de Login", Layouts diferenciados y gestión de estado.
* **Optimización:** Carga diferida de datos y gestión eficiente de llamadas a API.

-----

## 📂 Estructura del Proyecto

La solución sigue la Regla de Dependencia, dividida en capas concéntricas:

### 1. FishClubAlginet.Core (Dominio)
*El núcleo del sistema. Contiene la lógica empresarial pura.*
* **Entidades:** `Pescador`, `Liga`, `Torneo`, `IdentityUser`.
* **Value Objects:** `Address` (Manejo de direcciones sin tablas redundantes).
* **Interfaces:** Contratos para Repositorios, Servicios de Auth y Unit of Work.
* **DTOs:** Objetos de transferencia de datos para Login, Registro y Vistas.

### 2. FishClubAlginet.Infrastructure (Infraestructura)
*La capa de implementación. Conecta con el mundo exterior.*
* **Persistencia:** `AppDbContext`, configuraciones de EF Core y Migraciones.
* **Identidad:** Implementación de servicios de autenticación y generación de Tokens.
* **Seeds:** Carga automática de datos iniciales (Usuarios Admin, Municipios).

### 3. FishClubAlginet.API (Presentación - Backend)
*El punto de entrada de datos.*
* **Controllers:** Endpoints RESTful protegidos por atributos `[Authorize]`.
* **Configuración:** Inyección de dependencias y configuración de CORS para Blazor.

### 4. FishClubAlginet.Blazor (Presentación - Frontend)
*La aplicación cliente.*
* **Pages:** Vistas organizadas por módulos (`Auth`, `Torneos`, `Usuarios`).
* **Services:** Servicios HTTP tipados para comunicar con la API.

-----

## 🛠️ Primeros Pasos

### Requisitos Previos
* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* [SQL Server](https://www.microsoft.com/sql-server/) (LocalDB o Express)
* Visual Studio 2022 o VS Code

### Instalación y Ejecución

1.  **Clonar el repositorio:**
    ```bash
    git clone [https://github.com/tu-usuario/ProyectoPesca.git](https://github.com/tu-usuario/ProyectoPesca.git)
    ```

2.  **Configurar Base de Datos:**
    Asegúrate de que la cadena de conexión en `FishClubAlginet.API/appsettings.Development.json` apunta a tu instancia local de SQL Server.

3.  **Aplicar Migraciones y Seeds:**
    Al ejecutar la API por primera vez, el `DbInitializer` creará la base de datos y cargará los datos iniciales automáticamente. Si necesitas hacerlo manual:
    ```bash
    dotnet ef database update --project FishClubAlginet.Infrastructure --startup-project FishClubAlginet.API
    ```

4.  **Ejecutar la Solución:**
    Se recomienda configurar la solución para iniciar múltiples proyectos (API + Blazor) o ejecutarlos por terminal:

    * **Backend (API):** `https://localhost:7179`
    * **Frontend (Blazor):** `https://localhost:7234` (o el puerto configurado)

    Navega a la URL del Frontend para iniciar sesión.

-----

## 📄 Licencia

Derechos de autor © 2026 [Tu Nombre]. Todos los derechos reservados.

Este proyecto es propiedad intelectual de su creador. No se permite la reproducción, distribución o uso comercial del código fuente sin autorización expresa.
