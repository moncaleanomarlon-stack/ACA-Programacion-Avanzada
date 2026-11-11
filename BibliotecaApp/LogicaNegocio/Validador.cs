using System;
using System.Text.RegularExpressions;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.LogicaNegocio
{
    /// <summary>Reglas de validación reutilizables por todas las clases de negocio.</summary>
    public static class Validador
    {
        private static readonly Regex RegexNombre =
            new Regex(@"^[\p{L}][\p{L}\s'.\-]*$", RegexOptions.Compiled);
        private static readonly Regex RegexCorreo =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled);
        private static readonly Regex RegexTelefono =
            new Regex(@"^\+?\d{7,20}$", RegexOptions.Compiled);
        private static readonly Regex RegexDocumento =
            new Regex(@"^\d{5,20}$", RegexOptions.Compiled);
        private static readonly Regex RegexIsbn =
            new Regex(@"^[0-9][0-9\-]{8,18}[0-9Xx]$", RegexOptions.Compiled);

        /// <summary>Texto obligatorio con longitud máxima. Devuelve el texto sin espacios sobrantes.</summary>
        public static string Texto(string valor, string campo, int maximo)
        {
            string limpio = (valor ?? string.Empty).Trim();
            if (limpio.Length == 0)
                throw new ReglaNegocioException("El campo '" + campo + "' es obligatorio.");
            if (limpio.Length > maximo)
                throw new ReglaNegocioException(
                    "El campo '" + campo + "' no puede superar los " + maximo + " caracteres.");
            return limpio;
        }

        public static string NombrePersona(string valor, string campo, int maximo)
        {
            string limpio = Texto(valor, campo, maximo);
            if (!RegexNombre.IsMatch(limpio))
                throw new ReglaNegocioException(
                    "El campo '" + campo + "' solo admite letras, espacios, apóstrofes, puntos y guiones.");
            return limpio;
        }

        public static string Correo(string valor, string campo, int maximo)
        {
            string limpio = Texto(valor, campo, maximo);
            if (!RegexCorreo.IsMatch(limpio))
                throw new ReglaNegocioException("El campo '" + campo + "' no tiene un formato de correo válido.");
            return limpio;
        }

        public static string Telefono(string valor, string campo)
        {
            string limpio = Texto(valor, campo, 20).Replace(" ", "");
            if (!RegexTelefono.IsMatch(limpio))
                throw new ReglaNegocioException(
                    "El campo '" + campo + "' debe contener entre 7 y 20 dígitos (puede iniciar con +).");
            return limpio;
        }

        public static string Documento(string valor, string campo)
        {
            string limpio = Texto(valor, campo, 20);
            if (!RegexDocumento.IsMatch(limpio))
                throw new ReglaNegocioException(
                    "El campo '" + campo + "' debe contener solo dígitos (entre 5 y 20).");
            return limpio;
        }

        public static string Isbn(string valor, string campo)
        {
            string limpio = Texto(valor, campo, 20);
            if (!RegexIsbn.IsMatch(limpio))
                throw new ReglaNegocioException(
                    "El campo '" + campo + "' debe tener entre 10 y 20 caracteres: dígitos y guiones " +
                    "(puede terminar en X).");
            return limpio.ToUpperInvariant();
        }

        public static int Entero(string valor, string campo, int minimo, int maximo)
        {
            int numero;
            if (!int.TryParse((valor ?? string.Empty).Trim(), out numero))
                throw new ReglaNegocioException("El campo '" + campo + "' debe ser un número entero.");
            if (numero < minimo || numero > maximo)
                throw new ReglaNegocioException(
                    "El campo '" + campo + "' debe estar entre " + minimo + " y " + maximo + ".");
            return numero;
        }

        public static int Seleccion(object valor, string campo)
        {
            int id;
            if (valor == null || !int.TryParse(valor.ToString(), out id) || id <= 0)
                throw new ReglaNegocioException("Debe seleccionar un valor en '" + campo + "'.");
            return id;
        }
    }
}
