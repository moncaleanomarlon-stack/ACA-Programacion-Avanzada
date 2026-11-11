# Guía de Git y GitHub para el proyecto

El enunciado exige evidenciar el uso de Git: repositorio creado, `README.md`, commits frecuentes y descriptivos, publicación en GitHub, y script SQL y documentación incluidos.

## 1. Crear y publicar el repositorio

```bash
git init
git branch -M main
git add .gitignore README.md
git commit -m "docs: crea repositorio y README inicial"
git remote add origin https://github.com/moncaleanomarlon-stack/ACA-Programacion-Avanzada.git
git push -u origin main
```

## 2. Commits sugeridos (uno por avance real, en este orden)

Haz cada commit **cuando termines esa parte**, no todos juntos al final. Usa el prefijo convencional (`feat`, `fix`, `docs`, `refactor`, `test`, `chore`).

```text
feat(bd): crea script SQL con tablas, claves y restricciones
feat(bd): agrega datos de prueba al script SQL
feat(bd): agrega vistas para reportes
feat(entidades): agrega clases Persona, Autor, Usuario, Editorial, Libro y Prestamo
feat(datos): agrega ConexionBD con consultas parametrizadas y traduccion de errores
feat(datos): agrega DAL de autores, editoriales, usuarios y libros
feat(datos): agrega PrestamoDAL con transacciones de prestamo y devolucion
feat(negocio): agrega Validador y clases BLL
feat(ui): agrega FormPrincipal con navegacion lateral
feat(ui): agrega FormCrudBase y formularios de autores, editoriales y usuarios
feat(ui): agrega formulario de libros con busqueda por ISBN, titulo y autor
feat(ui): agrega formulario de prestamos y devoluciones
feat(ui): agrega modulo de reportes con exportacion a CSV
test: agrega pruebas automaticas de validaciones
docs: agrega diagramas ER, clases y casos de uso
docs: completa documento tecnico y capturas de pantalla
```

## 3. Buenas prácticas

- Revisa `git status` y `git diff` antes de cada commit.
- Mensajes en imperativo, cortos y específicos (evita "cambios" o "arreglos").
- Cada commit debe dejar el proyecto compilando.
- Para mostrar el historial en el documento: `git log --oneline --graph` y toma una captura.
