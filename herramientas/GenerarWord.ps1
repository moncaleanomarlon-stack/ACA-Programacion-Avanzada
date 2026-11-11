param([string]$Html, [string]$Docx, [string]$PdfPrueba)
# Convierte el HTML de la documentación en un .docx usando Microsoft Word (COM).
$ErrorActionPreference = "Stop"

# 1. HTML adaptado a Word: saltos de página que Word reconoce y sin reglas CSS que no entiende
$h = Get-Content $Html -Raw -Encoding UTF8
$h = $h -replace '<div style="page-break-after: always;"></div>', "<br clear=all style='page-break-before:always'>"
$h = $h -replace 'div\[align="center"\] \{[^}]*\}', '' -replace 'div\[align="center"\] img \{[^}]*\}', ''
$h = $h -replace '@page \{[^}]*\}', ''
$tmpHtml = [IO.Path]::ChangeExtension($Html, ".word.html")
[IO.File]::WriteAllText($tmpHtml, $h, (New-Object Text.UTF8Encoding($false)))

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
try {
  $doc = $word.Documents.Open($tmpHtml, $false, $true)   # sin confirmar conversión, solo lectura
  $doc.ActiveWindow.View.Type = 3                         # diseño de impresión

  # 2. Página A4 con márgenes
  $ps = $doc.PageSetup
  $ps.PaperSize = 7                                       # wdPaperA4
  $ps.TopMargin = $word.CentimetersToPoints(2); $ps.BottomMargin = $word.CentimetersToPoints(2.2)
  $ps.LeftMargin = $word.CentimetersToPoints(2); $ps.RightMargin = $word.CentimetersToPoints(2)
  $ancho = $ps.PageWidth - $ps.LeftMargin - $ps.RightMargin
  $alto = $ps.PageHeight - $ps.TopMargin - $ps.BottomMargin - 40

  # 3. Imágenes incrustadas (no enlazadas) y ajustadas a la página
  for ($i = $doc.InlineShapes.Count; $i -ge 1; $i--) {
    $s = $doc.InlineShapes.Item($i)
    try { if ($s.LinkFormat) { $s.LinkFormat.SavePictureWithDocument = $true; $s.LinkFormat.BreakLink() } } catch {}
  }
  foreach ($s in $doc.InlineShapes) {
    $s.LockAspectRatio = -1
    if ($s.Width -gt $ancho) { $s.Width = $ancho }
    if ($s.Height -gt $alto) { $s.Height = $alto }
    $s.Range.ParagraphFormat.Alignment = 1               # centrado
  }

  # 4. Tablas al ancho de la página
  # Cada columna se ajusta a su contenido y luego todas se escalan en proporción
  # hasta ocupar exactamente el ancho útil (con un mínimo para columnas muy cortas).
  foreach ($t in $doc.Tables) {
    $t.AllowAutoFit = $true
    $t.Range.Cells.PreferredWidthType = 1                 # sin anchos fijos heredados del HTML
    $t.AutoFitBehavior(1)                                 # según el contenido
    $t.AllowAutoFit = $false
    $n = $t.Columns.Count
    $anchos = @(); for ($j = 1; $j -le $n; $j++) { $anchos += [double]$t.Columns.Item($j).Width }
    # Una columna puede quedar muy ancha si tiene texto largo; se limita al 55 % antes de escalar
    for ($j = 0; $j -lt $n; $j++) { $anchos[$j] = [Math]::Min($anchos[$j], $ancho * 0.55) }
    $suma = ($anchos | Measure-Object -Sum).Sum
    for ($j = 1; $j -le $n; $j++) { $t.Columns.Item($j).Width = [Math]::Max(36, $anchos[$j - 1] * $ancho / $suma) }
    $t.Rows.AllowBreakAcrossPages = $false
    # Filas compactas: sin espacio extra de párrafo dentro de las celdas
    $t.Range.ParagraphFormat.SpaceBefore = 0
    $t.Range.ParagraphFormat.SpaceAfter = 0
    $t.Range.ParagraphFormat.LineSpacingRule = 0          # sencillo
    $t.TopPadding = 2; $t.BottomPadding = 2
    $t.Rows.HeightRule = 0                                # alto automático
  }

  # 5. Tabla de contenido real de Word en lugar de la lista de enlaces
  $ini = $null; $fin = $null
  foreach ($p in $doc.Paragraphs) {
    $txt = $p.Range.Text.Trim()
    if ($ini -eq $null -and $txt -eq "Tabla de contenido") { $ini = $p }
    elseif ($ini -ne $null -and $txt -like "Numeración de páginas*") { $fin = $p; break }
  }
  if ($ini -and $fin) {
    # El título del índice no debe aparecer dentro del propio índice: estilo normal con formato de título
    $ini.Range.Style = -1                                 # wdStyleNormal
    $ini.Range.Font.Name = "Segoe UI"; $ini.Range.Font.Size = 15; $ini.Range.Font.Bold = $true
    $ini.Range.ParagraphFormat.SpaceAfter = 10
    $r = $doc.Range($ini.Range.End, $fin.Range.Start)
    $r.Text = ""
    $r.InsertParagraphAfter()
    $toc = $doc.TablesOfContents.Add($r, $true, 2, 2)     # títulos de nivel 2 (secciones 1 a 18)
    $toc.Update()
    # El índice no debe heredar el borde lateral ni la sangría de la nota que lo sigue
    $toc.Range.ParagraphFormat.Borders.Enable = 0
    $toc.Range.ParagraphFormat.LeftIndent = 0
    foreach ($div in $doc.HTMLDivisions) {
      if ($div.Range.Start -le $toc.Range.Start -and $div.Range.End -ge $toc.Range.Start) {
        $div.Borders.Enable = 0; $div.LeftIndent = 0
      }
    }
    # Estilo de las entradas del índice (TDC 2 = wdStyleTOC2, -21), números incluidos
    $doc.Styles.Item(-21).Font.Name = "Segoe UI"
    $toc.Update()
  }

  # 6. Pie de página: "Sistema de Gestión de Biblioteca — Documentación técnica      Página X de Y"
  $pie = $doc.Sections.Item(1).Footers.Item(1).Range
  $pie.Text = "Sistema de Gestión de Biblioteca — Documentación técnica`tPágina "
  $pie.Font.Size = 8; $pie.Font.Name = "Segoe UI"; $pie.Font.Color = 0x80746B
  $pie.ParagraphFormat.TabStops.ClearAll()
  [void]$pie.ParagraphFormat.TabStops.Add($ancho, 2)      # tabulación derecha
  $r = $doc.Sections.Item(1).Footers.Item(1).Range; $r.Collapse(0); [void]$r.Fields.Add($r, 33)    # PAGE
  $r = $doc.Sections.Item(1).Footers.Item(1).Range; $r.Collapse(0); $r.InsertAfter(" de ")
  $r = $doc.Sections.Item(1).Footers.Item(1).Range; $r.Collapse(0); [void]$r.Fields.Add($r, 26)    # NUMPAGES
  $doc.Sections.Item(1).Footers.Item(1).Range.Fields.Update() | Out-Null


  $doc.Fields.Update() | Out-Null
  if ($doc.TablesOfContents.Count -gt 0) { $doc.TablesOfContents.Item(1).Update() }

  $doc.SaveAs2($Docx, 16)                                 # .docx
  $doc.SaveAs2($PdfPrueba, 17)                            # PDF solo para revisar
  "Páginas: " + $doc.ComputeStatistics(2) + " | Imágenes: " + $doc.InlineShapes.Count + " | Tablas: " + $doc.Tables.Count
  $doc.Close(0)
} finally {
  # Cierra todo sin guardar (lo que había que guardar ya se guardó) para no dejar Word abierto
  try { foreach ($d in @($word.Documents)) { $d.Close(0) } } catch {}
  $word.Quit()
  [void][Runtime.InteropServices.Marshal]::ReleaseComObject($word)
}










