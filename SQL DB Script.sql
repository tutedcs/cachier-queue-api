CREATE DATABASE CASHIERQUEUE
USE CASHIERQUEUE

CREATE TABLE ROL (
idRol INT IDENTITY PRIMARY KEY NOT NULL,
nombre VARCHAR(20) NOT NULL
)	

INSERT INTO ROL (nombre) VALUES ('ADMINISTRADOR')
INSERT INTO ROL (nombre) VALUES ('USUARIO')

CREATE TABLE SECCION (
	idSeccion INT IDENTITY PRIMARY KEY NOT NULL,
	nSeccion INT NOT NULL,
	nombre VARCHAR(20) NULL
)

INSERT INTO SECCION (nSeccion) VALUES (0);  -- Esta es la seccion para la "Caja" para los administradores y los usuarios deslogeados, tendria el idSeccion 1

INSERT INTO SECCION (nSeccion, nombre) VALUES (1, 'Poker Room'); -- Seccion 1 o Poker Room
INSERT INTO SECCION (nSeccion, nombre) VALUES (2, 'Nucleo 1'); -- Seccion 2 o Nucleo 1
INSERT INTO SECCION (nSeccion, nombre) VALUES (3, 'Nucleo 2'); -- Seccion 3 o Nucleo 2
INSERT INTO SECCION (nSeccion, nombre) VALUES (4, 'Nucleo 3'); -- Seccion 4 o Nucleo 3

CREATE TABLE CAJAS (
    idCaja INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	nCaja INT NOT NULL,
	seccion INT NOT NULL,
	isLogged BIT NOT NULL,
	FOREIGN KEY (seccion) REFERENCES SECCION(idSeccion),
);

INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (1, 0, 0); -- Esta es la "Caja" para los administradores, tendria el idCaja 1
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (1, 0, 0); -- Esta es la "Caja" para los usuarios deslogeados, tendria el idCaja 2

-- Seccion 1 o Poker Room
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (2, 1, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (2, 2, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (2, 3, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (2, 4, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (2, 5, 0);

-- Seccion 2 o Nucleo 1
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (3, 1, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (3, 2, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (3, 3, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (3, 4, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (3, 5, 0);

-- Seccion 3 o Nucleo 2
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (4, 1, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (4, 2, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (4, 3, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (4, 4, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (4, 5, 0);

-- Seccion 4 o Nucleo 3
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (5, 1, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (5, 2, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (5, 3, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (5, 4, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged) VALUES (5, 5, 0);



CREATE TABLE USUARIO (
    idUsuario INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    nombre VARCHAR(20) NOT NULL,
    apellido VARCHAR(20) NOT NULL,
    usuario VARCHAR(20) UNIQUE NOT NULL,
	passwordHash VARCHAR(64) NULL,
    rol INT NOT NULL,
	caja INT NOT NULL,
    FOREIGN KEY (rol) REFERENCES ROL(idRol),
    FOREIGN KEY (caja) REFERENCES CAJAS(idCaja)
);

--- Administradores

-- Administrador maximo, cualquier otro tendra que ser declarado abajo
-- se tendria que deshabilitar, una vez seteado los usuarios no?
INSERT INTO USUARIO (nombre, apellido, usuario, passwordHash, rol, caja) VALUES ('admin', '#0', 'admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 1, 1);  




--Datos para prueba

-- Por default, todos tendrian el idCaja 2
-- Al logear se le asignaria una caja hasta deslogear

----- Seccion 1 o Poker Room
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Tomas Bautista', 'Raris', '000010', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Joaquin', 'Garay', '000011', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Sofia', 'Ojeda', '000012', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Federico', 'Sosa', '000013', 2, 2);

----- Seccion 2 o Nucleo 1
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Ezequiel', 'Robles', '000020', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Elias', 'Ferreyra', '000021', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Luciano', 'Cayssals', '000022', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Lara', 'Molina', '000023', 2, 2);

----- Seccion 3 o Nucleo 2
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Thomas', 'Alvares', '000030', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Mateo', 'Tomas', '000031', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Geronimo', 'Ramos', '000032', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Tomas', 'Montejano', '000033', 2, 2);

----- Seccion 4 o Nucleo 3
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Martin', 'Jerez', '000040', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Jeronimo', 'ConJota', '000041', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Mauro', 'Tolaba', '000042', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Ernesto', 'Medina', '000043', 2, 2);

