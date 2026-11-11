using System.Windows.Forms;
using BibliotecaApp.Entidades;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Presentacion
{
    public class FormEditoriales : FormCrudBase
    {
        private readonly EditorialBLL editorialBll = new EditorialBLL();
        private readonly TextBox txtNombre;
        private readonly TextBox txtTelefono;
        private readonly TextBox txtEmail;

        public FormEditoriales() : base("Editoriales")
        {
            txtNombre = AgregarTexto("Nombre:", 100);
            txtTelefono = AgregarTexto("Teléfono:", 20);
            txtEmail = AgregarTexto("Correo electrónico:", 100);
        }

        private Editorial LeerFormulario()
        {
            return new Editorial(idSeleccionado, txtNombre.Text, txtTelefono.Text, txtEmail.Text);
        }

        protected override void Refrescar()
        {
            MostrarTabla(editorialBll.Listar(txtBuscar.Text), "EditorialID");
        }

        protected override void Guardar()
        {
            editorialBll.Registrar(LeerFormulario());
            Informar("Editorial registrada correctamente.");
        }

        protected override void Actualizar()
        {
            editorialBll.Actualizar(LeerFormulario());
            Informar("Editorial actualizada correctamente.");
        }

        protected override void Eliminar()
        {
            editorialBll.Eliminar(idSeleccionado);
            Informar("Editorial eliminada correctamente.");
        }

        protected override void LimpiarCampos()
        {
            txtNombre.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtNombre.Focus();
        }

        protected override void MostrarFila(DataGridViewRow fila)
        {
            idSeleccionado = CeldaEntera(fila, "EditorialID");
            txtNombre.Text = Celda(fila, "Nombre");
            txtTelefono.Text = Celda(fila, "Telefono");
            txtEmail.Text = Celda(fila, "Email");
        }
    }
}
