using System;
using System.Windows.Forms;
using BibliotecaApp.Presentacion;

namespace BibliotecaApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Último recurso: ninguna excepción no controlada cierra la app sin avisar.
            Application.ThreadException += (s, e) =>
                MessageBox.Show("Ocurrió un error inesperado:\n" + e.Exception.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Application.Run(new FormPrincipal());
        }
    }
}
