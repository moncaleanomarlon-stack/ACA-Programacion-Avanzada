using System;
using System.Collections.Generic;
using System.Linq;
using BibliotecaApp.AccesoDatos;
using BibliotecaApp.Entidades;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Pruebas
{
    /// <summary>
    /// Pruebas automáticas sencillas, sin librerías externas.
    ///   dotnet run --project BibliotecaApp.Pruebas            (no necesita SQL Server)
    ///   dotnet run --project BibliotecaApp.Pruebas -- bd      (además prueba contra BibliotecaDB)
    /// </summary>
    internal static class Program
    {
        private static int fallas;
        private static int total;

        private static void Verificar(string nombre, bool condicion)
        {
            total++;
            Console.WriteLine((condicion ? "OK    " : "FALLA ") + nombre);
            if (!condicion) fallas++;
        }

        private static bool LanzaReglaNegocio(Action accion)
        {
            try { accion(); return false; }
            catch (ReglaNegocioException) { return true; }
        }

        /// <summary>True si lanza ReglaNegocioException y su mensaje contiene el fragmento.</summary>
        private static bool LanzaReglaConMensaje(Action accion, string fragmento)
        {
            try { accion(); return false; }
            catch (ReglaNegocioException ex)
            {
                bool ok = ex.Message.IndexOf(fragmento, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!ok) Console.WriteLine("       mensaje recibido: " + ex.Message);
                return ok;
            }
        }

        private static int Main(string[] args)
        {
            bool conBaseDatos = args.Any(a => a.Trim('-').Equals("bd", StringComparison.OrdinalIgnoreCase));

            Console.WriteLine("== Validaciones y entidades ==");
            PruebasValidador();
            Console.WriteLine();
            Console.WriteLine("== Reglas de negocio que no necesitan base de datos ==");
            PruebasSinBaseDatos();

            if (conBaseDatos)
            {
                Console.WriteLine();
                Console.WriteLine("== Pruebas contra BibliotecaDB (crean y borran sus propios datos) ==");
                PruebasConBaseDatos();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("(Pruebas con SQL Server omitidas. Use el argumento 'bd' para ejecutarlas.)");
            }

            Console.WriteLine();
            Console.WriteLine(fallas == 0
                ? "TODAS LAS PRUEBAS PASARON (" + total + " de " + total + ")"
                : "PRUEBAS FALLIDAS: " + fallas + " de " + total);
            return fallas == 0 ? 0 : 1;
        }

        // ------------------------------------------------------------------
        // 1. Validador y entidades (las 19 pruebas originales)
        // ------------------------------------------------------------------
        private static void PruebasValidador()
        {
            Verificar("Nombre válido con tilde y espacios", Validador.NombrePersona("  José Ángel ", "Nombre", 50) == "José Ángel");
            Verificar("Nombre con dígitos es rechazado", LanzaReglaNegocio(() => Validador.NombrePersona("Ana123", "Nombre", 50)));
            Verificar("Campo vacío es rechazado", LanzaReglaNegocio(() => Validador.Texto("   ", "Título", 10)));
            Verificar("Longitud excedida es rechazada", LanzaReglaNegocio(() => Validador.Texto("12345678901", "Título", 10)));
            Verificar("Correo válido", Validador.Correo("a@b.co", "Correo", 100) == "a@b.co");
            Verificar("Correo inválido es rechazado", LanzaReglaNegocio(() => Validador.Correo("a@b", "Correo", 100)));
            Verificar("Teléfono válido (se quitan espacios)", Validador.Telefono("310 123 4567", "Teléfono") == "3101234567");
            Verificar("Teléfono muy corto es rechazado", LanzaReglaNegocio(() => Validador.Telefono("123", "Teléfono")));
            Verificar("Documento válido", Validador.Documento("1018456789", "Documento") == "1018456789");
            Verificar("Documento con letras es rechazado", LanzaReglaNegocio(() => Validador.Documento("10A456", "Documento")));
            Verificar("ISBN del script de datos es válido", Validador.Isbn("978-958-42-2000-4", "ISBN") == "978-958-42-2000-4");
            Verificar("ISBN con X final se normaliza", Validador.Isbn("0-306-40615-x", "ISBN") == "0-306-40615-X");
            Verificar("ISBN corto es rechazado", LanzaReglaNegocio(() => Validador.Isbn("123", "ISBN")));
            Verificar("Entero fuera de rango es rechazado", LanzaReglaNegocio(() => Validador.Entero("-1", "Stock", 0, 10)));
            Verificar("Selección vacía es rechazada", LanzaReglaNegocio(() => Validador.Seleccion(null, "Autor")));

            Persona autor = new Autor(1, "Gabriel", "García Márquez", "Colombiana");
            Verificar("Polimorfismo: Autor.Describir()", autor.Describir() == "Gabriel García Márquez (Colombiana)");
            Persona usuario = new Usuario(1, "123", "Ana", "Paz", "3001234567", "a@b.co");
            Verificar("Polimorfismo: Usuario.ToString()", usuario.ToString() == "Ana Paz - Doc. 123");
            Verificar("Libro sin stock no tiene existencias", !new Libro(1, "x", "t", 1, 1, 2000, 0).TieneExistencias);
            Verificar("Préstamo nuevo inicia como 'Prestado'", new Prestamo().Estado == Prestamo.EstadoPrestado);
        }

        // ------------------------------------------------------------------
        // 2. Reglas que se rechazan antes de llegar a SQL Server
        // ------------------------------------------------------------------
        private static void PruebasSinBaseDatos()
        {
            PrestamoBLL prestamos = new PrestamoBLL();
            DateTime ahora = DateTime.Now;

            Verificar("Préstamo sin usuario es rechazado",
                LanzaReglaConMensaje(() => prestamos.RegistrarPrestamo(0, 1, ahora), "usuario"));
            Verificar("Préstamo sin libro es rechazado",
                LanzaReglaConMensaje(() => prestamos.RegistrarPrestamo(1, 0, ahora), "libro"));
            Verificar("Préstamo con fecha futura es rechazado",
                LanzaReglaConMensaje(() => prestamos.RegistrarPrestamo(1, 1, ahora.AddDays(1)), "futura"));
            Verificar("Devolución con fecha futura es rechazada",
                LanzaReglaConMensaje(() => prestamos.RegistrarDevolucion(1, ahora.AddDays(1)), "futura"));
            Verificar("Devolución anterior al préstamo es rechazada",
                LanzaReglaConMensaje(() => prestamos.ActualizarFechas(1, ahora, ahora.AddDays(-1)), "anterior"));

            Verificar("Búsqueda LIKE escapa los comodines % _ [",
                ConexionBD.PatronLike(" 50%_[x] ") == "%50[%][_][[]x]%");
            Verificar("Error SQL en inglés se asocia a la restricción exacta",
                ConexionBD.NombreRestriccion("Violation of UNIQUE KEY constraint 'UQ_Libros_ISBNSinGuiones'. " +
                    "Cannot insert duplicate key in object 'dbo.Libros'.") == "UQ_Libros_ISBNSinGuiones");
            Verificar("Error SQL en español (comillas dobles) se reconoce",
                ConexionBD.NombreRestriccion("Instrucción DELETE en conflicto con la restricción REFERENCE " +
                    "\"FK_Libros_Autores\". El conflicto ha aparecido en la base de datos \"BibliotecaDB\".")
                    == "FK_Libros_Autores");
        }

        // ------------------------------------------------------------------
        // 3. Préstamos, devoluciones y duplicados contra la base de datos real
        // ------------------------------------------------------------------
        private static void PruebasConBaseDatos()
        {
            string mensaje;
            if (!new ConexionBD().ProbarConexion(out mensaje))
            {
                Verificar("Conexión con SQL Server (" + mensaje + ")", false);
                return;
            }

            ConexionBD cx = new ConexionBD();
            AutorBLL autores = new AutorBLL();
            EditorialBLL editoriales = new EditorialBLL();
            UsuarioBLL usuarios = new UsuarioBLL();
            LibroBLL libros = new LibroBLL();
            PrestamoBLL prestamos = new PrestamoBLL();

            // Datos únicos para esta ejecución
            string letras = new string(Guid.NewGuid().ToString("N").Substring(0, 8)
                .Select(c => (char)('a' + c % 26)).ToArray());
            string numero = (DateTime.Now.Ticks % 10000000000L).ToString("D10");

            int autorId = 0, editorialId = 0;
            List<int> usuariosCreados = new List<int>();
            List<int> librosCreados = new List<int>();

            Func<int, int> stock = id => Convert.ToInt32(cx.Escalar(
                "SELECT Stock FROM Libros WHERE LibroID = @Id", ConexionBD.Param("@Id", id)));

            try
            {
                autorId = autores.Registrar(new Autor(0, "Prueba", "Auto" + letras, "Colombiana"));
                editorialId = editoriales.Registrar(new Editorial(0, "Editorial Prueba " + letras,
                    "6010000000", "prueba" + letras + "@test.co"));
                int u1 = usuarios.Registrar(new Usuario(0, "8" + numero, "Prueba", "Uno", "3000000001",
                    "uno" + letras + "@test.co"));
                usuariosCreados.Add(u1);
                int u2 = usuarios.Registrar(new Usuario(0, "7" + numero, "Prueba", "Dos", "3000000002",
                    "dos" + letras + "@test.co"));
                usuariosCreados.Add(u2);

                string isbnBase = "999-" + numero.Substring(0, 9);
                int libroA = libros.Registrar(new Libro(0, isbnBase + "1", "Libro A " + letras, autorId, editorialId, 2020, 2));
                librosCreados.Add(libroA);
                int libroB = libros.Registrar(new Libro(0, isbnBase + "2", "Libro B " + letras, autorId, editorialId, 2020, 1));
                librosCreados.Add(libroB);
                int libroC = libros.Registrar(new Libro(0, isbnBase + "3", "Libro C " + letras, autorId, editorialId, 2020, 1));
                librosCreados.Add(libroC);
                int libroD = libros.Registrar(new Libro(0, isbnBase + "4", "Libro D " + letras, autorId, editorialId, 2020, 1));
                librosCreados.Add(libroD);

                // Duplicados
                Verificar("Autor duplicado (sin distinguir mayúsculas) es rechazado",
                    LanzaReglaConMensaje(() => autores.Registrar(
                        new Autor(0, "PRUEBA", "AUTO" + letras.ToUpperInvariant(), "Chilena")), "ya existe un autor"));
                Verificar("ISBN repetido escrito sin guiones es rechazado",
                    LanzaReglaConMensaje(() => libros.Registrar(new Libro(0, (isbnBase + "1").Replace("-", ""),
                        "Copia", autorId, editorialId, 2020, 1)), "con o sin guiones"));

                // Préstamo y stock
                DateTime hace = DateTime.Now.AddMinutes(-5);
                int pA = prestamos.RegistrarPrestamo(u1, libroA, hace);
                Verificar("Registrar préstamo descuenta una unidad (2 -> 1)", stock(libroA) == 1);
                Verificar("Mismo libro activo para el mismo usuario es rechazado",
                    LanzaReglaConMensaje(() => prestamos.RegistrarPrestamo(u1, libroA, hace), "ya tiene prestado"));

                int pB = prestamos.RegistrarPrestamo(u1, libroB, hace);
                prestamos.RegistrarPrestamo(u1, libroC, hace);
                Verificar("Cuarto préstamo activo del usuario es rechazado (máximo 3)",
                    LanzaReglaConMensaje(() => prestamos.RegistrarPrestamo(u1, libroD, hace), "máximo"));
                Verificar("Préstamo de un libro sin existencias es rechazado",
                    LanzaReglaConMensaje(() => prestamos.RegistrarPrestamo(u2, libroC, hace), "existencias"));
                Verificar("Un préstamo rechazado no cambia el stock", stock(libroC) == 0 && stock(libroD) == 1);

                // Devolución
                prestamos.RegistrarDevolucion(pA, DateTime.Now);
                Verificar("Registrar devolución repone la unidad (1 -> 2)", stock(libroA) == 2);
                Verificar("Devolver dos veces el mismo préstamo es rechazado",
                    LanzaReglaConMensaje(() => prestamos.RegistrarDevolucion(pA, DateTime.Now), "ya fue devuelto"));
                Verificar("La segunda devolución no infla el stock", stock(libroA) == 2);

                // Eliminar préstamo activo
                prestamos.Eliminar(pB);
                Verificar("Eliminar un préstamo activo repone el stock (0 -> 1)", stock(libroB) == 1);

                // Edición de existencias con valor desactualizado
                Libro editado = new Libro(libroA, isbnBase + "1", "Libro A " + letras, autorId, editorialId, 2020, 5);
                Verificar("Actualizar con un stock desactualizado es rechazado",
                    LanzaReglaConMensaje(() => libros.Actualizar(editado, 99), "cambiaron"));
                Verificar("...y no modifica el stock", stock(libroA) == 2);
                libros.Actualizar(editado, 2);
                Verificar("Actualizar con el stock vigente sí guarda (2 -> 5)", stock(libroA) == 5);

                // Integridad referencial
                Verificar("Eliminar un usuario con préstamos es rechazado",
                    LanzaReglaConMensaje(() => usuarios.Eliminar(u1), "préstamos"));
                Verificar("Eliminar un autor con libros es rechazado",
                    LanzaReglaConMensaje(() => autores.Eliminar(autorId), "libros"));
            }
            catch (Exception ex)
            {
                Verificar("Error inesperado: " + ex.Message, false);
            }
            finally
            {
                // Limpieza: se borra todo lo creado por esta ejecución
                try
                {
                    foreach (int u in usuariosCreados)
                        cx.Ejecutar("DELETE FROM Prestamos WHERE UsuarioID = @U", ConexionBD.Param("@U", u));
                    foreach (int l in librosCreados)
                        cx.Ejecutar("DELETE FROM Libros WHERE LibroID = @L", ConexionBD.Param("@L", l));
                    foreach (int u in usuariosCreados)
                        cx.Ejecutar("DELETE FROM Usuarios WHERE UsuarioID = @U", ConexionBD.Param("@U", u));
                    if (autorId > 0)
                        cx.Ejecutar("DELETE FROM Autores WHERE AutorID = @A", ConexionBD.Param("@A", autorId));
                    if (editorialId > 0)
                        cx.Ejecutar("DELETE FROM Editoriales WHERE EditorialID = @E", ConexionBD.Param("@E", editorialId));
                    Console.WriteLine("       (datos de prueba eliminados)");
                }
                catch (Exception ex)
                {
                    Verificar("Limpieza de datos de prueba: " + ex.Message, false);
                }
            }
        }
    }
}
