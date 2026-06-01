---
description: notebooklm-aspnet-architecture
---

# PROTOCOLO DE ARQUITECTURA: INTEGRACIÓN DE CODEGRAPH Y NOTEBOOKLM

## OBJETIVO PRINCIPAL
Este workflow define cómo el agente debe integrar las directrices de arquitectura de ASP.NET Core provenientes del cuaderno de NotebookLM "FULL ASP.NET Core" (ID: `f2e3c464-f0c4-4168-85d2-8ab1299c5c2e`) con la exploración estructural del código provista por la herramienta `codegraph`. El propósito es garantizar que cualquier refactorización, diseño de nuevos componentes o cambios en la arquitectura del proyecto `FishClubAlginet` se alineen estrictamente con los estándares y mejores prácticas de ASP.NET Core definidos en el cuaderno.

## PROTOCOLO DE HERRAMIENTAS: CONSULTA DE NOTEBOOKLM
Ante cualquier duda de arquitectura, patrones de diseño o mejores prácticas en ASP.NET Core, el agente DEBE consultar el cuaderno de NotebookLM utilizando la herramienta `notebooklm-mcp-server`.

### Flujo de Consulta Estricto:
1. **Identificación de Concepto:** Identificar el patrón arquitectónico, API de ASP.NET Core o principio de diseño sobre el cual se tiene dudas (ej. Clean Architecture, inyección de dependencias, patrones de repositorio, configuración de middleware, etc.).
2. **Consulta a NotebookLM:** Utilizar la herramienta `notebook_query` con el ID de cuaderno `f2e3c464-f0c4-4168-85d2-8ab1299c5c2e` formulando una consulta precisa sobre el tema de arquitectura.
3. **Correlación con el Grafo de Código (`codegraph`):** Analizar el grafo de dependencias generado por `codegraph` en el proyecto actual y cruzarlo con las directrices devueltas por NotebookLM para evaluar la adherencia del proyecto a las prácticas óptimas de ASP.NET Core.

## CRITERIOS DE APLICACIÓN DE ARQUITECTURA
- **Patrones y Buenas Prácticas:** Asegurar que los controladores, servicios, repositorios y configuraciones de middleware sigan las especificaciones del cuaderno "FULL ASP.NET Core".
- **Evaluación de Impacto:** Al proponer un cambio, justificar arquitectónicamente la decisión basándose en el cuaderno de NotebookLM y validar la viabilidad estructural usando `codegraph`.
- **Mitigación de Deuda Técnica:** Utilizar el cuaderno para estructurar refactorizaciones que mejoren el desacoplamiento y sigan principios SOLID.

## FORMATO DE PROPUESTA DE ARQUITECTURA
Cada sugerencia de cambio arquitectónico o refactorización de gran escala debe documentar:
1. **Directriz de NotebookLM:** Citar textualmente o resumir la directriz de arquitectura obtenida del cuaderno "FULL ASP.NET Core".
2. **Análisis Estructural (Codegraph):** Mapear el estado actual en el grafo de código y cómo se alineará con la directriz.
3. **Implementación:** Código limpio que siga las directrices identificadas.
4. **Verificación de Grafo:** Confirmar que no se rompen dependencias clave del sistema según `codegraph`.
