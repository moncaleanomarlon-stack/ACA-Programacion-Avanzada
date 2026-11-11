using System.Data;
using BibliotecaApp.AccesoDatos;

namespace BibliotecaApp.LogicaNegocio
{
    public class ReporteBLL
    {
        private readonly ReporteDAL dal = new ReporteDAL();

        public string[] NombresReportes { get { return ReporteDAL.Nombres; } }

        public DataTable Generar(string nombreReporte) { return dal.Obtener(nombreReporte); }

        public DataTable Resumen() { return dal.Obtener(ReporteDAL.Nombres[0]); }
    }
}
