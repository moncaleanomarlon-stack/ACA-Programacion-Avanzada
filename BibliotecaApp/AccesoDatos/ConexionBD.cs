using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.AccesoDatos
{
    /// <summary>
    /// Gestiona la conexión con SQL Server y ofrece métodos auxiliares para
    /// ejecutar consultas SIEMPRE parametrizadas. También traduce los errores
    /// de SQL Server a mensajes comprensibles para el usuario.
    /// </summary>
    public class ConexionBD
    {
        private const string NombreCadena = "BibliotecaDB";

        // Constraint (nombre definido en el script SQL) -> mensaje para el usuario
        private static readonly Dictionary<string, string> MensajesRestricciones =
            new Dictionary<string, string>
        {
            { "UQ_Libros_ISBNSinGuiones", "Ya existe un libro con ese ISBN (escrito con o sin guiones)." },
            { "UQ_Libros_ISBN",          "Ya existe un libro registrado con ese ISBN." },
            { "UQ_Autores_NombreApellido", "Ya existe un autor registrado con ese nombre y apellido." },
            { "UQ_Usuarios_Documento",   "Ya existe un usuario registrado con ese documento." },
            { "UQ_Usuarios_Email",       "Ya existe un usuario registrado con ese correo electrónico." },
            { "UQ_Editoriales_Nombre",   "Ya existe una editorial con ese nombre." },
            { "UQ_Editoriales_Email",    "Ya existe una editorial con ese correo electrónico." },
            { "FK_Libros_Autores",       "Operación no permitida: el autor está relacionado con libros registrados." },
            { "FK_Libros_Editoriales",   "Operación no permitida: la editorial está relacionada con libros registrados." },
            { "FK_Prestamos_Usuarios",   "Operación no permitida: el usuario tiene préstamos registrados." },
            { "FK_Prestamos_Libros",     "Operación no permitida: el libro tiene préstamos registrados." },
            { "CK_Libros_Anio",          "El año de publicación está fuera del rango permitido." },
            { "CK_Libros_Stock",         "Las existencias no pueden ser negativas." },
            { "CK_Prestamos_Coherencia", "El estado del préstamo no es coherente con la fecha de devolución." },
            { "CK_Usuarios_Email",       "El correo electrónico del usuario no tiene un formato válido." },
            { "CK_Editoriales_Email",    "El correo electrónico de la editorial no tiene un formato válido." }
        };

        private static string ObtenerCadena()
        {
            ConnectionStringSettings cfg = ConfigurationManager.ConnectionStrings[NombreCadena];
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.ConnectionString))
                throw new InvalidOperationException(
                    "No se encontró la cadena de conexión '" + NombreCadena + "' en App.config.");
            return cfg.ConnectionString;
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(ObtenerCadena());
        }

        /// <summary>Verifica que se pueda abrir la conexión con SQL Server.</summary>
        public bool ProbarConexion(out string mensaje)
        {
            try
            {
                using (SqlConnection con = ObtenerConexion())
                {
                    con.Open();
                    mensaje = "Conectado a " + con.DataSource + " / " + con.Database;
                    return true;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Sin conexión: " + ex.Message;
                return false;
            }
        }

        public static SqlParameter Param(string nombre, object valor)
        {
            return new SqlParameter(nombre, valor ?? DBNull.Value);
        }

        /// <summary>Convierte el texto en un patrón LIKE seguro (escapa % _ [ ).</summary>
        public static string PatronLike(string texto)
        {
            string t = (texto ?? string.Empty).Trim();
            t = t.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
            return "%" + t + "%";
        }

        public DataTable Consultar(string sql, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection con = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    if (parametros != null) cmd.Parameters.AddRange(parametros);
                    DataTable tabla = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(tabla);
                    }
                    return tabla;
                }
            }
            catch (SqlException ex) { throw Traducir(ex); }
        }

        public int Ejecutar(string sql, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection con = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    if (parametros != null) cmd.Parameters.AddRange(parametros);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex) { throw Traducir(ex); }
        }

        public object Escalar(string sql, params SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection con = ObtenerConexion())
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    if (parametros != null) cmd.Parameters.AddRange(parametros);
                    con.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex) { throw Traducir(ex); }
        }

        /// <summary>Traduce una SqlException a una excepción con mensaje claro.</summary>
        public static Exception Traducir(SqlException ex)
        {
            switch (ex.Number)
            {
                case -1: case 2: case 53: case 233: case 4060: case 18456:
                    return new Exception(
                        "No se pudo conectar con SQL Server. Verifique que el servicio esté activo, " +
                        "que la base BibliotecaDB exista (ejecute el script SQL) y la cadena de " +
                        "conexión en App.config.", ex);

                case 2627: case 2601: case 547:
                    string restriccion = NombreRestriccion(ex.Message);
                    string mensaje;
                    if (restriccion != null && MensajesRestricciones.TryGetValue(restriccion, out mensaje))
                        return new ReglaNegocioException(mensaje, ex);
                    foreach (KeyValuePair<string, string> par in MensajesRestricciones)
                    {
                        // Respaldo por si el formato del mensaje cambia (idioma o versión de SQL Server)
                        if (ex.Message.Contains(par.Key))
                            return new ReglaNegocioException(par.Value, ex);
                    }
                    return new ReglaNegocioException(
                        "La operación viola una restricción de integridad de la base de datos " +
                        "(valor duplicado o registro relacionado).", ex);

                default:
                    return new Exception("Error de base de datos (" + ex.Number + "): " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Busca en el mensaje de SQL Server el nombre exacto de la restricción, que
        /// aparece entre comillas simples o dobles (p. ej. 'UQ_Libros_ISBN'). Se compara
        /// el nombre completo para no confundir UQ_Libros_ISBN con UQ_Libros_ISBNSinGuiones.
        /// </summary>
        public static string NombreRestriccion(string mensajeSql)
        {
            if (mensajeSql == null) return null;
            foreach (Match m in Regex.Matches(mensajeSql, "['\"]([A-Za-z0-9_]+)['\"]"))
            {
                if (MensajesRestricciones.ContainsKey(m.Groups[1].Value))
                    return m.Groups[1].Value;
            }
            return null;
        }
    }
}
