using Biblioteca.DTO;
using Biblioteca.Servicios;
using HangmanGame_Servidor.Modelo;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace HangmanGame_Servidor
{
    public class HangmanServicio : IHangmanService
    {
        public ResponseDTO Autenticacion(string correo, string contrasena)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var jugador = context.jugador
                        .FirstOrDefault(j => j.correo == correo && j.contrasena == contrasena);

                    if (jugador != null)
                    {
                        var jugadorResponseDTO = new JugadorDTO
                        {
                            id_jugador = jugador.id_jugador,
                            correo = jugador.correo,
                            usuario = jugador.usuario,
                            nombre = jugador.nombre,
                            telefono = jugador.telefono,
                            puntuacion = jugador.puntuacion ?? 0
                        };

                        return new ResponseDTO
                        {
                            success = true,
                            data = jugadorResponseDTO,
                            message = "Autenticación exitosa"
                        };
                    }
                    else
                    {
                        return new ResponseDTO
                        {
                            success = false,
                            message = "Credenciales no encontradas"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    success = false,
                    message = $"Error en el servidor: {ex.Message}"
                };
            }
        }

        public ResponsePalabrasConIdDTO ObtenerPalabrasPorCategoria(string categoria)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    // Buscar el id_categoria basado en el nombre de la categoría
                    var categoriaId = context.categoria
                        .Where(c => c.nombre == categoria)
                        .Select(c => c.id_categoria)
                        .FirstOrDefault();

                    if (categoriaId == 0) // Si no se encuentra la categoría
                    {
                        return new ResponsePalabrasConIdDTO
                        {
                            success = false,
                            message = "Categoría no encontrada"
                        };
                    }

                    // Obtener las palabras asociadas al id_categoria
                    var palabras = context.palabra
                        .Where(p => p.id_categoria == categoriaId)
                        .Select(p => new PalabraDTO
                        {
                            IdPalabra = p.id_palabra,
                            nombre = p.nombre
                        })
                        .ToList();

                    if (palabras.Any())
                    {
                        
                        return new ResponsePalabrasConIdDTO
                        {
                            success = true,
                            data = new Palabras { PalabrasConId = palabras },
                            message = "Palabras obtenidas exitosamente" 
                        }; 
                    }
                    else
                    {
                        return new ResponsePalabrasConIdDTO
                        {
                            success = false,
                            message = "No se encontraron palabras para esta categoría"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResponsePalabrasConIdDTO
                {
                    success = false,
                    message = $"Error al obtener palabras: {ex.Message}"
                };
            }
        }

        public ResponsePartidaDTO CrearPartida(int idAdivinador, int idPalabra, string codigo, int idIdiomaPartida)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var partida = new partida
                    {
                        codigo = codigo,
                        id_palabra = idPalabra,
                        id_adivinador = idAdivinador, // Anfitrión
                        id_retador = null, // Todavía no hay retador
                        id_estado_partida = 6, // Asumimos que 6 es "esperando"
                        fecha_creacion = DateTime.Now
                    };

                    context.partida.Add(partida);
                    context.SaveChanges();

                    return new ResponsePartidaDTO
                    {
                        success = true,
                        data = new PartidaDTO
                        {
                            IdPartida = partida.id_partida,
                            Codigo = partida.codigo,
                            Idioma = partida.idioma,
                            IdPalabra = partida.id_palabra ?? 0,
                            IdRetador = partida.id_retador ?? 0,
                            IdAdivinador = partida.id_adivinador ?? 0,
                            IdEstadoPartida = partida.id_estado_partida ?? 0,
                            IdIdiomaPartida = partida.id_idioma_partida ?? 0
                        },
                        message = "Partida creada exitosamente"
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                // Captura de errores internos (como restricciones SQL)
                var inner = ex.InnerException;
                string fullError = ex.Message;

                while (inner != null)
                {
                    fullError += " --> " + inner.Message;
                    inner = inner.InnerException;
                }

                return new ResponsePartidaDTO
                {
                    success = false,
                    message = $"Error de base de datos al crear la partida: {fullError}"
                };
            }
            catch (Exception ex)
            {
                // Otros errores no relacionados con EF
                return new ResponsePartidaDTO
                {
                    success = false,
                    message = $"Error general al crear la partida: {ex.Message}"
                };
            }
        }


        public ResponsePartidaDTO ObtenerPartidasDisponibles()
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var partidas = (from p in context.partida
                                    join j in context.jugador on p.id_adivinador equals j.id_jugador
                                    where p.id_estado_partida == 6
                                    select new PartidaDTO
                                    {
                                        IdPartida = p.id_partida,
                                        Codigo = p.codigo,
                                        Idioma = p.idioma,
                                        fecha = (DateTime)p.fecha_creacion,
                                        Nickname = j.usuario,
                                        IdPalabra = p.id_palabra ?? 0,
                                        IdRetador = p.id_retador ?? 0,
                                        IdAdivinador = p.id_adivinador ?? 0,
                                        IdEstadoPartida = p.id_estado_partida ?? 0,
                                        IdIdiomaPartida = p.id_idioma_partida ?? 0
                                    }).ToList();

                    return new ResponsePartidaDTO
                    {
                        success = true,
                        data = new PartidaDTO { Partidas = partidas },
                        message = "Partidas disponibles obtenidas"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponsePartidaDTO
                {
                    success = false,
                    message = $"Error al obtener partidas: {ex.Message}"
                };
            }
        }

        public ResponsePartidaDTO UnirsePartida(string codigoPartida, int idJugador)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    using (var transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        var partida = context.partida
                            .FirstOrDefault(p => p.codigo == codigoPartida && p.id_estado_partida == 6);

                        if (partida == null)
                        {
                            return new ResponsePartidaDTO
                            {
                                success = false,
                                message = "Partida no encontrada o ya no está disponible"
                            };
                        }

                        if (partida.id_retador != null)
                        {
                            return new ResponsePartidaDTO
                            {
                                success = false,
                                message = "La partida ya tiene un retador"
                            };
                        }

                        if (partida.id_adivinador == idJugador)
                        {
                            return new ResponsePartidaDTO
                            {
                                success = false,
                                message = "No puedes unirte a tu propia partida"
                            };
                        }

                        partida.id_retador = idJugador;
                        partida.id_estado_partida = 7; // "Lista para comenzar"
                        context.SaveChanges();

                        var retador = context.jugador.FirstOrDefault(j => j.id_jugador == idJugador);
                        string nicknameRetador = retador?.usuario ?? "Jugador";

                        // Notificar a través del servidor de sockets
                        Program.NotificarUnionRetador(codigoPartida, nicknameRetador).Wait();

                        transaction.Commit();

                        return new ResponsePartidaDTO
                        {
                            success = true,
                            message = $"{nicknameRetador} se ha unido a la partida",
                            data = new PartidaDTO
                            {
                                IdPartida = partida.id_partida,
                                Codigo = partida.codigo,
                                Idioma = partida.idioma,
                                fecha = partida.fecha_creacion ?? DateTime.MinValue,
                                Nickname = partida.jugador != null ? partida.jugador.usuario : "Anónimo",
                                IdPalabra = partida.id_palabra ?? 0,
                                IdRetador = partida.id_retador ?? 0,
                                IdAdivinador = partida.id_adivinador ?? 0,
                                IdEstadoPartida = partida.id_estado_partida ?? 0,
                                IdIdiomaPartida = partida.id_idioma_partida ?? 0
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResponsePartidaDTO
                {
                    success = false,
                    message = $"Error al unirse a la partida: {ex.Message}"
                };
            }
        }

        public ResponsePartidaDTO ObtenerEstadoPartida(string codigoPartida)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var query = from p in context.partida
                                join jAdivinador in context.jugador on p.id_adivinador equals jAdivinador.id_jugador
                                join jRetador in context.jugador on p.id_retador equals jRetador.id_jugador into retadorGroup
                                from jRetador in retadorGroup.DefaultIfEmpty()
                                join pal in context.palabra on p.id_palabra equals pal.id_palabra
                                where p.codigo == codigoPartida && (p.id_estado_partida == 6 || p.id_estado_partida == 7)
                                select new
                                {
                                    Partida = p,
                                    Anfitrion = jAdivinador,
                                    Retador = jRetador,
                                    Palabra = pal,
                                    Pista = pal.pista,
                                    PistaEn = pal.descripcion_en
                                };

                    var result = query.FirstOrDefault();

                    if (result == null)
                    {
                        return new ResponsePartidaDTO
                        {
                            success = false,
                            message = "Partida no encontrada o no está en estado de espera"
                        };
                    }

                    var partida = result.Partida;
                    var anfitrion = result.Anfitrion;
                    var retador = result.Retador;
                    var palabra = result.Palabra;

                    return new ResponsePartidaDTO
                    {
                        success = true,
                        partida = new PartidaDTO
                        {
                            IdPartida = partida.id_partida,
                            Codigo = partida.codigo,
                            Idioma = partida.idioma,
                            fecha = partida.fecha_creacion ?? DateTime.MinValue,
                            Nickname = anfitrion != null ? anfitrion.usuario : "Anónimo",
                            NicknameRetador = retador != null ? retador.usuario : null,
                            IdPalabra = partida.id_palabra ?? 0,
                            IdRetador = partida.id_retador ?? 0,
                            IdAdivinador = partida.id_adivinador ?? 0,
                            IdEstadoPartida = partida.id_estado_partida ?? 0,
                            IdIdiomaPartida = partida.id_idioma_partida ?? 0,
                            Palabra = palabra?.nombre?.ToUpper(),
                            Pista = palabra.pista,
                            PistaEn = palabra.descripcion_en
                        },
                        message = "Estado de la partida obtenido"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponsePartidaDTO
                {
                    success = false,
                    message = $"Error al obtener el estado de la partida: {ex.Message}"
                };
            }
        }

        public ResponseResultadoDTO GuardarResultado(int idJugador, int idPartida, bool gano, bool esAbandono = false)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var partida = context.partida.FirstOrDefault(p => p.id_partida == idPartida);
                    if (partida == null)
                    {
                        return new ResponseResultadoDTO
                        {
                            success = false,
                            message = "Partida no encontrada."
                        };
                    }

                    // Determinar los puntos según el caso
                    int puntos = esAbandono ? -3 : (gano ? 10 : 5);

                    // Determinar el oponente
                    int idOponente = idJugador == partida.id_adivinador ? partida.id_retador ?? 0 : partida.id_adivinador ?? 0;
                    var oponente = context.jugador.FirstOrDefault(j => j.id_jugador == idOponente);
                    string usuarioOponente = oponente?.usuario ?? "Desconocido";

                    // Guardar en la tabla resultado
                    var resultado = new resultado
                    {
                        id_jugador = idJugador,
                        id_partida = idPartida,
                        gano = gano,
                        puntos = puntos
                    };

                    context.resultado.Add(resultado);
                    context.SaveChanges();

                    // Preparar respuesta con ResultadoDTO
                    var resultadoDTO = new ResultadoDTO
                    {
                        FechaPartida = (DateTime)partida.fecha_creacion,
                        PalabraAdivinar = context.palabra.FirstOrDefault(p => p.id_palabra == partida.id_palabra).nombre,
                        Gano = gano,
                        Puntos = puntos,
                        UsuarioOponente = usuarioOponente
                    };

                    return new ResponseResultadoDTO
                    {
                        success = true,
                        message = "Resultado guardado exitosamente.",
                        data = resultadoDTO
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseResultadoDTO
                {
                    success = false,
                    message = $"Error al guardar resultado: {ex.Message}"
                };
            }
        }

        public ResponseEstadisticaDTO ObtenerEstadisticasPorJugador(int idJugador)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var estadisticas = (from r in context.resultado
                                        join p in context.partida on r.id_partida equals p.id_partida
                                        join j1 in context.jugador on r.id_jugador equals j1.id_jugador
                                        join j2 in context.jugador on p.id_adivinador equals j2.id_jugador // Anfitrión
                                        join j3 in context.jugador on p.id_retador equals j3.id_jugador into retadorGroup // Retador (puede ser null)
                                        from j3 in retadorGroup.DefaultIfEmpty()
                                        join pal in context.palabra on p.id_palabra equals pal.id_palabra
                                        where r.id_jugador == idJugador
                                        select new EstadisticaDTO
                                        {
                                            FechaCreacion = (DateTime)p.fecha_creacion,
                                            Puntos = r.puntos,
                                            Resultado = r.gano ? "Victoria" : "Derrota",
                                            Nickname = idJugador == p.id_adivinador ? (j3.usuario ?? "Desconocido") : j2.usuario
                                        }).ToList();

                    return new ResponseEstadisticaDTO
                    {
                        success = true,
                        data = new EstadisticaDTO { Estadisticas = estadisticas },
                        message = "Partidas disponibles obtenidas"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseEstadisticaDTO
                {
                    success = false,
                    message = $"Error al obtener palabras: {ex.Message}"
                };
            }
        }
        public ResponseDTO RegistrarJugador(JugadorDTO nuevoJugador)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var recordPlayer = new jugador
                    {
                        usuario = nuevoJugador.usuario,
                        nombre = nuevoJugador.nombre,
                        fecha_nacimiento = nuevoJugador.fecha_nacimiento,
                        telefono = nuevoJugador.telefono,
                        correo = nuevoJugador.correo,
                        contrasena = nuevoJugador.contraseña,
                        puntuacion = 0
                    };

                    context.jugador.Add(recordPlayer);
                    context.SaveChanges();

                    return new ResponseDTO
                    {
                        success = true,
                        message = "Jugador registrado exitosamente."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    success = false,
                    message = $"Error al registrar jugador: {ex.Message}"
                };
            }
        }
            
        public ResponseDTO ActualizarJugador(JugadorDTO jugadorActualizado)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var jugador = context.jugador.FirstOrDefault(j => j.id_jugador == jugadorActualizado.id_jugador);
                    if (jugador == null)
                    {
                        return new ResponseDTO
                        {
                            success = false,
                            message = "Jugador no encontrado."
                        };
                    }
                    jugador.usuario = jugadorActualizado.usuario;
                    jugador.nombre = jugadorActualizado.nombre;
                    jugador.fecha_nacimiento = jugadorActualizado.fecha_nacimiento;
                    jugador.telefono = jugadorActualizado.telefono;
                    context.SaveChanges();
                    return new ResponseDTO
                    {
                        success = true,
                        message = "Jugador actualizado exitosamente."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    success = false,
                    message = $"Error al actualizar jugador: {ex.Message}"
                };
            }
        }

        public ResponsePartidaDTO CancelarPartida(string codigoPartida)
        {
            try
            {
                using (var context = new HangmanEntidades())
                {
                    var partida = context.partida
                        .FirstOrDefault(p => p.codigo == codigoPartida);

                    if (partida == null)
                    {
                        return new ResponsePartidaDTO
                        {
                            success = false,
                            message = "Partida no encontrada o no pertenece al adivino"
                        };
                    }

                    partida.id_estado_partida = 3; // Cancelada
                    context.SaveChanges();

                    return new ResponsePartidaDTO
                    {
                        success = true,
                        message = "Partida cancelada exitosamente"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponsePartidaDTO
                {
                    success = false,
                    message = $"Error al cancelar la partida: {ex.Message}"
                };
            }
        }
    }
}
