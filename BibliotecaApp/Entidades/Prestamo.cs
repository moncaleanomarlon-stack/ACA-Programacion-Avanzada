using System;

namespace BibliotecaApp.Entidades
{
    public class Prestamo
    {
        public const string EstadoPrestado = "Prestado";
        public const string EstadoDevuelto = "Devuelto";

        public int PrestamoID { get; set; }
        public int UsuarioID { get; set; }
        public int LibroID { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string Estado { get; set; }

        public Prestamo()
        {
            FechaPrestamo = DateTime.Now;
            Estado = EstadoPrestado;
        }

        public Prestamo(int id, int usuarioId, int libroId, DateTime fechaPrestamo,
                        DateTime? fechaDevolucion, string estado)
        {
            PrestamoID = id;
            UsuarioID = usuarioId;
            LibroID = libroId;
            FechaPrestamo = fechaPrestamo;
            FechaDevolucion = fechaDevolucion;
            Estado = estado;
        }

        public bool EstaDevuelto
        {
            get { return Estado == EstadoDevuelto; }
        }
    }
}
