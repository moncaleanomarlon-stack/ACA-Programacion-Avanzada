using System;
using System.Drawing;
using System.Windows.Forms;
using BibliotecaApp.Entidades;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Presentacion
{
    /// <summary>
    /// Módulo de préstamos y devoluciones. Reutiliza la estructura de FormCrudBase:
    /// "Guardar" registra un préstamo, "Actualizar" edita las fechas y se agrega el
    /// botón "Registrar devolución".
    /// </summary>
    public class FormPrestamos : FormCrudBase
    {
        private readonly PrestamoBLL prestamoBll = new PrestamoBLL();
        private readonly UsuarioBLL usuarioBll = new UsuarioBLL();
        private readonly LibroBLL libroBll = new LibroBLL();

        private readonly ComboBox cboUsuario;
        private readonly ComboBox cboLibro;
        private readonly DateTimePicker dtpFechaPrestamo;
        private readonly DateTimePicker dtpFechaDevolucion;
        private readonly Label lblEstadoActual;
        private readonly ComboBox cboFiltroEstado;
        private Button btnDevolver;
        private string estadoSeleccionado;

        public FormPrestamos() : base("Préstamos y devoluciones")
        {
            cboUsuario = AgregarLista("Usuario:");
            cboLibro = AgregarLista("Libro:");

            dtpFechaPrestamo = new DateTimePicker();
            dtpFechaPrestamo.Format = DateTimePickerFormat.Custom;
            dtpFechaPrestamo.CustomFormat = "dd/MM/yyyy HH:mm";
            AgregarCampo("Fecha de préstamo:", dtpFechaPrestamo);

            dtpFechaDevolucion = new DateTimePicker();
            dtpFechaDevolucion.Format = DateTimePickerFormat.Custom;
            dtpFechaDevolucion.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpFechaDevolucion.ShowCheckBox = true;
            AgregarCampo("Fecha de devolución:", dtpFechaDevolucion);

            lblEstadoActual = new Label();
            lblEstadoActual.AutoSize = true;
            lblEstadoActual.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            AgregarCampo("Estado:", lblEstadoActual);

            btnGuardar.Text = "Registrar préstamo";
            btnGuardar.Width = 150;
            btnActualizar.Text = "Actualizar fechas";
            btnActualizar.Width = 130;
            btnDevolver = AgregarBotonExtra("Registrar devolución", Color.FromArgb(202, 80, 16), AlPulsarDevolver);
            btnDevolver.Width = 150;

            cboFiltroEstado = AgregarCriterioBusqueda(PrestamoBLL.Estados);
        }

        protected override string MensajeConfirmarEliminar
        {
            get
            {
                return "¿Eliminar el préstamo seleccionado?\n" +
                       "Si el préstamo está activo, la unidad volverá al stock del libro.";
            }
        }

        private void CargarCombos()
        {
            cboUsuario.DataSource = usuarioBll.ListarParaCombo();
            cboUsuario.DisplayMember = "Descripcion";
            cboUsuario.ValueMember = "UsuarioID";

            cboLibro.DataSource = libroBll.ListarParaCombo();
            cboLibro.DisplayMember = "Descripcion";
            cboLibro.ValueMember = "LibroID";

            cboUsuario.SelectedIndex = -1;
            cboLibro.SelectedIndex = -1;
        }

        protected override void PrepararControles()
        {
            CargarCombos();
        }

        protected override void DespuesDeOperacion()
        {
            CargarCombos();   // refresca las existencias mostradas en la lista de libros
        }

        protected override void Refrescar()
        {
            MostrarTabla(prestamoBll.Listar(cboFiltroEstado.SelectedItem.ToString(), txtBuscar.Text),
                "UsuarioID", "LibroID");
            if (dgvRegistros.Columns.Contains("FechaPrestamo"))
                dgvRegistros.Columns["FechaPrestamo"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (dgvRegistros.Columns.Contains("FechaDevolucion"))
                dgvRegistros.Columns["FechaDevolucion"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        protected override void Guardar()
        {
            int usuarioId = Validador.Seleccion(cboUsuario.SelectedValue, "Usuario");
            int libroId = Validador.Seleccion(cboLibro.SelectedValue, "Libro");
            prestamoBll.RegistrarPrestamo(usuarioId, libroId, dtpFechaPrestamo.Value);
            Informar("Préstamo registrado correctamente. Se descontó una unidad del stock.");
        }

        protected override void Actualizar()
        {
            DateTime? devolucion = null;
            if (dtpFechaDevolucion.Enabled && dtpFechaDevolucion.Checked)
                devolucion = dtpFechaDevolucion.Value;
            prestamoBll.ActualizarFechas(idSeleccionado, dtpFechaPrestamo.Value, devolucion);
            Informar("Fechas del préstamo actualizadas correctamente.");
        }

        protected override void Eliminar()
        {
            prestamoBll.Eliminar(idSeleccionado);
            Informar("Préstamo eliminado correctamente.");
        }

        private void AlPulsarDevolver(object sender, EventArgs e)
        {
            Ejecutar(delegate
            {
                if (idSeleccionado == 0)
                    throw new ReglaNegocioException("Seleccione un préstamo activo de la tabla.");
                if (!Mensajes.Confirmar("¿Registrar la devolución del libro seleccionado?")) return;
                prestamoBll.RegistrarDevolucion(idSeleccionado, DateTime.Now);
                Informar("Devolución registrada. El préstamo quedó 'Devuelto' y el stock fue actualizado.");
                FinalizarOperacion();
            });
        }

        protected override void LimpiarCampos()
        {
            estadoSeleccionado = null;
            cboUsuario.Enabled = true;
            cboLibro.Enabled = true;
            cboUsuario.SelectedIndex = -1;
            cboLibro.SelectedIndex = -1;
            dtpFechaPrestamo.Value = DateTime.Now;
            dtpFechaDevolucion.Checked = false;
            dtpFechaDevolucion.Enabled = false;
            lblEstadoActual.Text = "(préstamo nuevo)";
        }

        protected override void MostrarFila(DataGridViewRow fila)
        {
            idSeleccionado = CeldaEntera(fila, "PrestamoID");
            estadoSeleccionado = Celda(fila, "Estado");
            bool devuelto = estadoSeleccionado == Prestamo.EstadoDevuelto;

            // El usuario y el libro de un préstamo existente no se cambian (evita
            // inconsistencias de stock); para corregirlos se elimina y se registra de nuevo.
            cboUsuario.SelectedValue = CeldaEntera(fila, "UsuarioID");
            cboLibro.SelectedValue = CeldaEntera(fila, "LibroID");
            cboUsuario.Enabled = false;
            cboLibro.Enabled = false;

            dtpFechaPrestamo.Value = (DateTime)fila.Cells["FechaPrestamo"].Value;
            dtpFechaDevolucion.Enabled = devuelto;
            if (devuelto)
            {
                dtpFechaDevolucion.Value = (DateTime)fila.Cells["FechaDevolucion"].Value;
                dtpFechaDevolucion.Checked = true;
            }
            else
            {
                dtpFechaDevolucion.Checked = false;
            }
            lblEstadoActual.Text = estadoSeleccionado;
        }

        protected override void AlCambiarSeleccion()
        {
            if (btnDevolver != null)
                btnDevolver.Enabled = idSeleccionado != 0 && estadoSeleccionado == Prestamo.EstadoPrestado;
        }
    }
}
