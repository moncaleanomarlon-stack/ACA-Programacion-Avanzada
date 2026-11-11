# Sistema de Gestión de Biblioteca

Aplicación de escritorio desarrollada en **C# (Windows Forms, .NET 8)** y **SQL Server** para administrar libros, autores, editoriales, usuarios, préstamos y devoluciones, con módulo de reportes.

> Proyecto ACA Final — Programación Avanzada (2026) · CUN · Ingeniería de Sistemas
> Autor: Marlon Fabián Moncaleano Núñez

## Características

- Módulos: **Inicio, Libros, Usuarios, Autores, Editoriales, Préstamos y Reportes** (navegación lateral desde `FormPrincipal`).
- CRUD completo de libros, autores, editoriales y usuarios, con búsqueda (libros por **ISBN, título o autor**).
- Préstamos y devoluciones con **transacciones SQL**: al prestar se descuenta el stock y al devolver se repone; no se puede devolver dos veces ni prestar sin existencias.
- Reglas de negocio: máximo 3 préstamos activos por usuario y no repetir un libro activo para el mismo usuario.
- 9 reportes (resumen, libros, autores, editoriales, usuarios, préstamos activos/devueltos, historial, más prestados) con **exportación a CSV**.
- Arquitectura por capas (Presentación → Lógica de negocio → Acceso a datos → SQL Server) más capa de Entidades (POO).
- Consultas **100 % parametrizadas**, validación de datos y manejo de excepciones con mensajes claros.

## Estructura del repositorio

```
BibliotecaApp.sln
BibliotecaApp/                 Proyecto principal (Windows Forms)
  Entidades/                   Persona (abstracta), Autor, Usuario, Editorial, Libro, Prestamo
  AccesoDatos/                 ConexionBD y un DAL por entidad (+ ReporteDAL)
  LogicaNegocio/               Validador y un BLL por módulo
  Presentacion/                Formularios (FormCrudBase es la base de los CRUD)
  App.config                   Cadena de conexión
BibliotecaApp.Pruebas/         Pruebas automáticas (validaciones, reglas y, con "bd", contra SQL Server)
sql/Script_Biblioteca_Completo.sql   Base de datos, restricciones, datos de prueba y vistas
docs/                          Diagramas (ER, clases, casos de uso, capas) y capturas
DOCUMENTACION.md / .pdf        Documento técnico del proyecto
GUIA_GIT.md                    Flujo de commits sugerido
```

## Requisitos

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (o Visual Studio 2022 con la carga de trabajo *Desarrollo de escritorio de .NET*)
- SQL Server 2019/2022 (Developer, Express o LocalDB) y SQL Server Management Studio

## Instrucciones de ejecución

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/moncaleanomarlon-stack/ACA-Programacion-Avanzada.git
   cd ACA-Programacion-Avanzada
   ```
2. Abrir `sql/Script_Biblioteca_Completo.sql` en SSMS y **ejecutarlo completo** (crea `BibliotecaDB`, tablas, datos de prueba y vistas).
3. Revisar la cadena de conexión en `BibliotecaApp/App.config` y ajustar `Server` si es necesario:

   | Instalación | Server |
   |---|---|
   | Instancia por defecto | `localhost` |
   | SQL Server Express | `.\SQLEXPRESS` |
   | LocalDB | `(localdb)\MSSQLLocalDB` |

4. Abrir `BibliotecaApp.sln` en Visual Studio y pulsar **F5**, o desde consola:
   ```bash
   dotnet run --project BibliotecaApp
   ```
5. (Opcional) Ejecutar las pruebas automáticas. Sin argumentos no requieren base de datos
   (validaciones, entidades y reglas de préstamo):
   ```bash
   dotnet run --project BibliotecaApp.Pruebas
   ```
   Con el argumento `bd` además prueban préstamos, devoluciones, stock y duplicados contra
   `BibliotecaDB`; crean sus propios datos y los borran al terminar:
   ```bash
   dotnet run --project BibliotecaApp.Pruebas -- bd
   ```

La barra de estado de la ventana principal indica si la conexión con SQL Server fue exitosa.

## Capturas de pantalla

Ver la carpeta [`docs/capturas`](docs/capturas) y la sección 13 de [`DOCUMENTACION.md`](DOCUMENTACION.md).

## Documentación

El documento técnico completo (requerimientos, casos de uso, diagrama de clases, modelo E-R, diccionario de datos, arquitectura, pruebas, conclusiones y referencias APA) está en [`DOCUMENTACION.md`](DOCUMENTACION.md) y en PDF con numeración de páginas.
