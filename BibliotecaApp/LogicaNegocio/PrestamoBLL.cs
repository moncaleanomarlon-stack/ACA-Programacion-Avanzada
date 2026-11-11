using System;
using System.Data;
using BibliotecaApp.AccesoDatos;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.LogicaNegocio
{
    public class PrestamoBLL
    {
        /// <summary>Regla de negocio: máximo de préstamos activos simultáneos por usuario.</summary>
        public const int MaxPrestamosActivosPorUsuario = 3;

        private readonly PrestamoDAL dal = new PrestamoDAL();

        public static readonly string[] Estados = new string[]
        {
            PrestamoDAL.FiltroTodos, Prestamo.EstadoPrestado, Prestamo.EstadoDevuelto
        };

        public DataTable Listar(string estado, string texto) { return dal.Listar(estado, texto); }

        public int RegistrarPrestamo(int usuarioId, int libroId, DateTime fechaPrestamo)
        {
            if (usuarioId <= 0) throw new ReglaNegocioException("Debe seleccionar un usuario.");
            if (libroId <= 0) throw new ReglaNegocioException("Debe seleccionar un libro.");
            if (fechaPrestamo > DateTime.Now.AddMinutes(1))
                throw new ReglaNegocioException("La fecha del préstamo no puede ser futura.");

            if (dal.ExistePrestamoActivo(usuarioId, libroId))
                throw new ReglaNegocioException(
                    "El usuario ya tiene prestado este libro y aún no lo ha devuelto.");
            if (dal.ContarActivosDeUsuario(usuarioId) >= MaxPrestamosActivosPorUsuario)
                throw new ReglaNegocioException(
                    "El usuario alcanzó el máximo de " + MaxPrestamosActivosPorUsuario +
                    " préstamos activos. Debe devolver un libro antes de solicitar otro.");

            // Las comprobaciones anteriores dan un aviso rápido; el DAL las repite dentro
            // de la transacción (con bloqueo) junto con usuario, libro y existencias,
            // para que dos préstamos simultáneos no puedan saltarse las reglas.
            return dal.RegistrarPrestamo(usuarioId, libroId, fechaPrestamo, MaxPrestamosActivosPorUsuario);
        }

        public void RegistrarDevolucion(int prestamoId, DateTime fechaDevolucion)
        {
            if (prestamoId <= 0) throw new ReglaNegocioException("Debe seleccionar un préstamo.");
            if (fechaDevolucion > DateTime.Now.AddMinutes(1))
                throw new ReglaNegocioException("La fecha de devolución no puede ser futura.");
            dal.RegistrarDevolucion(prestamoId, fechaDevolucion);
        }

        public void ActualizarFechas(int prestamoId, DateTime fechaPrestamo, DateTime? fechaDevolucion)
        {
            if (prestamoId <= 0) throw new ReglaNegocioException("Debe seleccionar un préstamo.");
            if (fechaPrestamo > DateTime.Now.AddMinutes(1))
                throw new ReglaNegocioException("La fecha del préstamo no puede ser futura.");
            if (fechaDevolucion.HasValue)
            {
                if (fechaDevolucion.Value < fechaPrestamo)
                    throw new ReglaNegocioException(
                        "La fecha de devolución no puede ser anterior a la fecha del préstamo.");
                if (fechaDevolucion.Value > DateTime.Now.AddMinutes(1))
                    throw new ReglaNegocioException("La fecha de devolución no puede ser futura.");
            }
            dal.ActualizarFechas(prestamoId, fechaPrestamo, fechaDevolucion);
        }

        public void Eliminar(int prestamoId)
        {
            if (prestamoId <= 0) throw new ReglaNegocioException("Debe seleccionar un préstamo.");
            dal.Eliminar(prestamoId);
        }
    }
}
