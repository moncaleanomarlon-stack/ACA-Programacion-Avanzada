// Convierte DOCUMENTACION.md en PDF: Markdig -> HTML, y Edge sin interfaz imprime el
// HTML con el protocolo DevTools (Page.printToPDF) para poner "Página X de Y" en el pie.
// Uso: MdPdf <archivo.md> <salida.pdf> <ruta msedge.exe> <carpeta temporal>
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;

string md = Path.GetFullPath(args[0]);
string pdf = Path.GetFullPath(args[1]);
string edge = args[2];
string tmp = Path.GetFullPath(args[3]);
Directory.CreateDirectory(tmp);

var pipeline = new MarkdownPipelineBuilder()
    .UsePipeTables().UseEmphasisExtras().UseAutoLinks()
    .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
    .Build();
string cuerpo = Markdown.ToHtml(File.ReadAllText(md, Encoding.UTF8), pipeline);
string baseHref = new Uri(Path.GetDirectoryName(md)! + Path.DirectorySeparatorChar).AbsoluteUri;

string html = $$"""
<!doctype html><html lang="es"><head><meta charset="utf-8">
<base href="{{baseHref}}">
<title>Sistema de Gestión de Biblioteca — Documentación técnica</title>
<style>
  @page { size: A4; margin: 18mm 16mm 20mm 16mm; }
  body { font-family: "Segoe UI", Arial, sans-serif; font-size: 10.5pt; line-height: 1.45; color: #20262e; }
  h1 { font-size: 20pt; margin: 0.4em 0; } h2 { font-size: 15pt; border-bottom: 1px solid #d0d7de; padding-bottom: 4px; margin-top: 1.4em; break-after: avoid; }
  h3 { font-size: 12pt; margin-top: 1.1em; break-after: avoid; }
  table { border-collapse: collapse; width: 100%; margin: 0.6em 0 1em; font-size: 8.8pt; }
  /* overflow-wrap: break-word no reduce el ancho mínimo de columna (las palabras no se cortan letra a letra) */
  th, td { border: 1px solid #d0d7de; padding: 4px 6px; vertical-align: top; text-align: left; word-break: normal; overflow-wrap: break-word; hyphens: manual; }
  td:first-child, th:first-child { white-space: nowrap; }
  th { background: #f3f5f8; } tr { break-inside: avoid; }
  code { font-family: Consolas, monospace; font-size: 8.8pt; background: #f3f5f8; padding: 0 3px; border-radius: 3px; }
  pre { background: #f6f8fa; padding: 8px 10px; border-radius: 4px; font-size: 7.4pt; white-space: pre-wrap; break-inside: avoid; }
  pre code { background: none; padding: 0; }
  img { max-width: 100%; max-height: 225mm; width: auto; height: auto; object-fit: contain; border: 1px solid #e2e6ec; break-inside: avoid; display: block; margin: 6px auto; }
  blockquote { border-left: 4px solid #d0d7de; margin: 0.6em 0; padding: 2px 12px; color: #57606a; }
  a { color: #0067c0; text-decoration: none; }
  div[align="center"] { text-align: center; padding-top: 70mm; }
  div[align="center"] img { display: inline; }
</style></head><body>
{{cuerpo}}
</body></html>
""";
string htmlPath = Path.Combine(tmp, "documentacion.html");
File.WriteAllText(htmlPath, html, new UTF8Encoding(false));

// Edge sin interfaz con depuración remota en un perfil temporal
int puerto = 9333;
var psi = new ProcessStartInfo(edge,
    $"--headless=new --disable-gpu --no-first-run --no-default-browser-check --remote-debugging-port={puerto} --user-data-dir=\"{Path.Combine(tmp, "perfil")}\" about:blank")
    { UseShellExecute = false };
using var navegador = Process.Start(psi)!;
try
{
    using var http = new HttpClient();
    string wsUrl = "";
    for (int i = 0; i < 50 && wsUrl == ""; i++)
    {
        try
        {
            var lista = JsonNode.Parse(await http.GetStringAsync($"http://127.0.0.1:{puerto}/json/list"))!.AsArray();
            foreach (var p in lista) if ((string?)p!["type"] == "page") { wsUrl = (string)p["webSocketDebuggerUrl"]!; break; }
        }
        catch { }
        if (wsUrl == "") await Task.Delay(200);
    }
    if (wsUrl == "") throw new Exception("No se pudo conectar con Edge");

    using var ws = new ClientWebSocket();
    ws.Options.KeepAliveInterval = TimeSpan.Zero;
    await ws.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
    int id = 0;

    async Task<JsonNode> Recibir()
    {
        var buf = new byte[1 << 20]; var ms = new MemoryStream();
        WebSocketReceiveResult r;
        do { r = await ws.ReceiveAsync(buf, CancellationToken.None); ms.Write(buf, 0, r.Count); } while (!r.EndOfMessage);
        return JsonNode.Parse(Encoding.UTF8.GetString(ms.ToArray()))!;
    }
    async Task<JsonNode> Enviar(string metodo, object? parametros = null)
    {
        int mio = ++id;
        string json = JsonSerializer.Serialize(new { id = mio, method = metodo, @params = parametros ?? new { } });
        await ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);
        while (true) { var m = await Recibir(); if ((int?)m["id"] == mio) { if (m["error"] != null) throw new Exception(m["error"]!.ToJsonString()); return m["result"]!; } }
    }
    async Task Esperar(string evento)
    {
        while (true) { var m = await Recibir(); if ((string?)m["method"] == evento) return; }
    }

    await Enviar("Page.enable");
    await Enviar("Page.navigate", new { url = new Uri(htmlPath).AbsoluteUri });
    await Esperar("Page.loadEventFired");
    await Task.Delay(1500);   // imágenes y fuentes

    string pie = "<div style=\"font-family:Segoe UI,Arial;font-size:8px;color:#6b7480;width:100%;padding:0 16mm;display:flex;justify-content:space-between\">" +
                 "<span>Sistema de Gestión de Biblioteca — Documentación técnica</span>" +
                 "<span>Página <span class=\"pageNumber\"></span> de <span class=\"totalPages\"></span></span></div>";
    var res = await Enviar("Page.printToPDF", new
    {
        printBackground = true, preferCSSPageSize = true, displayHeaderFooter = true,
        headerTemplate = "<div></div>", footerTemplate = pie,
        generateDocumentOutline = true, generateTaggedPDF = true
    });
    File.WriteAllBytes(pdf, Convert.FromBase64String((string)res["data"]!));
    try { await Enviar("Browser.close"); } catch { }
    Console.WriteLine("PDF generado: " + pdf + " (" + new FileInfo(pdf).Length / 1024 + " KB)");
}
finally
{
    try { if (!navegador.HasExited) { navegador.WaitForExit(3000); if (!navegador.HasExited) navegador.Kill(true); } } catch { }
}
