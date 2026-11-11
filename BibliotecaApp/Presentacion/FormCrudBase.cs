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
        protected static readonly Color ColorPrimario = Color.FromArgb(0, 120, 212);
        protected static readonly Color ColorPeligro = Color.FromArgb(196, 43, 28);

        protected readonly GroupBox grpDatos = new GroupBox();
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
            Font = new Font("Segoe UI", 9.5f);
            BackColor = Color.White;

            ConstruirBusqueda();
            ConstruirTabla();

            btnLimpiar = CrearBoton("Nuevo / Limpiar", Color.FromArgb(96, 106, 118));
            btnGuardar = CrearBoton("Guardar", ColorPrimario);
            btnActualizar = CrearBoton("Actualizar", Color.FromArgb(16, 124, 16));
            btnEliminar = CrearBoton("Eliminar", ColorPeligro);
            ConstruirPanelDatos(titulo);

            // Orden de adición: el control Fill primero, los acoplados a los bordes después.
            Controls.Add(dgvRegistros);
            Controls.Add(lblTotal);
            Controls.Add(flpBusqueda);
            Controls.Add(grpDatos);

            btnLimpiar.Click += delegate { Limpiar(); };
            btnGuardar.Click += delegate { Ejecutar(delegate { Guardar(); FinalizarOperacion(); }); };
            btnActualizar.Click += delegate { Ejecutar(delegate { Actualizar(); FinalizarOperacion(); }); };
            btnEliminar.Click += AlPulsarEliminar;
            btnBuscar.Click += delegate { Buscar(); };
            btnMostrarTodos.Click += delegate { txtBuscar.Clear(); Buscar(); };
            txtBuscar.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Buscar(); }
            };
            dgvRegistros.SelectionChanged += AlCambiarSeleccionTabla;
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
            b.Size = new Size(118, 34);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = color;
            b.ForeColor = Color.White;
            b.Margin = new Padding(3, 3, 3, 6);
            b.Cursor = Cursors.Hand;
            return b;
        }

        private void ConstruirBusqueda()
        {
            flpBusqueda.Dock = DockStyle.Top;
            flpBusqueda.Height = 48;
            flpBusqueda.Padding = new Padding(8, 8, 8, 0);
            flpBusqueda.WrapContents = false;
            flpBusqueda.BackColor = Color.FromArgb(243, 243, 243);

            lblBuscar.Text = "Buscar:";
            lblBuscar.AutoSize = true;
            lblBuscar.Margin = new Padding(3, 7, 3, 0);

            txtBuscar.Width = 220;
            txtBuscar.Margin = new Padding(3, 4, 3, 0);

            btnBuscar.Text = "Buscar";
            btnBuscar.Size = new Size(80, 28);
            btnBuscar.Margin = new Padding(3, 2, 3, 0);

            btnMostrarTodos.Text = "Mostrar todos";
            btnMostrarTodos.Size = new Size(110, 28);
            btnMostrarTodos.Margin = new Padding(3, 2, 3, 0);

            flpBusqueda.Controls.Add(lblBuscar);
            flpBusqueda.Controls.Add(txtBuscar);
            flpBusqueda.Controls.Add(btnBuscar);
            flpBusqueda.Controls.Add(btnMostrarTodos);

            lblTotal.Dock = DockStyle.Bottom;
            lblTotal.Height = 26;
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
        }

        private void ConstruirTabla()
        {
            dgvRegistros.Dock = DockStyle.Fill;
            dgvRegistros.ReadOnly = true;
            dgvRegistros.AllowUserToAddRows = false;
            dgvRegistros.AllowUserToDeleteRows = false;
            dgvRegistros.MultiSelect = false;
            dgvRegistros.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRegistros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRegistros.RowHeadersVisible = false;
            dgvRegistros.BackgroundColor = Color.White;
            dgvRegistros.BorderStyle = BorderStyle.None;
            dgvRegistros.EnableHeadersVisualStyles = false;
            dgvRegistros.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 43, 54);
            dgvRegistros.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRegistros.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
        }

        private void ConstruirPanelDatos(string titulo)
        {
            grpDatos.Text = "Datos - " + titulo;
            grpDatos.Dock = DockStyle.Left;
            grpDatos.Width = 400;
            grpDatos.Padding = new Padding(10);

            tblCampos.Dock = DockStyle.Fill;
            tblCampos.ColumnCount = 2;
            tblCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));
            tblCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64f));
            tblCampos.AutoScroll = true;

            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Height = 100;
            pnlBotones.FlowDirection = FlowDirection.LeftToRight;
            pnlBotones.WrapContents = true;
            pnlBotones.Controls.Add(btnGuardar);
            pnlBotones.Controls.Add(btnActualizar);
            pnlBotones.Controls.Add(btnEliminar);
            pnlBotones.Controls.Add(btnLimpiar);

            grpDatos.Controls.Add(tblCampos);
            grpDatos.Controls.Add(pnlBotones);
        }

        // ------------------------------------------------------------------
        // Utilidades para los formularios hijos
        // ------------------------------------------------------------------
        protected void AgregarCampo(string etiqueta, Control control)
        {
            Label lbl = new Label();
            lbl.Text = etiqueta;
            lbl.AutoSize = true;
            lbl.Anchor = AnchorStyles.Left;
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
            return cbo;
        }

        protected Button AgregarBotonExtra(string texto, Color color, EventHandler alPulsar)
        {
            Button b = CrearBoton(texto, color);
            b.Click += alPulsar;
            pnlBotones.Controls.Add(b);
            return b;
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
                lblTotal.Text = "  Registros encontrados: " + tabla.Rows.Count;
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
