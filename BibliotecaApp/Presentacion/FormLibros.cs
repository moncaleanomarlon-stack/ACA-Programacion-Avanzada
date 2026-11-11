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

        protected override void Actualizar()
        {
            libroBll.Actualizar(LeerFormulario());
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
            cboAutor.SelectedIndex = -1;
            cboEditorial.SelectedIndex = -1;
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
            AsignarNumero(numDisponibles, CeldaEntera(fila, "Disponibles"));
        }
    }
}
