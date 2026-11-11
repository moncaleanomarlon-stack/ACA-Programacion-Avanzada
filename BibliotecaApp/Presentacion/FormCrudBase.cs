using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.Presentacion
{
    /// <summary>
    /// Formulario base para los módulos con CRUD (Libros, Autores, Editoriales,
    /// Usuarios y Préstamos). Construye la interfaz común (panel de datos a la
    /// izquierda, búsqueda arriba y tabla al centro) y centraliza el manejo de
    /// excepciones. Cada módulo hereda y solo define sus campos y operaciones
    /// (herencia + métodos abstractos = polimorfismo).
    /// </summary>
    public abstract class FormCrudBase : Form
    {
        protected static readonly Color ColorPrimario = Tema.Primario;
        protected static readonly Color ColorPeligro = Tema.Peligro;

        /// <summary>Panel izquierdo con el título, los campos y los botones de acción.</summary>
        protected readonly Panel pnlDatos = new Panel();
        protected readonly Label lblTituloDatos = new Label();
        protected readonly ToolTip ayuda = new ToolTip();
        protected readonly TableLayoutPanel tblCampos = new TableLayoutPanel();
        protected readonly FlowLayoutPanel pnlBotones = new FlowLayoutPanel();
        protected readonly FlowLayoutPanel flpBusqueda = new FlowLayoutPanel();
        protected readonly Label lblBuscar = new Label();
        protected readonly TextBox txtBuscar = new TextBox();
        protected readonly Button btnBuscar = new Button();
        protected readonly Button btnMostrarTodos = new Button();
        protected readonly DataGridView dgvRegistros = new DataGridView();
        protected readonly Label lblTotal = new Label();

        protected readonly Button btnLimpiar;
        protected readonly Button btnGuardar;
        protected readonly Button btnActualizar;
        protected readonly Button btnEliminar;

        /// <summary>Identificador del registro seleccionado (0 = ninguno / registro nuevo).</summary>
        protected int idSeleccionado;

        private bool cargando;

        protected FormCrudBase(string titulo)
        {
            Text = titulo;
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            Font = Tema.Normal;
            BackColor = Tema.Fondo;
            KeyPreview = true;   // atajos de teclado (ver ProcessCmdKey)

            ConstruirBusqueda();
            ConstruirTabla();

            btnLimpiar = CrearBoton("Nuevo / Limpiar", Tema.Neutro);
            btnGuardar = CrearBoton("Guardar", Tema.Primario);
            btnActualizar = CrearBoton("Actualizar", Tema.Exito);
            btnEliminar = CrearBoton("Eliminar", Tema.Peligro);
            ConstruirPanelDatos(titulo);

            ayuda.SetToolTip(btnGuardar, "Registrar un nuevo registro (Ctrl+G)");
            ayuda.SetToolTip(btnActualizar, "Guardar los cambios del registro seleccionado (Ctrl+G)");
            ayuda.SetToolTip(btnEliminar, "Eliminar el registro seleccionado (Supr en la tabla)");
            ayuda.SetToolTip(btnLimpiar, "Limpiar el formulario para un registro nuevo (Ctrl+N)");
            ayuda.SetToolTip(btnBuscar, "Aplicar la búsqueda (Enter)");
            ayuda.SetToolTip(btnMostrarTodos, "Quitar el texto de búsqueda y recargar (F5)");

            // Orden de adición: el control Fill primero, los acoplados a los bordes después.
            Panel separador = new Panel();
            separador.Dock = DockStyle.Left;
            separador.Width = 1;
            separador.BackColor = Tema.Borde;
            Controls.Add(dgvRegistros);
            Controls.Add(lblTotal);
            Controls.Add(flpBusqueda);
            Controls.Add(separador);
            Controls.Add(pnlDatos);

            btnLimpiar.Click += delegate { Limpiar(); };
            btnGuardar.Click += delegate { Ejecutar(delegate { Guardar(); FinalizarOperacion(); }); };
            btnActualizar.Click += delegate
            {
                Ejecutar(delegate
                {
                    if (!ConfirmarActualizacion()) return;   // el usuario sigue editando
                    Actualizar();
                    FinalizarOperacion();
                });
            };
            btnEliminar.Click += AlPulsarEliminar;
            btnBuscar.Click += delegate { Buscar(); };
            btnMostrarTodos.Click += delegate { txtBuscar.Clear(); Buscar(); };
            txtBuscar.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Buscar(); }
            };
            dgvRegistros.SelectionChanged += AlCambiarSeleccionTabla;
            // Al mostrarse por primera vez, la grilla vuelve a enlazar los datos y
            // selecciona sola la primera fila; eso dejaba el formulario en modo edición
            // y "Guardar" deshabilitado. Se vuelve al estado de registro nuevo.
            dgvRegistros.DataBindingComplete += delegate { if (!cargando) Limpiar(); };
            Load += delegate
            {
                Ejecutar(delegate { PrepararControles(); Refrescar(); Limpiar(); });
            };
        }

        // ------------------------------------------------------------------
        // Operaciones que cada módulo debe implementar
        // ------------------------------------------------------------------
        /// <summary>Carga la tabla con los registros (aplicando el texto de búsqueda).</summary>
        protected abstract void Refrescar();
        /// <summary>Valida y registra un nuevo registro con los datos del formulario.</summary>
        protected abstract void Guardar();
        /// <summary>Valida y actualiza el registro seleccionado.</summary>
        protected abstract void Actualizar();
        /// <summary>Elimina el registro seleccionado.</summary>
        protected abstract void Eliminar();
        /// <summary>Deja los campos de captura en su estado inicial.</summary>
        protected abstract void LimpiarCampos();
        /// <summary>Pasa los datos de la fila seleccionada a los campos y fija idSeleccionado.</summary>
        protected abstract void MostrarFila(DataGridViewRow fila);

        // ------------------------------------------------------------------
        // Ganchos opcionales
        // ------------------------------------------------------------------
        /// <summary>Se ejecuta una vez al cargar el formulario (p. ej. llenar ComboBox).</summary>
        protected virtual void PrepararControles() { }
        /// <summary>Se ejecuta tras una operación exitosa (p. ej. recargar existencias).</summary>
        protected virtual void DespuesDeOperacion() { }
        /// <summary>Se ejecuta cuando cambia el registro seleccionado.</summary>
        protected virtual void AlCambiarSeleccion() { }
        /// <summary>
        /// Se ejecuta antes de Actualizar. Devuelve false para cancelar sin perder lo
        /// escrito (p. ej. Libros pide confirmación al cambiar existencias).
        /// </summary>
        protected virtual bool ConfirmarActualizacion() { return true; }
        protected virtual string MensajeConfirmarEliminar
        {
            get { return "¿Está seguro de eliminar el registro seleccionado?\nEsta acción no se puede deshacer."; }
        }

        // ------------------------------------------------------------------
        // Construcción de la interfaz
        // ------------------------------------------------------------------
        private static Button CrearBoton(string texto, Color color)
        {
            Button b = new Button();
            b.Text = texto;
            b.Size = new Size(170, 36);   // dos botones por fila en el panel de datos
            b.Margin = new Padding(3, 3, 3, 6);
            Tema.EstilizarBoton(b, color);
            return b;
        }

        private void ConstruirBusqueda()
        {
            flpBusqueda.Dock = DockStyle.Top;
            flpBusqueda.Height = 54;
            flpBusqueda.Padding = new Padding(12, 12, 8, 0);
            flpBusqueda.WrapContents = false;
            flpBusqueda.BackColor = Tema.Fondo;

            lblBuscar.Text = "Buscar:";
            lblBuscar.AutoSize = true;
            lblBuscar.ForeColor = Tema.TextoSuave;
            lblBuscar.Margin = new Padding(3, 7, 3, 0);

            txtBuscar.Width = 180;
            txtBuscar.Margin = new Padding(3, 4, 3, 0);
            txtBuscar.PlaceholderText = "Escriba y pulse Enter…";

            btnBuscar.Text = "Buscar";
            btnBuscar.Size = new Size(80, 29);
            btnBuscar.Margin = new Padding(3, 3, 3, 0);
            Tema.EstilizarBotonSecundario(btnBuscar);

            btnMostrarTodos.Text = "Mostrar todos";
            btnMostrarTodos.Size = new Size(112, 29);
            btnMostrarTodos.Margin = new Padding(3, 3, 3, 0);
            Tema.EstilizarBotonSecundario(btnMostrarTodos);

            flpBusqueda.Controls.Add(lblBuscar);
            flpBusqueda.Controls.Add(txtBuscar);
            flpBusqueda.Controls.Add(btnBuscar);
            flpBusqueda.Controls.Add(btnMostrarTodos);

            lblTotal.Dock = DockStyle.Bottom;
            lblTotal.Height = 30;
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            lblTotal.ForeColor = Tema.TextoSuave;
            lblTotal.Padding = new Padding(10, 0, 0, 0);
        }

        private void ConstruirTabla()
        {
            dgvRegistros.Dock = DockStyle.Fill;
            dgvRegistros.ReadOnly = true;
            dgvRegistros.AllowUserToAddRows = false;
            dgvRegistros.AllowUserToDeleteRows = false;
            dgvRegistros.AllowUserToResizeRows = false;
            dgvRegistros.MultiSelect = false;
            dgvRegistros.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRegistros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Tema.EstilizarGrilla(dgvRegistros);
        }

        private void ConstruirPanelDatos(string titulo)
        {
            pnlDatos.Dock = DockStyle.Left;
            pnlDatos.Width = 380;
            pnlDatos.Padding = new Padding(16, 10, 12, 10);
            pnlDatos.BackColor = Tema.FondoSuave;

            lblTituloDatos.Text = "Datos del registro";
            lblTituloDatos.Dock = DockStyle.Top;
            lblTituloDatos.Height = 34;
            lblTituloDatos.Font = new Font(Tema.Familia, 11f, FontStyle.Bold);
            lblTituloDatos.ForeColor = Tema.Texto;
            lblTituloDatos.TextAlign = ContentAlignment.MiddleLeft;

            tblCampos.Dock = DockStyle.Fill;
            tblCampos.ColumnCount = 2;
            tblCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            tblCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            tblCampos.AutoScroll = true;

            // Alto automático: el panel crece si los botones pasan a otra fila
            // (FormPrestamos agrega un quinto botón que antes quedaba recortado).
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.AutoSize = true;
            pnlBotones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlBotones.FlowDirection = FlowDirection.LeftToRight;
            pnlBotones.WrapContents = true;
            pnlBotones.Controls.Add(btnGuardar);
            pnlBotones.Controls.Add(btnActualizar);
            pnlBotones.Controls.Add(btnEliminar);
            pnlBotones.Controls.Add(btnLimpiar);

            pnlDatos.Controls.Add(tblCampos);
            pnlDatos.Controls.Add(pnlBotones);
            pnlDatos.Controls.Add(lblTituloDatos);
        }

        /// <summary>
        /// Atajos: F5 recargar, Ctrl+N nuevo, Ctrl+G guardar o actualizar,
        /// Supr eliminar (solo con el foco en la tabla, para no interferir al escribir).
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    txtBuscar.Clear(); Buscar(); return true;
                case Keys.Control | Keys.N:
                    btnLimpiar.PerformClick(); return true;
                case Keys.Control | Keys.G:
                    if (btnGuardar.Enabled) btnGuardar.PerformClick();
                    else if (btnActualizar.Enabled) btnActualizar.PerformClick();
                    return true;
                case Keys.Delete:
                    if (dgvRegistros.Focused && btnEliminar.Enabled) { btnEliminar.PerformClick(); return true; }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ------------------------------------------------------------------
        // Utilidades para los formularios hijos
        // ------------------------------------------------------------------
        protected void AgregarCampo(string etiqueta, Control control)
        {
            Label lbl = new Label();
            lbl.Text = etiqueta;
            lbl.AutoSize = true;
            lbl.ForeColor = Tema.TextoSuave;
            // Arriba + izquierda: si la fila es más alta (p. ej. la última), la etiqueta
            // queda junto a su control y no centrada verticalmente.
            lbl.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lbl.Margin = new Padding(3, 10, 3, 6);

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(3, 6, 3, 6);

            tblCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            int fila = tblCampos.RowStyles.Count - 1;
            tblCampos.RowCount = tblCampos.RowStyles.Count;
            tblCampos.Controls.Add(lbl, 0, fila);
            tblCampos.Controls.Add(control, 1, fila);
        }

        protected TextBox AgregarTexto(string etiqueta, int longitudMaxima)
        {
            TextBox txt = new TextBox();
            txt.MaxLength = longitudMaxima;
            AgregarCampo(etiqueta, txt);
            return txt;
        }

        protected ComboBox AgregarLista(string etiqueta)
        {
            ComboBox cbo = new ComboBox();
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            AgregarCampo(etiqueta, cbo);
            return cbo;
        }

        /// <summary>Agrega una lista de criterios (filtro) a la barra de búsqueda.</summary>
        protected ComboBox AgregarCriterioBusqueda(string[] opciones)
        {
            ComboBox cbo = new ComboBox();
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo.Width = 140;
            cbo.Margin = new Padding(3, 4, 3, 0);
            cbo.Items.AddRange(opciones);
            cbo.SelectedIndex = 0;
            flpBusqueda.Controls.Add(cbo);
            flpBusqueda.Controls.SetChildIndex(cbo, 1);   // justo después de "Buscar:"
            // Se suscribe después de fijar el valor inicial: cambiar el criterio o el
            // filtro de estado actualiza la tabla sin tener que pulsar "Buscar".
            cbo.SelectedIndexChanged += delegate { Buscar(); };
            return cbo;
        }

        protected Button AgregarBotonExtra(string texto, Color color, EventHandler alPulsar)
        {
            Button b = CrearBoton(texto, color);
            b.Click += alPulsar;
            pnlBotones.Controls.Add(b);
            return b;
        }

        /// <summary>
        /// Deja un ComboBox enlazado a datos sin selección. Se asigna -1 dos veces
        /// porque, con DataSource, la primera asignación a veces se ignora (WinForms).
        /// </summary>
        protected static void QuitarSeleccion(ComboBox combo)
        {
            combo.SelectedIndex = -1;
            combo.SelectedIndex = -1;
        }

        protected static void AsignarNumero(NumericUpDown control, int valor)
        {
            control.Value = Math.Max(control.Minimum, Math.Min(control.Maximum, valor));
        }

        protected static string Celda(DataGridViewRow fila, string columna)
        {
            object valor = fila.Cells[columna].Value;
            return (valor == null || valor == DBNull.Value) ? string.Empty : valor.ToString();
        }

        protected static int CeldaEntera(DataGridViewRow fila, string columna)
        {
            return Convert.ToInt32(fila.Cells[columna].Value);
        }

        protected void MostrarTabla(DataTable tabla, params string[] columnasOcultas)
        {
            cargando = true;
            try
            {
                dgvRegistros.DataSource = tabla;
                foreach (string columna in columnasOcultas)
                {
                    if (dgvRegistros.Columns.Contains(columna))
                        dgvRegistros.Columns[columna].Visible = false;
                }
                lblTotal.Text = "Registros encontrados: " + tabla.Rows.Count;
                dgvRegistros.ClearSelection();
            }
            finally
            {
                cargando = false;
            }
        }

        protected void Informar(string mensaje)
        {
            Mensajes.Informacion(mensaje);
        }

        /// <summary>Ejecuta una acción mostrando mensajes claros ante cualquier error.</summary>
        protected bool Ejecutar(Action accion)
        {
            try
            {
                accion();
                return true;
            }
            catch (ReglaNegocioException ex)
            {
                Mensajes.Advertencia(ex.Message);
            }
            catch (Exception ex)
            {
                Mensajes.Error(ex.Message);
            }
            return false;
        }

        protected void FinalizarOperacion()
        {
            Refrescar();
            DespuesDeOperacion();
            Limpiar();
        }

        protected void Limpiar()
        {
            idSeleccionado = 0;
            LimpiarCampos();
            dgvRegistros.ClearSelection();
            ActualizarBotones();
        }

        private void Buscar()
        {
            Ejecutar(delegate { Refrescar(); Limpiar(); });
        }

        private void ActualizarBotones()
        {
            btnGuardar.Enabled = idSeleccionado == 0;
            btnActualizar.Enabled = idSeleccionado != 0;
            btnEliminar.Enabled = idSeleccionado != 0;
            AlCambiarSeleccion();
        }

        private void AlCambiarSeleccionTabla(object sender, EventArgs e)
        {
            if (cargando || dgvRegistros.SelectedRows.Count != 1) return;
            Ejecutar(delegate
            {
                MostrarFila(dgvRegistros.SelectedRows[0]);
                ActualizarBotones();
            });
        }

        private void AlPulsarEliminar(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                if (idSeleccionado == 0)
                    throw new ReglaNegocioException("Seleccione un registro de la tabla.");
                if (!Mensajes.Confirmar(MensajeConfirmarEliminar)) return;
                Eliminar();
                FinalizarOperacion();
            });
        }
    }
}
