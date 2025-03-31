--CREATE DATABASE CASHIERQUEUE
USE CASHIERQUEUE

CREATE TABLE ROL (
idRol INT IDENTITY PRIMARY KEY NOT NULL,
nombre VARCHAR(20) NOT NULL
)	

INSERT INTO ROL (nombre) VALUES ('ADMINISTRADOR')
INSERT INTO ROL (nombre) VALUES ('USUARIO')

CREATE TABLE SECCION (
	idSeccion INT IDENTITY PRIMARY KEY NOT NULL,
	nSeccion INT NOT NULL
)

INSERT INTO SECCION (nSeccion) VALUES (0);  -- Esta es la seccion para la "Caja" para los administradores, tendria el idSeccion 1
INSERT INTO SECCION (nSeccion) VALUES (1);
INSERT INTO SECCION (nSeccion) VALUES (2);
INSERT INTO SECCION (nSeccion) VALUES (3);
INSERT INTO SECCION (nSeccion) VALUES (4);

CREATE TABLE CAJAS (
    idCaja INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	nCaja INT NOT NULL,
	seccion INT NOT NULL,
    isLogged BIT NOT NULL,
	disponibilidad BIT NOT NULL,
	FOREIGN KEY (seccion) REFERENCES SECCION(idSeccion),
);

INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (1, 0, 0, 0); -- Esta es la "Caja" para los administradores, tendria el idCaja 1

INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (2, 1, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (2, 2, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (2, 3, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (2, 4, 0, 0);

INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (3, 1, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (3, 2, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (3, 3, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (3, 4, 0, 0);

INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (4, 1, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (4, 2, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (4, 3, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (4, 4, 0, 0);

INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (5, 1, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (5, 2, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (5, 3, 0, 0);
INSERT INTO CAJAS (seccion, nCaja, isLogged, disponibilidad) VALUES (5, 4, 0, 0);



CREATE TABLE USUARIO (
    idUsuario INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    nombre VARCHAR(20) NOT NULL,
    apellido VARCHAR(20) NOT NULL,
    usuario VARCHAR(20) UNIQUE NOT NULL,
    rol INT NOT NULL,
	caja INT NOT NULL,
    FOREIGN KEY (rol) REFERENCES ROL(idRol),
    FOREIGN KEY (caja) REFERENCES CAJAS(idCaja)
);

--- Administradores
INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('admin', '#0', 'admin', 1, 1);  -- Administrador maximo, cualquier otro tendra que ser declarado abajo




--Datos para prueba
----- Seccion 1
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Tomas Bautista', 'Raris', '000010', 2, 2);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Joaquin', 'Garay', '000011', 2, 3);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Sofia', 'Ojeda', '000012', 2, 4);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Federico', 'Sosa', '000013', 2, 5);

----- Seccion 2
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Ezequiel', 'Robles', '000020', 2, 6);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Elias', 'Ferreyra', '000021', 2, 7);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Luciano', 'Cayssals', '000022', 2, 8);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Lara', 'Molina', '000023', 2, 9);

----- Seccion 3
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Thomas', 'Alvares', '000030', 2, 10);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Mateo', 'Tomas', '000031', 2, 11);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Geronimo', 'Ramos', '000032', 2, 12);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Tomas', 'Montejano', '000033', 2, 13);

----- Seccion 4
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Martin', 'Jerez', '000040', 2, 14);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Jeronimo', 'ConJota', '000041', 2, 15);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Mauro', 'Tolaba', '000042', 2, 16);
--INSERT INTO USUARIO (nombre, apellido, usuario, rol, caja) VALUES ('Ernesto', 'Medina', '000043', 2, 17);

