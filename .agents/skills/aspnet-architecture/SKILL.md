---
name: aspnet-architecture
description: Evalúa decisiones arquitectónicas relevantes de ASP.NET Core en FishClubAlginet combinando evidencia del código, CodeGraph y, cuando esté conectado, el cuaderno FULL ASP.NET Core de NotebookLM.
---

# ASP.NET Architecture

Usa esta skill para cambios arquitectónicos, no para implementaciones locales rutinarias.

1. Define la decisión y los requisitos de negocio que la motivan.
2. Mapea las dependencias actuales mediante `codegraph-analysis` cuando esté disponible y confirma los puntos críticos en el código.
3. Si existe la integración de NotebookLM, consulta el cuaderno `FULL ASP.NET Core` con ID `f2e3c464-f0c4-4168-85d2-8ab1299c5c2e` sobre la cuestión concreta.
4. Contrasta esa orientación con documentación oficial de Microsoft cuando la exactitud dependa de una API o versión actual.
5. Expón decisión, alternativas, impacto y forma de verificación.

NotebookLM es una fuente de apoyo, no un requisito para continuar. Si no está conectado, indícalo y basa la decisión en el código y fuentes primarias. No inventes consultas ni resultados del cuaderno.
