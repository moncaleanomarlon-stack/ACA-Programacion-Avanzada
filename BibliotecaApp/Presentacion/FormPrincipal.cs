using System;
using System.Drawing;
using System.Windows.Forms;
using BibliotecaApp.AccesoDatos;

namespace BibliotecaApp.Presentacion
{
    /// <summary>Formulario principal: menú lateral y área de contenido para cada módulo.</summary>
    public class FormPrincipal : Form
    {
        private static readonly Color ColorMenu = Color.FromArgb(33, 43, 54);
        private static readonly Color ColorActivo = Color.FromArgb(0, 120, 212);

        private readonly FlowLayoutPanel pnlMenu = new FlowLayoutPanel();
        private readonly Panel pnlEncabezado = new Panel();
        private readonly Label lblModulo = new Label();
        private readonly Panel pnlContenido = new Panel();
        private readonly StatusStrip barraEstado = new StatusStrip();
        private readonly ToolStripStatusLabel lblConexion = new ToolStripStatusLabel();

        private Form formularioActual;
        private Button botonActivo;

        public FormPrincipal()
        {
            Text = "Sistema de Gestión de Biblioteca";
            Size = new Size(1280, 760);
            MinimumSize = new Size(1050, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            pnlMenu.Dock = DockStyle.Left;
            pnlMenu.Width = 190;
            pnlMenu.BackColor = ColorMenu;
            pnlMenu.FlowDirection = FlowDirection.TopDown;
            pnlMenu.WrapContents = false;
            pnlMenu.Padding = new Padding(0, 16, 0, 0);

            pnlEncabezado.Dock = DockStyle.Top;
            pnlEncabezado.Height = 52;
            pnlEncabezado.BackColor = Color.FromArgb(243, 243, 243);

            lblModulo.Dock = DockStyle.Fill;
            lblModulo.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblModulo.TextAlign = ContentAlignment.MiddleLeft;
            lblModulo.Padding = new Padding(16, 0, 0, 0);
            pnlEncabezado.Controls.Add(lblModulo);

            pnlContenido.Dock = DockStyle.Fill;

            barraEstado.Items.Add(lblConexion);

            // Botones de navegación (RF08)
            Button btnInicio = CrearBotonMenu("Inicio", "Inicio", delegate { return new FormInicio(); });
            CrearBotonMenu("Libros", "Gestión de libros", delegate { return new FormLibros(); });
            CrearBotonMenu("Usuarios", "Gestión de usuarios", delegate { return new FormUsuarios(); });
            CrearBotonMenu("Autores", "Gestión de autores", delegate { return new FormAutores(); });
            CrearBotonMenu("Editoriales", "Gestión de editoriales", delegate { return new FormEditoriales(); });
            CrearBotonMenu("Préstamos", "Préstamos y devoluciones", delegate { return new FormPrestamos(); });
            CrearBotonMenu("Reportes", "Reportes y consultas", delegate { return new FormReportes(); });

            // Orden: Fill primero; luego los acoplados a los bordes.
            Controls.Add(pnlContenido);
            Controls.Add(pnlEncabezado);
            Controls.Add(pnlMenu);
            Controls.Add(barraEstado);

            Shown += delegate
            {
                VerificarConexion();
                Mostrar(new FormInicio(), "Inicio", btnInicio);
            };
        }

        private Button CrearBotonMenu(string texto, string titulo, Func<Form> fabrica)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Size = new Size(190, 46);
            boton.Margin = new Padding(0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.ForeColor = Color.White;
            boton.BackColor = ColorMenu;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Padding = new Padding(18, 0, 0, 0);
            boton.Font = new Font("Segoe UI", 11f);
            boton.Cursor = Cursors.Hand;
            boton.Click += delegate { Mostrar(fabrica(), titulo, boton); };
            pnlMenu.Controls.Add(boton);
            return boton;
        }

        private void Mostrar(Form formulario, string titulo, Button boton)
        {
            if (formularioActual != null)
            {
                pnlContenido.Controls.Remove(formularioActual);
                formularioActual.Dispose();
            }
            if (botonActivo != null) botonActivo.BackColor = ColorMenu;
            botonActivo = boton;
            botonActivo.BackColor = ColorActivo;

            lblModulo.Text = titulo;
            formularioActual = formulario;
            pnlContenido.Controls.Add(formulario);
            formulario.Show();
        }

        private void VerificarConexion()
        {
            string mensaje;
            bool ok = new ConexionBD().ProbarConexion(out mensaje);
            lblConexion.Text = mensaje;
            lblConexion.ForeColor = ok ? Color.DarkGreen : Color.Firebrick;
            if (!ok)
                Mensajes.Advertencia("No se pudo conectar con SQL Server.\n\n" + mensaje +
                    "\n\nEjecute sql/Script_Biblioteca_Completo.sql y revise la cadena de conexión en App.config.");
        }
    }
}
