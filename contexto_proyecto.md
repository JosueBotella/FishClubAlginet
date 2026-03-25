# Contexto del Proyecto: FishClubAlginet

## Objetivo
Plataforma de gestión para un club local de pesca. El sistema debe permitir administrar socios (pescadores), organizar ligas anuales y gestionar competiciones específicas.

## Stack Tecnológico
- **Backend:** .NET 9 con Clean Architecture.
- **Persistencia:** Entity Framework Core con SQL Server.
- **Frontend:** Blazor WebAssembly.
- **Comunicación:** MediatR (patrón CQRS) en la capa de Application.
- **Autenticación:** ASP.NET Core Identity (Roles: Admin y Pescador).

## Reglas de Negocio Generales
- Solo los administradores pueden crear ligas y competiciones.
- Las ligas son anuales.
- Una competición pertenece a una única liga.
- Cada competición tiene un número limitado de "pesqueras" (puestos).