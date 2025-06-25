USE Hangman;

CREATE TABLE partida (
	[id_partida] INT IDENTITY(1,1),
	[codigo] VARCHAR(7),
	[idioma] VARCHAR(10),
	[fecha_creacion] DATE,
	[id_palabra] INT,
	[id_jugador_retador] INT,
	[id_jugador_adivinador] INT,
	[id_estado_partida] INT,
	[id_idioma_partida] INT,
	PRIMARY KEY ([id_partida])
)

CREATE TABLE palabra(
	[id_palabra] INT IDENTITY(1,1),
	[nombre] VARCHAR(20),
	[nombre_en] VARCHAR(20),
	[pista] VARCHAR(150),
	[pista_en] VARCHAR(150),
	[id_categoria] INT,
	PRIMARY KEY ([id_palabra])
)

CREATE TABLE categoria(
	[id_categoria] INT IDENTITY(1,1),
	[nombre] VARCHAR(45),
	[nombre_en] VARCHAR(45),
	PRIMARY KEY([id_categoria])
)

CREATE TABLE jugador (
	[id_jugador] INT IDENTITY(1,1),
    [usuario] VARCHAR(12),
	[nombre_completo] VARCHAR(40),
	[fecha_nacimiento] DATE,
	[telefono] INT,
	[correo] VARCHAR(45),
	[contrasena] VARCHAR (20),
	[puntuacion] INT,
	PRIMARY KEY ([id_jugador])
)

CREATE TABLE estado_partida (
	[id_estado_partida] INT IDENTITY(1,1),
	[nombre] VARCHAR(50),
	PRIMARY KEY ([id_estado_partida])
)

CREATE TABLE idioma_partida (
	[id_idioma_partida] INT IDENTITY(1,1),
	[nombre] VARCHAR(50),
	PRIMARY KEY ([id_idioma_partida])
)

ALTER TABLE partida ADD CONSTRAINT FK_partida_palabra FOREIGN KEY ([id_palabra]) REFERENCES palabra ([id_palabra]) ON DELETE CASCADE;
ALTER TABLE partida ADD CONSTRAINT FK_partida_jugador FOREIGN KEY ([id_jugador]) REFERENCES jugador ([id_jugador]) ON DELETE CASCADE;
ALTER TABLE partida ADD CONSTRAINT FK_partida_estado_partida FOREIGN KEY ([id_estado_partida]) REFERENCES estado_partida ([id_estado_partida]) ON DELETE CASCADE;
ALTER TABLE partida ADD CONSTRAINT FK_partida_idioma_partida FOREIGN KEY ([id_idioma_partida]) REFERENCES idioma_partida ([id_idioma_partida]) ON DELETE CASCADE;
ALTER TABLE palabra ADD CONSTRAINT FK_palabra_categoria FOREIGN KEY ([id_categoria]) REFERENCES categoria ([id_categoria]) ON DELETE CASCADE;

INSERT INTO categoria (nombre, nombre_en)
VALUES ('Música', 'Music'),
       ('Películas', 'Movies'),
       ('Series', 'TV Series');

INSERT INTO jugador (usuario, nombre_completo, fecha_nacimiento, telefono, correo, contrasena, puntuacion)
VALUES ('The Maker', 'Daniel Mongeote Tlachy', '2003-05-07', 2288663355, 'danielmongeote@gmail.com', 'HangmanUser17@', 150),
       ('The Void', 'Luis Angel Elizalde Arroyo', '2002-09-20', 2233446655, 'luiselizalde@gmail.com', 'HangmanUser17@', 0);

INSERT INTO estado_partida (nombre)
VALUES ('Completada'),
       ('Cancelada'),
       ('Perdida'),
       ('Ganada'),
	   ('Espera'),
	   ('Lista');

INSERT INTO idioma_partida (nombre)
VALUES ('Español'),
       ('Ingles');

INSERT INTO partida (codigo, idioma, fecha_creacion, id_palabra, id_jugador, id_estado_partida, id_idioma_partida)
VALUES ('ABC1234', 'Español', '2025-05-14', 1, 1, 2, 1, 1),
       ('XYZ7890', 'Ingles', '2025-05-14', 2, 2, 1, 1, 2);

INSERT INTO palabra (nombre, nombre_en, pista, pista_en, id_categoria)
VALUES ('Beatles', 'Beatles', 'Banda británica de los 60s conocida por éxitos como "Hey Jude"', 'British band from the 60s known for hits like "Hey Jude"', 1),
       ('Thriller', 'Thriller', 'Álbum de Michael Jackson con canciones icónicas como "Billie Jean"', 'Michael Jackson album with iconic songs like "Billie Jean"', 1),
       ('Madonna', 'Madonna', 'Reina del pop con éxitos como "Like a Virgin"', 'Queen of pop with hits like "Like a Virgin"', 1),
       ('Bohemian', 'Bohemian', 'Canción de Queen conocida por su estructura operística', 'Queen song known for its operatic structure', 1),
       ('Metallica', 'Metallica', 'Banda de thrash metal famosa por "Enter Sandman"', 'Thrash metal band famous for "Enter Sandman"', 1),
       ('Coldplay', 'Coldplay', 'Banda británica actual con éxitos como "Yellow"', 'Current British band with hits like "Yellow"', 1),
       ('Titanic', 'Titanic', 'Barco que se hunde en una película romántica de 1997', 'Ship that sinks in a 1997 romantic movie', 2),
       ('Matrix', 'Matrix', 'Realidad simulada en una película de ciencia ficción de 1999', 'Simulated reality in a 1999 sci-fi movie', 2),
       ('Avatar', 'Avatar', 'Aliens azules en un planeta llamado Pandora', 'Blue aliens on a planet called Pandora', 2),
       ('Star Wars', 'Star Wars', 'La fuerza te acompaña en esta saga espacial', 'The Force is with you in this space saga', 2),
       ('Frozen', 'Frozen', 'Princesa con poderes de hielo en una película de Disney', 'Princess with ice powers in a Disney movie', 2),
       ('Joker', 'Joker', 'Villano de Batman con una risa icónica', 'Batman villain with an iconic laugh', 2),
       ('Friends', 'Friends', 'Seis amigos en Nueva York en una serie de comedia', 'Six friends in New York in a comedy series', 3),
       ('Lost', 'Lost', 'Avión estrellado en una isla misteriosa', 'Plane crashed on a mysterious island', 3),
       ('Breaking Bad', 'Breaking Bad', 'Profesor de química hace drogas en Albuquerque', 'Chemistry teacher makes drugs in Albuquerque', 3),
       ('Stranger', 'Stranger', 'Niños y monstruos en un pueblo de los 80s', 'Kids and monsters in a 1980s town', 3),
       ('Squid Game', 'Squid Game', 'Juegos mortales por dinero en una serie coreana', 'Deadly games for money in a Korean series', 3),
       ('The Office', 'The Office', 'Comedia en una oficina de una empresa papelera', 'Comedy in the office of a paper company', 3);