using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Presentacion
{
    /// <summary>Pantalla de bienvenida con indicadores generales de la biblioteca.</summary>
    public class FormInicio : Form
    {
        private readonly ReporteBLL reporteBll = new ReporteBLL();
        private readonly FlowLayoutPanel pnlIndicadores = new FlowLayoutPanel();
        private readonly Label lblMensaje = new Label();

        public FormInicio()
        {
            Text = "Inicio";
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 10f);
            BackColor = Color.White;

            Label titulo = new Label();
            titulo.Text = "Bienvenido al Sistema de Gestión de Biblioteca";
            titulo.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            titulo.AutoSize = true;
            titulo.Location = new Point(24, 24);

            lblMensaje.Text = "Use el menú de la izquierda para administrar libros, usuarios, autores, " +
                              "editoriales, préstamos y consultar reportes.";
            lblMensaje.AutoSize = true;
            lblMensaje.MaximumSize = new Size(900, 0);
            lblMensaje.Location = new Point(26, 70);

            pnlIndicadores.Location = new Point(24, 120);
            pnlIndicadores.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlIndicadores.Size = new Size(900, 400);
            pnlIndicadores.WrapContents = true;

            Controls.Add(titulo);
            Controls.Add(lblMensaje);
            Controls.Add(pnlIndicadores);

            Load += delegate { CargarIndicadores(); };
        }

        private void CargarIndicadores()
        {
            try
            {
                DataTable resumen = reporteBll.Resumen();
                pnlIndicadores.Controls.Clear();
                foreach (DataRow fila in resumen.Rows)
                    pnlIndicadores.Controls.Add(CrearTarjeta(fila["Concepto"].ToString(), fila["Total"].ToString()));
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "No fue posible cargar los indicadores: " + ex.Message;
                lblMensaje.ForeColor = Color.Firebrick;
            }
        }

        private static Panel CrearTarjeta(string concepto, string valor)
        {
            Panel tarjeta = new Panel();
            tarjeta.Size = new Size(200, 96);
            tarjeta.Margin = new Padding(8);
            tarjeta.BackColor = Color.FromArgb(0, 120, 212);

            Label lblValor = new Label();
            lblValor.Text = valor;
            lblValor.ForeColor = Color.White;
            lblValor.Font = new Font("Segoe UI", 24f, FontStyle.Bold);
            lblValor.AutoSize = true;
            lblValor.Location = new Point(12, 8);

            Label lblConcepto = new Label();
            lblConcepto.Text = concepto;
            lblConcepto.ForeColor = Color.White;
            lblConcepto.AutoSize = true;
            lblConcepto.Location = new Point(14, 66);

            tarjeta.Controls.Add(lblValor);
            tarjeta.Controls.Add(lblConcepto);
            return tarjeta;
        }
    }
}
