using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BibliotecaApp.Presentacion
{
    /// <summary>
    /// Colores, fuentes y estilos compartidos por todos los formularios. Así la
    /// apariencia se cambia en un solo lugar y los módulos se ven iguales.
    /// </summary>
    public static class Tema
    {
        // ---- Paleta -------------------------------------------------------
        public static readonly Color Menu = Color.FromArgb(27, 36, 48);
        public static readonly Color MenuHover = Color.FromArgb(40, 52, 68);
        public static readonly Color MenuActivo = Color.FromArgb(44, 58, 78);
        public static readonly Color MenuTexto = Color.FromArgb(214, 221, 230);
        public static readonly Color MenuTextoSuave = Color.FromArgb(138, 152, 170);

        public static readonly Color Primario = Color.FromArgb(0, 103, 192);
        public static readonly Color Exito = Color.FromArgb(16, 124, 16);
        public static readonly Color Peligro = Color.FromArgb(196, 43, 28);
        public static readonly Color Aviso = Color.FromArgb(202, 80, 16);
        public static readonly Color Neutro = Color.FromArgb(96, 106, 118);

        public static readonly Color Fondo = Color.White;
        public static readonly Color FondoSuave = Color.FromArgb(245, 247, 250);
        public static readonly Color Borde = Color.FromArgb(226, 230, 236);
        public static readonly Color Texto = Color.FromArgb(32, 38, 46);
        public static readonly Color TextoSuave = Color.FromArgb(96, 106, 118);

        public static readonly Color FilaAlterna = Color.FromArgb(248, 250, 252);
        public static readonly Color Seleccion = Color.FromArgb(204, 228, 247);

        // Insignias de estado (fondo claro + texto oscuro del mismo tono)
        public static readonly Color PrestadoFondo = Color.FromArgb(255, 241, 214);
        public static readonly Color PrestadoTexto = Color.FromArgb(138, 76, 0);
        public static readonly Color DevueltoFondo = Color.FromArgb(220, 242, 222);
        public static readonly Color DevueltoTexto = Color.FromArgb(16, 92, 16);
        public static readonly Color SinStockTexto = Color.FromArgb(176, 32, 20);

        // ---- Fuentes ------------------------------------------------------
        public const string Familia = "Segoe UI";
        /// <summary>Fuente de iconos incluida en Windows 10 y 11 (sin dependencias).</summary>
        public const string FamiliaIconos = "Segoe MDL2 Assets";
        public static readonly Font Normal = new Font(Familia, 9.5f);
        public static readonly Font Negrita = new Font(Familia, 9.5f, FontStyle.Bold);
        public static readonly Font Titulo = new Font(Familia, 16f, FontStyle.Bold);
        public static readonly Font Subtitulo = new Font(Familia, 9.5f);

        // ---- Iconos (Segoe MDL2 Assets) -----------------------------------
        public const string IconoInicio = "";
        public const string IconoLibros = "";
        public const string IconoUsuarios = "";
        public const string IconoAutores = "";
        public const string IconoEditoriales = "";
        public const string IconoPrestamos = "";
        public const string IconoReportes = "";

        // ---- Títulos legibles para columnas de la base de datos -----------
        private static readonly Dictionary<string, string> TitulosColumnas =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "PrestamoID", "N.º" },
            { "Titulo", "Título" },
            { "AnioPublicacion", "Año" },
            { "Telefono", "Teléfono" },
            { "Email", "Correo" },
            { "FechaPrestamo", "Fecha préstamo" },
            { "FechaDevolucion", "Fecha devolución" },
            { "TotalPrestamos", "Total préstamos" },
            { "LibrosRegistrados", "Libros" },
            { "UsuarioID", "ID" }, { "AutorID", "ID" }, { "EditorialID", "ID" }, { "LibroID", "ID" }
        };

        // Ancho relativo de algunas columnas (las demás valen 100): los números
        // cortos ocupan menos y dejan espacio a nombres y títulos.
        private static readonly Dictionary<string, float> PesosColumnas =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
        {
            { "PrestamoID", 35 }, { "AnioPublicacion", 45 }, { "Disponibles", 60 },
            { "Estado", 70 }, { "Documento", 85 }, { "ISBN", 95 },
            { "Titulo", 150 }, { "Libro", 140 }, { "Email", 140 }
        };

        // Ancho mínimo en píxeles para que estas columnas nunca se corten
        private static readonly Dictionary<string, int> AnchosMinimos =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "PrestamoID", 45 }, { "AnioPublicacion", 50 }, { "Disponibles", 92 },
            { "Estado", 92 }, { "Documento", 95 }, { "ISBN", 122 },
            { "FechaPrestamo", 92 }, { "FechaDevolucion", 92 }
        };

        // ---- Estilos ------------------------------------------------------

        /// <summary>Botón plano de color, con efecto al pasar el ratón y aspecto claro cuando está deshabilitado.</summary>
        public static void EstilizarBoton(Button b, Color color)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Aclarar(color, 0.12f);
            b.FlatAppearance.MouseDownBackColor = Oscurecer(color, 0.12f);
            b.Font = Negrita;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            b.Tag = color;
            AplicarEstado(b);
            b.EnabledChanged += delegate { AplicarEstado(b); };
        }

        /// <summary>Botón secundario: fondo blanco con borde (Buscar, Mostrar todos...).</summary>
        public static void EstilizarBotonSecundario(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Borde;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = FondoSuave;
            b.BackColor = Fondo;
            b.ForeColor = Texto;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
        }

        // Los botones planos deshabilitados casi no se distinguían de los activos.
        private static void AplicarEstado(Button b)
        {
            Color color = b.Tag is Color ? (Color)b.Tag : Primario;
            b.BackColor = b.Enabled ? color : Color.FromArgb(232, 235, 239);
            b.ForeColor = b.Enabled ? Color.White : Color.FromArgb(150, 158, 168);
        }

        /// <summary>Grilla de solo lectura con encabezados claros, filas espaciadas y títulos legibles.</summary>
        public static void EstilizarGrilla(DataGridView g)
        {
            g.BackgroundColor = Fondo;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Borde;
            g.RowHeadersVisible = false;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersDefaultCellStyle.BackColor = FondoSuave;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextoSuave;
            g.ColumnHeadersDefaultCellStyle.Font = Negrita;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = FondoSuave;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.ForeColor = Texto;
            g.DefaultCellStyle.SelectionBackColor = Seleccion;
            g.DefaultCellStyle.SelectionForeColor = Texto;
            g.AlternatingRowsDefaultCellStyle.BackColor = FilaAlterna;
            g.RowTemplate.Height = 32;
            g.DataBindingComplete += delegate { AplicarTitulos(g); };
        }

        /// <summary>Cambia nombres técnicos (AnioPublicacion) por títulos legibles (Año).</summary>
        public static void AplicarTitulos(DataGridView g)
        {
            foreach (DataGridViewColumn c in g.Columns)
            {
                string titulo;
                if (TitulosColumnas.TryGetValue(c.DataPropertyName, out titulo))
                    c.HeaderText = titulo;
                float peso;
                if (PesosColumnas.TryGetValue(c.DataPropertyName, out peso))
                    c.FillWeight = peso;
                int minimo;
                if (AnchosMinimos.TryGetValue(c.DataPropertyName, out minimo))
                    c.MinimumWidth = minimo;
                if (c.ValueType == typeof(DateTime))
                    c.DefaultCellStyle.Format = FormatoFecha(g);
            }
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<DataGridView, string> FormatosFecha =
            new System.Runtime.CompilerServices.ConditionalWeakTable<DataGridView, string>();

        /// <summary>
        /// Formato de fecha de una grilla (por defecto con hora). Se guarda aparte porque la
        /// grilla vuelve a aplicar estilos cada vez que se enlaza (p. ej. al mostrarse).
        /// </summary>
        public static void UsarFormatoFecha(DataGridView g, string formato)
        {
            FormatosFecha.AddOrUpdate(g, formato);
        }

        private static string FormatoFecha(DataGridView g)
        {
            string formato;
            return FormatosFecha.TryGetValue(g, out formato) ? formato : "dd/MM/yyyy HH:mm";
        }

        public static Color Aclarar(Color c, float f)
        {
            return Color.FromArgb(c.A, c.R + (int)((255 - c.R) * f), c.G + (int)((255 - c.G) * f), c.B + (int)((255 - c.B) * f));
        }

        public static Color Oscurecer(Color c, float f)
        {
            return Color.FromArgb(c.A, (int)(c.R * (1 - f)), (int)(c.G * (1 - f)), (int)(c.B * (1 - f)));
        }
    }
}
