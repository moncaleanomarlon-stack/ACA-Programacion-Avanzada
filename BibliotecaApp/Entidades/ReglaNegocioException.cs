using System;

namespace BibliotecaApp.Entidades
{
    /// <summary>
    /// Error esperado y entendible por el usuario (validación fallida, regla de
    /// negocio incumplida, duplicado, registro relacionado, etc.). La capa de
    /// presentación la muestra como advertencia, no como error del sistema.
    /// </summary>
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje) { }
        public ReglaNegocioException(string mensaje, Exception inner) : base(mensaje, inner) { }
    }
}
