---
name: codegraph-analysis
description: Analiza flujos, dependencias, callers, callees e impacto de cambios en FishClubAlginet mediante CodeGraph. Úsala para explicar recorridos de código, revisar cambios estructurales o preparar refactorizaciones; no la uses para búsquedas triviales de texto.
---

# CodeGraph Analysis

Usa el grafo local como primera fuente para preguntas sobre relaciones entre símbolos o impacto transversal.

## Flujo

1. Comprueba que el repositorio contiene `.codegraph/` y que CodeGraph responde con `codegraph status` o mediante sus herramientas MCP.
2. Para exploración general, usa `codegraph_explore` o `codegraph explore "<pregunta>"`.
3. Para análisis preciso, consulta callers, callees, nodos e impacto. Abre después únicamente los archivos relevantes para confirmar comportamiento y condiciones de negocio.
4. Antes de cerrar una refactorización, ejecuta `codegraph sync` si el MCP no está observando cambios y vuelve a consultar el impacto de los símbolos modificados.

No trates el grafo como sustituto de los tests ni atribuyas relaciones que su salida no demuestre.

## Disponibilidad y fallback

Si las herramientas MCP no aparecen:

- Comprueba si `codegraph` está en `PATH`.
- Si la CLI existe, usa sus comandos equivalentes con salida JSON cuando resulte útil.
- Si tampoco existe la CLI, informa de la limitación y continúa con `rg`, lectura dirigida, compilación y tests. No bloquees una tarea ordinaria ni afirmes que se usó CodeGraph.

La instalación y configuración global modifican el entorno del usuario; hazlas solo cuando el usuario las solicite. La configuración MCP estándar ejecuta `codegraph serve --mcp`.
