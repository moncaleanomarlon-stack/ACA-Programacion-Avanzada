using System;
using BibliotecaApp.Entidades;
using BibliotecaApp.LogicaNegocio;

namespace BibliotecaApp.Pruebas
{
    /// <summary>Pruebas automáticas sencillas (sin dependencias externas).</summary>
    internal static class Program
    {
        private static int fallas;

        private static void Verificar(string nombre, bool condicion)
        {
            Console.WriteLine((condicion ? "OK    " : "FALLA ") + nombre);
            if (!condicion) fallas++;
        }

        private static bool LanzaReglaNegocio(Action accion)
        {
            try { accion(); return false; }
            catch (ReglaNegocioException) { return true; }
        }

        private static int Main()
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

            Console.WriteLine();
            Console.WriteLine(fallas == 0 ? "TODAS LAS PRUEBAS PASARON" : "PRUEBAS FALLIDAS: " + fallas);
            return fallas == 0 ? 0 : 1;
        }
    }
}
