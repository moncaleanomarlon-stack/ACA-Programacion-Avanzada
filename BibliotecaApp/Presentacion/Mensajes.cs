using System.Windows.Forms;

namespace BibliotecaApp.Presentacion
{
    /// <summary>Mensajes estándar de confirmación, advertencia y error.</summary>
    public static class Mensajes
    {
        private const string Titulo = "Sistema de Biblioteca";

        public static void Informacion(string mensaje)
        {
            MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Advertencia(string mensaje)
        {
            MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(string mensaje)
        {
            MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Confirmar(string pregunta)
        {
            return MessageBox.Show(pregunta, Titulo, MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }
    }
}
