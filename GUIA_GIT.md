# Guía de Git y GitHub para el proyecto

El enunciado exige evidenciar el uso de Git: repositorio creado, `README.md`, commits frecuentes y descriptivos, publicación en GitHub, y script SQL y documentación incluidos.

**Repositorio:** https://github.com/moncaleanomarlon-stack/ACA-Programacion-Avanzada (rama `main`).

## 1. Cómo se creó

El repositorio se creó cuando el proyecto ya tenía un primer avance. El primer commit (`chore: estado inicial recibido`) guarda ese avance tal como estaba, y cada mejora posterior tiene su propio commit. El historial es real: no se reescribió ni se crearon commits con fechas anteriores.

```bash
git init -b main
git add -A
git commit -m "chore: estado inicial recibido"
git remote add origin https://github.com/moncaleanomarlon-stack/ACA-Programacion-Avanzada.git
git push -u origin main
```

La sección 15 de [`DOCUMENTACION.md`](DOCUMENTACION.md) resume los commits por etapa. Para verlos todos:

```bash
git log --oneline --graph
```

## 2. Cómo seguir trabajando

1. Revisar qué cambió: `git status` y `git diff`.
2. Compilar y ejecutar las pruebas antes de guardar el cambio:
   ```bash
   dotnet build BibliotecaApp.sln
   dotnet run --project BibliotecaApp.Pruebas -- bd
   ```
3. Hacer un commit por cambio terminado, con un prefijo que indique su tipo:

   | Prefijo | Uso | Ejemplo del historial |
   |---|---|---|
   | `feat` | Funcionalidad nueva | `feat(libros): control de existencias y filtros que se aplican solos` |
   | `fix` | Corrección de un error | `fix(prestamos): reglas dentro de la transaccion y rollback protegido` |
   | `test` | Pruebas | `test: resultados reales de las pruebas funcionales PF01-PF23` |
   | `docs` | Documentación | `docs: enlaza el repositorio real de GitHub` |
   | `build` / `chore` | Proyecto, dependencias, mantenimiento | `build: migra a .NET 10 y corrige el csproj de pruebas` |

4. Publicar: `git push`.

## 3. Buenas prácticas

- Mensajes en imperativo, cortos y específicos (evitar "cambios" o "arreglos"); en el cuerpo del commit, explicar el porqué.
- Cada commit debe dejar el proyecto compilando y con las pruebas en verde.
- No versionar `bin/`, `obj/` ni `.vs/` (ya están en `.gitignore`).
