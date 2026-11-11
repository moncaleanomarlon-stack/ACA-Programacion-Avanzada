using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Presentacion
{
    public class FormReportes : Form
    {
        private readonly ReporteBLL reporteBll = new ReporteBLL();
        private readonly ComboBox cboReporte = new ComboBox();
        private readonly Button btnGenerar = new Button();
        private readonly Button btnExportar = new Button();
        private readonly DataGridView dgvReporte = new DataGridView();
        private readonly Label lblTotal = new Label();

        public FormReportes()
        {
            Text = "Reportes";
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            Font = Tema.Normal;
            BackColor = Tema.Fondo;

            FlowLayoutPanel barra = new FlowLayoutPanel();
            barra.Dock = DockStyle.Top;
            barra.Height = 54;
            barra.Padding = new Padding(12, 12, 8, 0);
            barra.BackColor = Tema.Fondo;

            Label lbl = new Label();
            lbl.Text = "Reporte:";
            lbl.AutoSize = true;
            lbl.ForeColor = Tema.TextoSuave;
            lbl.Margin = new Padding(3, 7, 3, 0);

            cboReporte.DropDownStyle = ComboBoxStyle.DropDownList;
            cboReporte.Width = 320;
            cboReporte.Margin = new Padding(3, 4, 3, 0);
            cboReporte.Items.AddRange(reporteBll.NombresReportes);
            cboReporte.SelectedIndex = 0;

            btnGenerar.Text = "Generar";
            btnGenerar.Size = new Size(90, 29);
            btnGenerar.Margin = new Padding(3, 3, 3, 0);
            Tema.EstilizarBotonSecundario(btnGenerar);

            btnExportar.Text = "Exportar a CSV";
            btnExportar.Size = new Size(130, 29);
            btnExportar.Margin = new Padding(3, 3, 3, 0);
            Tema.EstilizarBoton(btnExportar, Tema.Exito);

            barra.Controls.Add(lbl);
            barra.Controls.Add(cboReporte);
            barra.Controls.Add(btnGenerar);
            barra.Controls.Add(btnExportar);

            dgvReporte.Dock = DockStyle.Fill;
            dgvReporte.ReadOnly = true;
            dgvReporte.AllowUserToAddRows = false;
            dgvReporte.AllowUserToDeleteRows = false;
            dgvReporte.AllowUserToResizeRows = false;
            dgvReporte.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReporte.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Tema.EstilizarGrilla(dgvReporte);   // también aplica títulos legibles y formato de fechas

            lblTotal.Dock = DockStyle.Bottom;
            lblTotal.Height = 30;
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            lblTotal.ForeColor = Tema.TextoSuave;
            lblTotal.Padding = new Padding(10, 0, 0, 0);

            Controls.Add(dgvReporte);
            Controls.Add(lblTotal);
            Controls.Add(barra);

            btnGenerar.Click += delegate { Generar(); };
            btnExportar.Click += delegate { Exportar(); };
            cboReporte.SelectedIndexChanged += delegate { Generar(); };
            Load += delegate { Generar(); };
        }

        private void Generar()
        {
            try
            {
                DataTable tabla = reporteBll.Generar(cboReporte.SelectedItem.ToString());
                dgvReporte.DataSource = tabla;
                Tema.AplicarTitulos(dgvReporte);
                lblTotal.Text = "Registros: " + tabla.Rows.Count;
            }
            catch (Exception ex)
            {
                Mensajes.Error(ex.Message);
            }
        }

        private void Exportar()
        {
            DataTable tabla = dgvReporte.DataSource as DataTable;
            if (tabla == null || tabla.Rows.Count == 0)
            {
                Mensajes.Advertencia("No hay datos para exportar. Genere un reporte primero.");
                return;
            }

            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Filter = "Archivo CSV (*.csv)|*.csv";
                dialogo.FileName = "reporte_biblioteca_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
                if (dialogo.ShowDialog() != DialogResult.OK) return;

                try
                {
                    StringBuilder sb = new StringBuilder();
                    string[] encabezados = new string[tabla.Columns.Count];
                    for (int i = 0; i < tabla.Columns.Count; i++)
                        encabezados[i] = EscaparCsv(tabla.Columns[i].ColumnName);
                    sb.AppendLine(string.Join(";", encabezados));

                    foreach (DataRow fila in tabla.Rows)
                    {
                        string[] valores = new string[tabla.Columns.Count];
                        for (int i = 0; i < tabla.Columns.Count; i++)
                            valores[i] = EscaparCsv(Convert.ToString(fila[i]));
                        sb.AppendLine(string.Join(";", valores));
                    }

                    File.WriteAllText(dialogo.FileName, sb.ToString(), new UTF8Encoding(true));
                    Mensajes.Informacion("Reporte exportado correctamente.");
                }
                catch (Exception ex)
                {
                    Mensajes.Error("No se pudo guardar el archivo: " + ex.Message);
                }
            }
        }

        private static string EscaparCsv(string valor)
        {
            if (valor == null) return string.Empty;
            if (valor.IndexOfAny(new char[] { ';', '"', '\n', '\r' }) >= 0)
                return "\"" + valor.Replace("\"", "\"\"") + "\"";
            return valor;
        }
    }
}
