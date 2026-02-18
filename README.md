# BurgiPlot – Sistema Integral para PyMEs (MVP)

Aplicación web desarrollada en **ASP.NET Core 8 (MVC)** como **MVP funcional** de un sistema integral de gestión para pequeñas empresas, orientado a la **digitalización progresiva de procesos administrativos y comerciales**.

El proyecto surge a partir de un **caso real**: *BürgiPlot*, un emprendimiento local de gráfica y estética vehicular de la ciudad de Rafaela (Santa Fe, Argentina), que operaba de forma mayormente manual y sin sistemas centralizados.

> 📌 Estado actual: **MVP – Fase 1**
>  
> Implementa Landing Page, Portal institucional público y un Sistema privado con **ABM completo de Clientes**.

---

## 🎯 Objetivo del proyecto

Desarrollar un sistema web **modular, escalable y reutilizable**, pensado para PyMEs, que permita:

- Formalizar el registro de clientes
- Centralizar información hoy dispersa (WhatsApp, Excel, notas)
- Sentar bases técnicas sólidas para futuras funcionalidades:
  - Órdenes de trabajo
  - Stock
  - Facturación
  - Reportes e indicadores (KPIs)

El enfoque fue **incremental**, priorizando entregar valor temprano mediante un MVP funcional antes que un sistema “todo en uno” incompleto.

---

## 🧩 Alcance del MVP (Fase 1)

### ✔ Landing Page (pública)
- Página institucional orientada a **presentar la solución a otras PyMEs**
- Secciones tipo hero + beneficios
- Botón de contacto directo por WhatsApp
- Diseño responsive con Bootstrap 5

### ✔ Portal institucional público
- Página institucional del negocio
- Secciones:
  - Misión
  - Visión
  - Valores
  - Objetivos
  - Trabajos realizados
- Botón **“Ingresar”** al sistema de gestión (sin login en esta fase)

### ✔ Sistema privado – Gestión de Clientes (ABM)
CRUD completo de la entidad **Cliente**:

- Crear cliente
- Editar cliente
- Eliminar cliente (con confirmación)
- Listar clientes
- Ver detalles

**Validaciones implementadas:**
- Campos obligatorios: Nombre, Teléfono, DNI
- Formato de correo electrónico (si se ingresa)
- DNI único en base de datos
- Validación de longitud de DNI y CUIT/CUIL mediante **Trigger en SQL Server**
- Manejo de errores desde controlador (SqlException)

> ⚠️ En el MVP no hay autenticación ni control de roles (planificado para Fase 2).

---

## 🛠️ Stack tecnológico

**Backend**
- C#
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core 8 (database-first)

**Base de datos**
- SQL Server Express 2022
- Triggers para validaciones de integridad

**Frontend**
- Razor Views
- HTML5 / CSS3
- JavaScript (ES6)
- Bootstrap 5

**Herramientas**
- Visual Studio 2022
- Git / GitHub
- IIS Express (entorno local)

---

## 🏗️ Arquitectura y decisiones técnicas

- **Patrón MVC** con separación clara de responsabilidades
- **Database-first**: la base de datos se diseñó primero y luego se generó el modelo con EF
- Validaciones críticas en **base de datos**, no solo en frontend
- Manejo explícito de errores SQL (códigos 2627, 2601, etc.)
- Landing + Portal + Sistema conviven bajo **un mismo dominio**, reduciendo complejidad y costos iniciales

Este enfoque prioriza:
- Mantenibilidad
- Escalabilidad
- Bajo costo de implementación inicial para PyMEs

---

## 🚀 Ejecución local

### Requisitos
- .NET SDK 8
- SQL Server Express 2022
- Visual Studio 2022

### Pasos
1. Clonar el repositorio
2. Abrir la solución en Visual Studio
3. Configurar la cadena de conexión en `appsettings.json`
4. Ejecutar migraciones / restaurar la base de datos
5. Ejecutar el proyecto con IIS Express

---

## 🔮 Funcionalidades planificadas

### Fase 2
- Autenticación (Identity)
- Roles: Administrador / Operario
- Gestión de Órdenes de trabajo
- Estados: Pendiente / En curso / Listo / Entregado
- Tablero Kanban básico
- Adjuntos por orden

### Fase 3
- Facturación electrónica (ARCA)
- Stock automatizado y mermas
- Presupuestación guiada
- Reportes y KPIs
- Integraciones con WhatsApp Business API y MercadoPago

---

## 📌 Qué demuestra este proyecto

- Comprensión del **ciclo completo de desarrollo** (análisis → diseño → implementación)
- Aplicación práctica del patrón MVC
- Uso consciente de Entity Framework
- Diseño de un MVP realista y escalable
- Capacidad de recortar alcance sin perder valor

---

## 👤 Autor

**Leonel Nicolás Bürgi**  
Estudiante de Tecnicatura Superior en Desarrollo de Software  
Rafaela, Santa Fe – Argentina

Proyecto desarrollado como **Trabajo Final Integrador**, basado en un caso real de PyME local.  
