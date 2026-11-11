# Herramientas auxiliares

No forman parte de la aplicación ni de la solución `BibliotecaApp.sln`.

## GenerarPdf

Genera `DOCUMENTACION.pdf` a partir de `DOCUMENTACION.md`: convierte el Markdown a HTML con [Markdig](https://github.com/xoofx/markdig) y lo imprime en A4 con Microsoft Edge sin interfaz, con «Página X de Y» en el pie de cada página. Las imágenes de `docs/` se incluyen en el PDF.

Desde la carpeta raíz del repositorio:

```bash
dotnet run --project herramientas/GenerarPdf -- DOCUMENTACION.md DOCUMENTACION.pdf "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" "%TEMP%\GenerarPdf"
```

Argumentos: archivo Markdown, PDF de salida, ruta de `msedge.exe` y una carpeta temporal (para el HTML intermedio y el perfil de Edge).
