using System;
using System.Windows.Forms;
using BibliotecaApp.Entidades;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Presentacion
{
    public class FormLibros : FormCrudBase
    {
        private readonly LibroBLL libroBll = new LibroBLL();
        private readonly AutorBLL autorBll = new AutorBLL();
        private readonly EditorialBLL editorialBll = new EditorialBLL();

        private readonly TextBox txtIsbn;
        private readonly TextBox txtTitulo;
        private readonly ComboBox cboAutor;
        private readonly ComboBox cboEditorial;
        private readonly NumericUpDown numAnio;
        private readonly NumericUpDown numDisponibles;
        private readonly ComboBox cboCriterio;

        /// <summary>Existencias que tenía el libro al seleccionarlo (para detectar cambios).</summary>
        private int stockCargado;

        public FormLibros() : base("Libros")
        {
            txtIsbn = AgregarTexto("ISBN (código):", 20);
            txtTitulo = AgregarTexto("Título:", 150);
            cboAutor = AgregarLista("Autor:");
            cboEditorial = AgregarLista("Editorial:");

            numAnio = new NumericUpDown();
            numAnio.Minimum = 1001;
            numAnio.Maximum = DateTime.Now.Year;
            numAnio.Value = DateTime.Now.Year;
            AgregarCampo("Año de publicación:", numAnio);

            numDisponibles = new NumericUpDown();
            numDisponibles.Minimum = 0;
            numDisponibles.Maximum = 100000;
            AgregarCampo("Ejemplares disponibles:", numDisponibles);

            cboCriterio = AgregarCriterioBusqueda(LibroBLL.Criterios);
        }

        protected override void PrepararControles()
        {
            cboAutor.DataSource = autorBll.ListarParaCombo();
            cboAutor.DisplayMember = "NombreCompleto";
            cboAutor.ValueMember = "AutorID";

            cboEditorial.DataSource = editorialBll.ListarParaCombo();
            cboEditorial.DisplayMember = "Nombre";
            cboEditorial.ValueMember = "EditorialID";
        }

        private Libro LeerFormulario()
        {
            Libro libro = new Libro();
            libro.LibroID = idSeleccionado;
            libro.ISBN = txtIsbn.Text;
            libro.Titulo = txtTitulo.Text;
            libro.AutorID = cboAutor.SelectedValue == null ? 0 : Convert.ToInt32(cboAutor.SelectedValue);
            libro.EditorialID = cboEditorial.SelectedValue == null ? 0 : Convert.ToInt32(cboEditorial.SelectedValue);
            libro.AnioPublicacion = (int)numAnio.Value;
            libro.Stock = (int)numDisponibles.Value;
            return libro;
        }

        protected override void Refrescar()
        {
            MostrarTabla(libroBll.Buscar(cboCriterio.SelectedItem.ToString(), txtBuscar.Text),
                "LibroID", "AutorID", "EditorialID");
        }

        protected override void Guardar()
        {
            libroBll.Registrar(LeerFormulario());
            Informar("Libro registrado correctamente.");
        }

        /// <summary>
        /// "Ejemplares disponibles" ya descuenta los prestados. Si el libro tiene
        /// préstamos activos y se cambia ese valor, se explica y se pide confirmación
        /// para no inflar o reducir el inventario por error.
        /// </summary>
        protected override bool ConfirmarActualizacion()
        {
            int nuevo = (int)numDisponibles.Value;
            if (nuevo == stockCargado) return true;

            int activos = libroBll.ContarPrestamosActivos(idSeleccionado);
            if (activos == 0) return true;

            return Mensajes.Confirmar(
                "Este libro tiene " + activos + " ejemplar(es) prestado(s).\n" +
                "\"Ejemplares disponibles\" cuenta solo los que están en la biblioteca: " +
                "los prestados ya están descontados y volverán a sumarse al devolverlos.\n\n" +
                "¿Cambiar los disponibles de " + stockCargado + " a " + nuevo + "?");
        }

        protected override void Actualizar()
        {
            libroBll.Actualizar(LeerFormulario(), stockCargado);
            Informar("Libro actualizado correctamente.");
        }

        protected override void Eliminar()
        {
            libroBll.Eliminar(idSeleccionado);
            Informar("Libro eliminado correctamente.");
        }

        protected override void LimpiarCampos()
        {
            txtIsbn.Clear();
            txtTitulo.Clear();
            QuitarSeleccion(cboAutor);
            QuitarSeleccion(cboEditorial);
            AsignarNumero(numAnio, DateTime.Now.Year);
            AsignarNumero(numDisponibles, 1);
            txtIsbn.Focus();
        }

        protected override void MostrarFila(DataGridViewRow fila)
        {
            idSeleccionado = CeldaEntera(fila, "LibroID");
            txtIsbn.Text = Celda(fila, "ISBN");
            txtTitulo.Text = Celda(fila, "Titulo");
            cboAutor.SelectedValue = CeldaEntera(fila, "AutorID");
            cboEditorial.SelectedValue = CeldaEntera(fila, "EditorialID");
            AsignarNumero(numAnio, CeldaEntera(fila, "AnioPublicacion"));
            stockCargado = CeldaEntera(fila, "Disponibles");
            AsignarNumero(numDisponibles, stockCargado);
        }
    }
}
