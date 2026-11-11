using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Presentacion
{
    /// <summary>
    /// Pantalla de inicio: indicadores generales (cada tarjeta abre su módulo)
    /// y accesos rápidos a las tareas más frecuentes.
    /// </summary>
    public class FormInicio : Form
    {
        /// <summary>Color, icono y módulo de destino de cada indicador de VISTA_ResumenGeneral.</summary>
        private class Indicador
        {
            public Color Color; public string Icono; public string Modulo; public bool SoloActivos;
            public Indicador(Color c, string i, string m, bool a = false) { Color = c; Icono = i; Modulo = m; SoloActivos = a; }
        }

        private static readonly Dictionary<string, Indicador> Indicadores = new Dictionary<string, Indicador>
        {
            { "Títulos registrados",    new Indicador(Color.FromArgb(0, 103, 192),  Tema.IconoLibros,      "Libros") },
            { "Ejemplares disponibles", new Indicador(Color.FromArgb(0, 130, 114),  "",              "Libros") },
            { "Autores",                new Indicador(Color.FromArgb(92, 45, 145),  Tema.IconoAutores,     "Autores") },
            { "Editoriales",            new Indicador(Color.FromArgb(135, 100, 184), Tema.IconoEditoriales, "Editoriales") },
            { "Usuarios",               new Indicador(Color.FromArgb(0, 99, 177),   Tema.IconoUsuarios,    "Usuarios") },
            { "Préstamos activos",      new Indicador(Tema.Aviso,                  Tema.IconoPrestamos,   "Préstamos", true) },
            { "Préstamos devueltos",    new Indicador(Tema.Exito,                  "",              "Préstamos") }
        };

        private readonly ReporteBLL reporteBll = new ReporteBLL();
        private readonly Func<string, Form> navegar;
        private readonly FlowLayoutPanel contenedor = new FlowLayoutPanel();
        private readonly FlowLayoutPanel pnlIndicadores = new FlowLayoutPanel();
        private readonly FlowLayoutPanel pnlAccesos = new FlowLayoutPanel();
        private readonly Label lblMensaje = new Label();

        /// <param name="navegar">Abre un módulo por nombre (lo proporciona FormPrincipal).</param>
        public FormInicio(Func<string, Form> navegar)
        {
            this.navegar = navegar;
            Text = "Inicio";
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            Font = Tema.Normal;
            BackColor = Tema.FondoSuave;

            // Contenedor vertical con desplazamiento: mensaje, indicadores y accesos rápidos
            contenedor.Dock = DockStyle.Fill;
            contenedor.FlowDirection = FlowDirection.TopDown;
            contenedor.WrapContents = false;
            contenedor.AutoScroll = true;
            contenedor.Padding = new Padding(24, 20, 24, 24);

            lblMensaje.Text = "Seleccione un indicador para abrir su módulo, o use los accesos rápidos.";
            lblMensaje.ForeColor = Tema.TextoSuave;
            lblMensaje.AutoSize = true;
            lblMensaje.Margin = new Padding(4, 0, 0, 12);

            ConfigurarFila(pnlIndicadores);
            ConfigurarFila(pnlAccesos);

            contenedor.Controls.Add(lblMensaje);
            contenedor.Controls.Add(pnlIndicadores);
            contenedor.Controls.Add(Seccion("Accesos rápidos"));
            contenedor.Controls.Add(pnlAccesos);
            Controls.Add(contenedor);

            pnlAccesos.Controls.Add(CrearAcceso("Registrar préstamo", Tema.IconoPrestamos, Tema.Primario, "Préstamos", false));
            pnlAccesos.Controls.Add(CrearAcceso("Registrar devolución", "", Tema.Aviso, "Préstamos", true));
            pnlAccesos.Controls.Add(CrearAcceso("Nuevo libro", "", Tema.Exito, "Libros", false));
            pnlAccesos.Controls.Add(CrearAcceso("Ver reportes", Tema.IconoReportes, Tema.Neutro, "Reportes", false));

            // Las filas envuelven sus tarjetas según el ancho disponible
            contenedor.Resize += delegate { AjustarAncho(); };
            Load += delegate { CargarIndicadores(); AjustarAncho(); };
        }

        private static void ConfigurarFila(FlowLayoutPanel fila)
        {
            fila.FlowDirection = FlowDirection.LeftToRight;
            fila.WrapContents = true;
            fila.AutoSize = true;
            fila.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fila.Margin = new Padding(0, 0, 0, 8);
        }

        private void AjustarAncho()
        {
            int ancho = Math.Max(300, contenedor.ClientSize.Width - contenedor.Padding.Horizontal);
            pnlIndicadores.MaximumSize = new Size(ancho, 0);
            pnlAccesos.MaximumSize = new Size(ancho, 0);
        }

        private static Label Seccion(string texto)
        {
            Label l = new Label();
            l.Text = texto;
            l.Font = new Font(Tema.Familia, 11f, FontStyle.Bold);
            l.ForeColor = Tema.Texto;
            l.AutoSize = true;
            l.Margin = new Padding(4, 16, 0, 8);
            return l;
        }

        private void CargarIndicadores()
        {
            try
            {
                DataTable resumen = reporteBll.Resumen();
                pnlIndicadores.Controls.Clear();
                foreach (DataRow fila in resumen.Rows)
                    pnlIndicadores.Controls.Add(CrearTarjeta(fila["Concepto"].ToString(), fila["Total"].ToString()));
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "No fue posible cargar los indicadores: " + ex.Message;
                lblMensaje.ForeColor = Tema.Peligro;
            }
        }

        /// <summary>Abre el módulo cuando termina el clic actual (este formulario se cierra al navegar).</summary>
        private void Abrir(string modulo, bool soloActivos)
        {
            if (navegar == null) return;
            Control principal = TopLevelControl;
            principal.BeginInvoke((Action)delegate
            {
                FormPrestamos prestamos = navegar(modulo) as FormPrestamos;
                if (prestamos != null && soloActivos) prestamos.MostrarSoloActivos();
            });
        }

        private Panel CrearTarjeta(string concepto, string valor)
        {
            Indicador ind;
            if (!Indicadores.TryGetValue(concepto, out ind))
                ind = new Indicador(Tema.Primario, Tema.IconoInicio, null);

            Panel tarjeta = new Panel();
            tarjeta.Size = new Size(214, 104);
            tarjeta.Margin = new Padding(4, 4, 12, 12);
            tarjeta.BackColor = Tema.Fondo;
            tarjeta.Cursor = ind.Modulo != null ? Cursors.Hand : Cursors.Default;
            tarjeta.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen borde = new Pen(Tema.Borde))
                    e.Graphics.DrawRectangle(borde, 0, 0, tarjeta.Width - 1, tarjeta.Height - 1);
                using (SolidBrush acento = new SolidBrush(ind.Color))
                    e.Graphics.FillRectangle(acento, 0, 0, 4, tarjeta.Height);
            };

            Label lblIcono = new Label();
            lblIcono.Text = ind.Icono;
            lblIcono.Font = new Font(Tema.FamiliaIconos, 16f);
            lblIcono.ForeColor = ind.Color;
            lblIcono.AutoSize = true;
            lblIcono.Location = new Point(170, 16);

            Label lblValor = new Label();
            lblValor.Text = valor;
            lblValor.ForeColor = Tema.Texto;
            lblValor.Font = new Font(Tema.Familia, 24f, FontStyle.Bold);
            lblValor.AutoSize = true;
            lblValor.Location = new Point(16, 10);

            Label lblConcepto = new Label();
            lblConcepto.Text = concepto;
            lblConcepto.ForeColor = Tema.TextoSuave;
            lblConcepto.AutoSize = true;
            lblConcepto.Location = new Point(18, 66);

            tarjeta.Controls.Add(lblIcono);
            tarjeta.Controls.Add(lblValor);
            tarjeta.Controls.Add(lblConcepto);

            if (ind.Modulo != null)
            {
                EventHandler alPulsar = delegate { Abrir(ind.Modulo, ind.SoloActivos); };
                tarjeta.Click += alPulsar;
                foreach (Control c in tarjeta.Controls) { c.Click += alPulsar; c.Cursor = Cursors.Hand; }
                new ToolTip().SetToolTip(tarjeta, "Abrir " + ind.Modulo);
            }
            return tarjeta;
        }

        private Button CrearAcceso(string texto, string icono, Color color, string modulo, bool soloActivos)
        {
            Button b = new Button();
            b.Text = texto;
            b.Size = new Size(214, 48);
            b.Margin = new Padding(4, 4, 12, 12);
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.Padding = new Padding(44, 0, 0, 0);
            Tema.EstilizarBoton(b, color);
            b.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Font f = new Font(Tema.FamiliaIconos, 13f))
                    TextRenderer.DrawText(e.Graphics, icono, f, new Rectangle(16, 0, 28, b.Height),
                        b.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };
            b.Click += delegate { Abrir(modulo, soloActivos); };
            return b;
        }
    }
}
