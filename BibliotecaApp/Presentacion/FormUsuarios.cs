using System.Windows.Forms;
using BibliotecaApp.Entidades;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Presentacion
{
    public class FormUsuarios : FormCrudBase
    {
        private readonly UsuarioBLL usuarioBll = new UsuarioBLL();
        private readonly TextBox txtDocumento;
        private readonly TextBox txtNombre;
        private readonly TextBox txtApellido;
        private readonly TextBox txtTelefono;
        private readonly TextBox txtEmail;

        public FormUsuarios() : base("Usuarios")
        {
            lblTituloDatos.Text = "Datos del usuario";
            txtDocumento = AgregarTexto("Documento:", 20);
            txtNombre = AgregarTexto("Nombre:", 50);
            txtApellido = AgregarTexto("Apellido:", 50);
            txtTelefono = AgregarTexto("Teléfono:", 20);
            txtEmail = AgregarTexto("Correo electrónico:", 100);
        }

        private Usuario LeerFormulario()
        {
            return new Usuario(idSeleccionado, txtDocumento.Text, txtNombre.Text,
                txtApellido.Text, txtTelefono.Text, txtEmail.Text);
        }

        protected override void Refrescar()
        {
            MostrarTabla(usuarioBll.Listar(txtBuscar.Text), "UsuarioID");
        }

        protected override void Guardar()
        {
            usuarioBll.Registrar(LeerFormulario());
            Informar("Usuario registrado correctamente.");
        }

        protected override void Actualizar()
        {
            usuarioBll.Actualizar(LeerFormulario());
            Informar("Usuario actualizado correctamente.");
        }

        protected override void Eliminar()
        {
            usuarioBll.Eliminar(idSeleccionado);
            Informar("Usuario eliminado correctamente.");
        }

        protected override void LimpiarCampos()
        {
            txtDocumento.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtDocumento.Focus();
        }

        protected override void MostrarFila(DataGridViewRow fila)
        {
            idSeleccionado = CeldaEntera(fila, "UsuarioID");
            txtDocumento.Text = Celda(fila, "Documento");
            txtNombre.Text = Celda(fila, "Nombre");
            txtApellido.Text = Celda(fila, "Apellido");
            txtTelefono.Text = Celda(fila, "Telefono");
            txtEmail.Text = Celda(fila, "Email");
        }
    }
}
