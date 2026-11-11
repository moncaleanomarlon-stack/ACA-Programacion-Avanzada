-- =====================================================================
-- SISTEMA DE GESTIÓN DE BIBLIOTECA
-- Script de creación de base de datos, tablas, restricciones, datos de
-- prueba y vistas de reportes.  Motor: SQL Server 2019 / 2022
-- Ejecutar completo en SQL Server Management Studio (SSMS).
-- =====================================================================

-- Opciones requeridas por el índice UNIQUE sobre la columna calculada
-- Libros.ISBNSinGuiones. SSMS ya las usa por defecto; sqlcmd no.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. CREACIÓN DE LA BASE DE DATOS -------------------------------------
USE master;
GO
IF DB_ID('BibliotecaDB') IS NOT NULL
BEGIN
    ALTER DATABASE BibliotecaDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BibliotecaDB;
END
GO
CREATE DATABASE BibliotecaDB;
GO
USE BibliotecaDB;
GO

-- 2. ESTRUCTURA DE TABLAS (DDL) ---------------------------------------

CREATE TABLE Autores (
    AutorID      INT IDENTITY(1,1) NOT NULL,
    Nombre       VARCHAR(50) NOT NULL,
    Apellido     VARCHAR(50) NOT NULL,
    Nacionalidad VARCHAR(50) NOT NULL,
    CONSTRAINT PK_Autores PRIMARY KEY (AutorID),
    -- Evita registrar dos veces al mismo autor (la intercalación no distingue mayúsculas)
    CONSTRAINT UQ_Autores_NombreApellido UNIQUE (Nombre, Apellido)
);
GO

CREATE TABLE Editoriales (
    EditorialID INT IDENTITY(1,1) NOT NULL,
    Nombre      VARCHAR(100) NOT NULL,
    Telefono    VARCHAR(20)  NOT NULL,
    Email       VARCHAR(100) NOT NULL,
    CONSTRAINT PK_Editoriales        PRIMARY KEY (EditorialID),
    CONSTRAINT UQ_Editoriales_Nombre UNIQUE (Nombre),
    CONSTRAINT UQ_Editoriales_Email  UNIQUE (Email),
    CONSTRAINT CK_Editoriales_Email  CHECK (Email LIKE '%_@__%.__%')
);
GO

CREATE TABLE Libros (
    LibroID         INT IDENTITY(1,1) NOT NULL,
    ISBN            VARCHAR(20)  NOT NULL,
    Titulo          VARCHAR(150) NOT NULL,
    AutorID         INT NOT NULL,
    EditorialID     INT NOT NULL,
    AnioPublicacion INT NOT NULL,
    Stock           INT NOT NULL CONSTRAINT DF_Libros_Stock DEFAULT 1,  -- ejemplares DISPONIBLES
    -- ISBN sin guiones: '978-958-42-2000-4' y '9789584220004' son el mismo libro
    ISBNSinGuiones  AS CAST(REPLACE(ISBN, '-', '') AS VARCHAR(20)) PERSISTED,
    CONSTRAINT PK_Libros PRIMARY KEY (LibroID),
    CONSTRAINT UQ_Libros_ISBN UNIQUE (ISBN),
    CONSTRAINT UQ_Libros_ISBNSinGuiones UNIQUE (ISBNSinGuiones),
    CONSTRAINT CK_Libros_Anio  CHECK (AnioPublicacion > 1000 AND AnioPublicacion <= 2100),
    CONSTRAINT CK_Libros_Stock CHECK (Stock >= 0),
    CONSTRAINT FK_Libros_Autores      FOREIGN KEY (AutorID)     REFERENCES Autores(AutorID),
    CONSTRAINT FK_Libros_Editoriales  FOREIGN KEY (EditorialID) REFERENCES Editoriales(EditorialID)
);
GO

CREATE TABLE Usuarios (
    UsuarioID INT IDENTITY(1,1) NOT NULL,
    Documento VARCHAR(20)  NOT NULL,
    Nombre    VARCHAR(50)  NOT NULL,
    Apellido  VARCHAR(50)  NOT NULL,
    Telefono  VARCHAR(20)  NOT NULL,
    Email     VARCHAR(100) NOT NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY (UsuarioID),
    CONSTRAINT UQ_Usuarios_Documento UNIQUE (Documento),
    CONSTRAINT UQ_Usuarios_Email     UNIQUE (Email),
    CONSTRAINT CK_Usuarios_Email     CHECK (Email LIKE '%_@__%.__%')
);
GO

CREATE TABLE Prestamos (
    PrestamoID      INT IDENTITY(1,1) NOT NULL,
    UsuarioID       INT NOT NULL,
    LibroID         INT NOT NULL,
    FechaPrestamo   DATETIME NOT NULL CONSTRAINT DF_Prestamos_Fecha DEFAULT GETDATE(),
    FechaDevolucion DATETIME NULL,
    Estado          VARCHAR(20) NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT 'Prestado',
    CONSTRAINT PK_Prestamos PRIMARY KEY (PrestamoID),
    CONSTRAINT FK_Prestamos_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID),
    CONSTRAINT FK_Prestamos_Libros   FOREIGN KEY (LibroID)   REFERENCES Libros(LibroID),
    CONSTRAINT CK_Prestamos_Estado   CHECK (Estado IN ('Prestado', 'Devuelto')),
    -- Coherencia entre estado y fecha de devolución
    CONSTRAINT CK_Prestamos_Coherencia CHECK (
        (Estado = 'Prestado' AND FechaDevolucion IS NULL)
        OR (Estado = 'Devuelto' AND FechaDevolucion IS NOT NULL AND FechaDevolucion >= FechaPrestamo))
);
GO

-- Índices sobre claves foráneas (mejoran joins y búsquedas)
CREATE INDEX IX_Libros_AutorID         ON Libros(AutorID);
CREATE INDEX IX_Libros_EditorialID     ON Libros(EditorialID);
CREATE INDEX IX_Libros_Titulo          ON Libros(Titulo);
CREATE INDEX IX_Prestamos_UsuarioID    ON Prestamos(UsuarioID);
CREATE INDEX IX_Prestamos_LibroID      ON Prestamos(LibroID);
CREATE INDEX IX_Prestamos_Estado       ON Prestamos(Estado);
GO

-- 3. DATOS DE PRUEBA (DML) --------------------------------------------

INSERT INTO Autores (Nombre, Apellido, Nacionalidad) VALUES
('Gabriel', 'García Márquez', 'Colombiana'),
('Mario', 'Vargas Llosa', 'Peruana'),
('Isabel', 'Allende', 'Chilena'),
('Jorge Luis', 'Borges', 'Argentina'),
('Julio', 'Cortázar', 'Argentina'),
('Laura', 'Restrepo', 'Colombiana'),
('Haruki', 'Murakami', 'Japonesa'),
('Stephen', 'King', 'Estadounidense');

INSERT INTO Editoriales (Nombre, Telefono, Email) VALUES
('Editorial Norma', '6013210000', 'contacto@norma.com'),
('Penguin Random House', '6015551234', 'info@penguinrandomhouse.com'),
('Grupo Editorial Planeta', '6014449876', 'atencion@planeta.com'),
('Alfaguara', '6017890123', 'servicio@alfaguara.com'),
('Fondo de Cultura Económica', '6013334455', 'ventas@fce.com.co');

-- Stock = ejemplares disponibles en este momento (ya descuenta préstamos activos)
INSERT INTO Libros (ISBN, Titulo, AutorID, EditorialID, AnioPublicacion, Stock) VALUES
('978-958-42-2000-4', 'Cien años de soledad', 1, 3, 1967, 5),
('978-958-30-0001-2', 'La ciudad y los perros', 2, 1, 1963, 3),
('978-958-42-5555-1', 'La casa de los espíritus', 3, 2, 1982, 4),
('978-958-70-4100-9', 'Ficciones', 4, 4, 1944, 2),
('978-958-88-2122-3', 'Rayuela', 5, 4, 1963, 3),
('978-958-42-8899-8', 'Delirio', 6, 3, 2004, 4),
('978-958-54-3210-0', 'Tokio blues', 7, 2, 1987, 2),
('978-030-77-4365-7', 'El resplandor', 8, 2, 1977, 5),
('978-958-42-1111-0', 'El amor en los tiempos del cólera', 1, 3, 1985, 3),
('978-958-30-0222-1', 'La fiesta del chivo', 2, 1, 2000, 1);

INSERT INTO Usuarios (Documento, Nombre, Apellido, Telefono, Email) VALUES
('1018456789', 'Marlon Fabián', 'Moncaleano Núñez', '3101234567', 'marlon.moncaleano@cun.edu.co'),
('1020987654', 'Angie Carolina', 'Rincón', '3209876543', 'angie.rincon@correo.com'),
('1015333444', 'Carlos Andrés', 'Mendoza', '3004567890', 'carlos.mendoza@gmail.com'),
('1032111222', 'Laura Daniela', 'Gómez', '3156543210', 'laura.gomez@hotmail.com'),
('1019888777', 'Santiago', 'Torres', '3187654321', 'santiago.torres@outlook.com'),
('1022444555', 'Valentina', 'Rojas', '3129876543', 'valentina.rojas@yahoo.com');

-- Fechas en formato ISO 8601 (con 'T'): no dependen del idioma del servidor.
-- Con 'yyyy-mm-dd hh:mm:ss' un SQL Server en español lee año-día-mes y falla.
INSERT INTO Prestamos (UsuarioID, LibroID, FechaPrestamo, FechaDevolucion, Estado) VALUES
(1, 1, '2026-10-01T09:30:00', '2026-10-10T14:15:00', 'Devuelto'),
(2, 3, '2026-10-05T11:00:00', '2026-10-12T16:45:00', 'Devuelto'),
(3, 5, '2026-10-10T15:20:00', '2026-10-20T10:00:00', 'Devuelto'),
(1, 4, '2026-11-01T10:00:00', NULL, 'Prestado'),
(2, 7, '2026-11-03T14:30:00', NULL, 'Prestado'),
(4, 2, '2026-11-05T08:45:00', NULL, 'Prestado'),
(5, 8, '2026-11-08T16:10:00', NULL, 'Prestado');
GO

-- 4. VISTAS DE CONSULTA Y REPORTES ------------------------------------

CREATE VIEW VISTA_InventarioLibros AS
SELECT l.LibroID, l.ISBN, l.Titulo,
       CONCAT(a.Nombre, ' ', a.Apellido) AS Autor,
       e.Nombre AS Editorial,
       l.AnioPublicacion, l.Stock AS Disponibles
FROM Libros l
INNER JOIN Autores a     ON l.AutorID = a.AutorID
INNER JOIN Editoriales e ON l.EditorialID = e.EditorialID;
GO

CREATE VIEW VISTA_ResumenAutores AS
SELECT a.AutorID, a.Nombre, a.Apellido, a.Nacionalidad,
       COUNT(l.LibroID) AS LibrosRegistrados
FROM Autores a
LEFT JOIN Libros l ON l.AutorID = a.AutorID
GROUP BY a.AutorID, a.Nombre, a.Apellido, a.Nacionalidad;
GO

CREATE VIEW VISTA_ResumenEditoriales AS
SELECT e.EditorialID, e.Nombre, e.Telefono, e.Email,
       COUNT(l.LibroID) AS LibrosRegistrados
FROM Editoriales e
LEFT JOIN Libros l ON l.EditorialID = e.EditorialID
GROUP BY e.EditorialID, e.Nombre, e.Telefono, e.Email;
GO

CREATE VIEW VISTA_PrestamosActivos AS
SELECT p.PrestamoID, u.Documento,
       CONCAT(u.Nombre, ' ', u.Apellido) AS Usuario,
       l.ISBN, l.Titulo AS Libro,
       CONCAT(a.Nombre, ' ', a.Apellido) AS Autor,
       p.FechaPrestamo, p.Estado
FROM Prestamos p
INNER JOIN Usuarios u ON p.UsuarioID = u.UsuarioID
INNER JOIN Libros l   ON p.LibroID = l.LibroID
INNER JOIN Autores a  ON l.AutorID = a.AutorID
WHERE p.Estado = 'Prestado';
GO

CREATE VIEW VISTA_HistorialPrestamos AS
SELECT p.PrestamoID, u.Documento,
       CONCAT(u.Nombre, ' ', u.Apellido) AS Usuario,
       l.ISBN, l.Titulo AS Libro,
       p.FechaPrestamo, p.FechaDevolucion, p.Estado
FROM Prestamos p
INNER JOIN Usuarios u ON p.UsuarioID = u.UsuarioID
INNER JOIN Libros l   ON p.LibroID = l.LibroID;
GO

CREATE VIEW VISTA_LibrosMasPrestados AS
SELECT l.LibroID, l.ISBN, l.Titulo, COUNT(p.PrestamoID) AS TotalPrestamos
FROM Libros l
LEFT JOIN Prestamos p ON p.LibroID = l.LibroID
GROUP BY l.LibroID, l.ISBN, l.Titulo;
GO

CREATE VIEW VISTA_ResumenGeneral AS
SELECT 'Títulos registrados'      AS Concepto, COUNT(*) AS Total FROM Libros
UNION ALL SELECT 'Ejemplares disponibles',  ISNULL(SUM(Stock), 0)            FROM Libros
UNION ALL SELECT 'Autores',                 COUNT(*)                         FROM Autores
UNION ALL SELECT 'Editoriales',             COUNT(*)                         FROM Editoriales
UNION ALL SELECT 'Usuarios',                COUNT(*)                         FROM Usuarios
UNION ALL SELECT 'Préstamos activos',       COUNT(*)                         FROM Prestamos WHERE Estado = 'Prestado'
UNION ALL SELECT 'Préstamos devueltos',     COUNT(*)                         FROM Prestamos WHERE Estado = 'Devuelto';
GO
