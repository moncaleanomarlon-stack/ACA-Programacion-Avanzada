using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using BibliotecaApp.AccesoDatos;

namespace BibliotecaApp.Presentacion
{
    /// <summary>Formulario principal: menú lateral y área de contenido para cada módulo (RF08).</summary>
    public class FormPrincipal : Form
    {
        /// <summary>Datos de cada opción del menú.</summary>
        private class Modulo
        {
            public string Nombre;
            public string Icono;
            public string Titulo;
            public string Descripcion;
            public Func<Form> Fabrica;
            public Button Boton;
        }

        private readonly Panel pnlLateral = new Panel();
        private readonly FlowLayoutPanel pnlMenu = new FlowLayoutPanel();
        private readonly Panel pnlEncabezado = new Panel();
        private readonly Label lblModulo = new Label();
        private readonly Label lblDescripcion = new Label();
        private readonly Panel pnlContenido = new Panel();
        private readonly StatusStrip barraEstado = new StatusStrip();
        private readonly ToolStripStatusLabel lblConexion = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel lblFecha = new ToolStripStatusLabel();
        private readonly Timer reloj = new Timer();

        private readonly Dictionary<string, Modulo> modulos = new Dictionary<string, Modulo>();
        private Form formularioActual;
        private Modulo moduloActivo;

        public FormPrincipal()
        {
            Text = "Sistema de Gestión de Biblioteca";
            Size = new Size(1280, 760);
            MinimumSize = new Size(1050, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Tema.Normal;
            BackColor = Tema.Fondo;

            ConstruirMenuLateral();
            ConstruirEncabezado();
            ConstruirBarraEstado();
            pnlContenido.Dock = DockStyle.Fill;

            AgregarModulo("Inicio", Tema.IconoInicio, "Inicio",
                "Resumen general de la biblioteca", delegate { return new FormInicio(IrA); });
            AgregarModulo("Libros", Tema.IconoLibros, "Gestión de libros",
                "Registre, busque y actualice el catálogo y sus existencias", delegate { return new FormLibros(); });
            AgregarModulo("Usuarios", Tema.IconoUsuarios, "Gestión de usuarios",
                "Personas que pueden solicitar préstamos", delegate { return new FormUsuarios(); });
            AgregarModulo("Autores", Tema.IconoAutores, "Gestión de autores",
                "Autores asociados a los libros del catálogo", delegate { return new FormAutores(); });
            AgregarModulo("Editoriales", Tema.IconoEditoriales, "Gestión de editoriales",
                "Editoriales y sus datos de contacto", delegate { return new FormEditoriales(); });
            AgregarModulo("Préstamos", Tema.IconoPrestamos, "Préstamos y devoluciones",
                "Registre préstamos, devoluciones y consulte su estado", delegate { return new FormPrestamos(); });
            AgregarModulo("Reportes", Tema.IconoReportes, "Reportes y consultas",
                "Consultas consolidadas con exportación a CSV", delegate { return new FormReportes(); });

            // Orden: Fill primero; luego los acoplados a los bordes.
            Controls.Add(pnlContenido);
            Controls.Add(pnlEncabezado);
            Controls.Add(pnlLateral);
            Controls.Add(barraEstado);

            Shown += delegate
            {
                VerificarConexion();
                IrA("Inicio");
            };
        }

        // ------------------------------------------------------------------
        // Construcción
        // ------------------------------------------------------------------
        private void ConstruirMenuLateral()
        {
            pnlLateral.Dock = DockStyle.Left;
            pnlLateral.Width = 220;
            pnlLateral.BackColor = Tema.Menu;

            // Marca del sistema
            Panel marca = new Panel();
            marca.Dock = DockStyle.Top;
            marca.Height = 84;
            Label icono = new Label();
            icono.Text = Tema.IconoLibros;
            icono.Font = new Font(Tema.FamiliaIconos, 20f);
            icono.ForeColor = Color.White;
            icono.AutoSize = true;
            icono.Location = new Point(18, 26);
            Label nombre = new Label();
            nombre.Text = "Biblioteca";
            nombre.Font = new Font(Tema.Familia, 14f, FontStyle.Bold);
            nombre.ForeColor = Color.White;
            nombre.AutoSize = true;
            nombre.Location = new Point(58, 20);
            Label lema = new Label();
            lema.Text = "Sistema de gestión";
            lema.Font = new Font(Tema.Familia, 8.5f);
            lema.ForeColor = Tema.MenuTextoSuave;
            lema.AutoSize = true;
            lema.Location = new Point(60, 46);
            marca.Controls.Add(icono);
            marca.Controls.Add(nombre);
            marca.Controls.Add(lema);

            pnlMenu.Dock = DockStyle.Fill;
            pnlMenu.FlowDirection = FlowDirection.TopDown;
            pnlMenu.WrapContents = false;
            pnlMenu.Padding = new Padding(0, 8, 0, 0);

            Label pie = new Label();
            pie.Dock = DockStyle.Bottom;
            pie.Height = 40;
            pie.Text = "ACA Final · Programación Avanzada";
            pie.Font = new Font(Tema.Familia, 8f);
            pie.ForeColor = Tema.MenuTextoSuave;
            pie.TextAlign = ContentAlignment.MiddleCenter;

            pnlLateral.Controls.Add(pnlMenu);
            pnlLateral.Controls.Add(pie);
            pnlLateral.Controls.Add(marca);
        }

        private void ConstruirEncabezado()
        {
            pnlEncabezado.Dock = DockStyle.Top;
            pnlEncabezado.Height = 72;
            pnlEncabezado.BackColor = Tema.Fondo;
            pnlEncabezado.Padding = new Padding(24, 12, 0, 0);
            // Línea inferior que separa el encabezado del contenido
            pnlEncabezado.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(Tema.Borde))
                    e.Graphics.DrawLine(p, 0, pnlEncabezado.Height - 1, pnlEncabezado.Width, pnlEncabezado.Height - 1);
            };

            lblModulo.Font = Tema.Titulo;
            lblModulo.ForeColor = Tema.Texto;
            lblModulo.AutoSize = true;
            lblModulo.Location = new Point(22, 10);

            lblDescripcion.Font = Tema.Subtitulo;
            lblDescripcion.ForeColor = Tema.TextoSuave;
            lblDescripcion.AutoSize = true;
            lblDescripcion.Location = new Point(24, 42);

            pnlEncabezado.Controls.Add(lblModulo);
            pnlEncabezado.Controls.Add(lblDescripcion);
        }

        private void ConstruirBarraEstado()
        {
            barraEstado.BackColor = Tema.FondoSuave;
            barraEstado.SizingGrip = false;
            lblConexion.Spring = true;
            lblConexion.TextAlign = ContentAlignment.MiddleLeft;
            barraEstado.Items.Add(lblConexion);
            barraEstado.Items.Add(lblFecha);

            ActualizarFecha();
            reloj.Interval = 30000;
            reloj.Tick += delegate { ActualizarFecha(); };
            reloj.Start();
            FormClosed += delegate { reloj.Stop(); };
        }

        private void ActualizarFecha()
        {
            CultureInfo es = new CultureInfo("es-CO");
            string texto = DateTime.Now.ToString("dddd d 'de' MMMM 'de' yyyy · HH:mm", es);
            lblFecha.Text = char.ToUpper(texto[0]) + texto.Substring(1);
        }

        private void AgregarModulo(string nombre, string icono, string titulo, string descripcion, Func<Form> fabrica)
        {
            Modulo m = new Modulo();
            m.Nombre = nombre;
            m.Icono = icono;
            m.Titulo = titulo;
            m.Descripcion = descripcion;
            m.Fabrica = fabrica;

            Button boton = new Button();
            boton.Text = nombre;                         // también es el nombre accesible del botón
            boton.Size = new Size(pnlLateral.Width, 44);
            boton.Margin = new Padding(0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Tema.MenuHover;
            boton.FlatAppearance.MouseDownBackColor = Tema.MenuActivo;
            boton.BackColor = Tema.Menu;
            boton.ForeColor = Tema.MenuTexto;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Padding = new Padding(52, 0, 0, 0);
            boton.Font = new Font(Tema.Familia, 10.5f);
            boton.Cursor = Cursors.Hand;
            boton.Click += delegate { IrA(nombre); };
            // Icono y barra del módulo activo, dibujados sobre el botón
            boton.Paint += delegate(object s, PaintEventArgs e)
            {
                bool activo = moduloActivo == m;
                if (activo)
                {
                    using (SolidBrush barra = new SolidBrush(Color.FromArgb(80, 160, 255)))
                        e.Graphics.FillRectangle(barra, 0, 8, 4, boton.Height - 16);
                }
                using (Font fuenteIcono = new Font(Tema.FamiliaIconos, 12f))
                {
                    TextRenderer.DrawText(e.Graphics, m.Icono, fuenteIcono,
                        new Rectangle(18, 0, 28, boton.Height),
                        activo ? Color.White : Tema.MenuTextoSuave,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            };
            m.Boton = boton;
            modulos[nombre] = m;
            pnlMenu.Controls.Add(boton);
        }

        // ------------------------------------------------------------------
        // Navegación
        // ------------------------------------------------------------------

        /// <summary>Abre un módulo por su nombre y devuelve su formulario (p. ej. desde las tarjetas de Inicio).</summary>
        public Form IrA(string nombre)
        {
            Modulo m;
            if (!modulos.TryGetValue(nombre, out m)) return null;

            if (formularioActual != null)
            {
                pnlContenido.Controls.Remove(formularioActual);
                formularioActual.Dispose();
            }

            Modulo anterior = moduloActivo;
            moduloActivo = m;
            if (anterior != null)
            {
                anterior.Boton.BackColor = Tema.Menu;
                anterior.Boton.ForeColor = Tema.MenuTexto;
                anterior.Boton.Font = new Font(Tema.Familia, 10.5f);
                anterior.Boton.Invalidate();
            }
            m.Boton.BackColor = Tema.MenuActivo;
            m.Boton.ForeColor = Color.White;
            m.Boton.Font = new Font(Tema.Familia, 10.5f, FontStyle.Bold);
            m.Boton.Invalidate();

            lblModulo.Text = m.Titulo;
            lblDescripcion.Text = m.Descripcion;

            Form formulario = m.Fabrica();
            formularioActual = formulario;
            pnlContenido.Controls.Add(formulario);
            formulario.Show();
            return formulario;
        }

        private void VerificarConexion()
        {
            string mensaje;
            bool ok = new ConexionBD().ProbarConexion(out mensaje);
            lblConexion.Text = "●  " + mensaje;
            lblConexion.ForeColor = ok ? Tema.Exito : Tema.Peligro;
            if (!ok)
                Mensajes.Advertencia("No se pudo conectar con SQL Server.\n\n" + mensaje +
                    "\n\nEjecute sql/Script_Biblioteca_Completo.sql y revise la cadena de conexión en App.config.");
        }
    }
}
