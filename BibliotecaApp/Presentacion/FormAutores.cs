using System.Windows.Forms;
using BibliotecaApp.Entidades;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Presentacion
{
    public class FormAutores : FormCrudBase
    {
        private readonly AutorBLL autorBll = new AutorBLL();
        private readonly TextBox txtNombre;
        private readonly TextBox txtApellido;
        private readonly TextBox txtNacionalidad;

        public FormAutores() : base("Autores")
        {
            txtNombre = AgregarTexto("Nombre:", 50);
            txtApellido = AgregarTexto("Apellido:", 50);
            txtNacionalidad = AgregarTexto("Nacionalidad:", 50);
        }

        private Autor LeerFormulario()
        {
            return new Autor(idSeleccionado, txtNombre.Text, txtApellido.Text, txtNacionalidad.Text);
        }

        protected override void Refrescar()
        {
            MostrarTabla(autorBll.Listar(txtBuscar.Text), "AutorID");
        }

        protected override void Guardar()
        {
            autorBll.Registrar(LeerFormulario());
            Informar("Autor registrado correctamente.");
        }

        protected override void Actualizar()
        {
            autorBll.Actualizar(LeerFormulario());
            Informar("Autor actualizado correctamente.");
        }

        protected override void Eliminar()
        {
            autorBll.Eliminar(idSeleccionado);
            Informar("Autor eliminado correctamente.");
        }

        protected override void LimpiarCampos()
        {
            txtNombre.Clear();
            txtApellido.Clear();
            txtNacionalidad.Clear();
            txtNombre.Focus();
        }

        protected override void MostrarFila(DataGridViewRow fila)
        {
            idSeleccionado = CeldaEntera(fila, "AutorID");
            txtNombre.Text = Celda(fila, "Nombre");
            txtApellido.Text = Celda(fila, "Apellido");
            txtNacionalidad.Text = Celda(fila, "Nacionalidad");
        }
    }
}
