# Herramientas auxiliares

No forman parte de la aplicación ni de la solución `BibliotecaApp.sln`.

## GenerarPdf

Genera `DOCUMENTACION.pdf` a partir de `DOCUMENTACION.md`: convierte el Markdown a HTML con [Markdig](https://github.com/xoofx/markdig) y lo imprime en A4 con Microsoft Edge sin interfaz, con «Página X de Y» en el pie de cada página. Las imágenes de `docs/` se incluyen en el PDF.

Desde la carpeta raíz del repositorio:

```bash
dotnet run --project herramientas/GenerarPdf -- DOCUMENTACION.md DOCUMENTACION.pdf "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" "%TEMP%\GenerarPdf"
```

Argumentos: archivo Markdown, PDF de salida, ruta de `msedge.exe` y una carpeta temporal (para el HTML intermedio y el perfil de Edge).

## GenerarWord.ps1

Genera `DOCUMENTACION.docx` con Microsoft Word (requiere Word instalado), a partir del HTML intermedio que deja `GenerarPdf` en su carpeta temporal (`documentacion.html`). Incrusta las imágenes, ajusta tablas e imágenes al ancho de la página A4, inserta una **tabla de contenido** de Word y el pie «Página X de Y». También guarda un PDF de revisión.

```powershell
# 1. Generar el PDF (deja el HTML intermedio en la carpeta temporal)
dotnet run --project herramientas/GenerarPdf -- DOCUMENTACION.md DOCUMENTACION.pdf "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" "$env:TEMP\GenerarPdf"
# 2. Convertir ese HTML a Word
powershell -ExecutionPolicy Bypass -File herramientas\GenerarWord.ps1 -Html "$env:TEMP\GenerarPdf\documentacion.html" -Docx "$PWD\DOCUMENTACION.docx" -PdfPrueba "$env:TEMP\GenerarPdf\revision_word.pdf"
```

En Word, la tabla de contenido y los números de página se actualizan con clic derecho → *Actualizar campos*.
