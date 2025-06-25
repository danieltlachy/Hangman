using Biblioteca.DTO;
using System.Collections.Generic;
using System.ServiceModel;

namespace Biblioteca.Servicios
{
    [ServiceContract]
    public interface IHangmanService
    {
        [OperationContract]
        ResponseDTO Autenticacion(string correo, string contrasena);
        [OperationContract]
        ResponsePalabrasConIdDTO ObtenerPalabrasPorCategoria(string categoria);
        [OperationContract]
        ResponsePartidaDTO CrearPartida(int idAdivinador, int idPalabra, string codigo, int idIdiomaPartida);
        [OperationContract]
        ResponsePartidaDTO ObtenerPartidasDisponibles();
        [OperationContract]
        ResponsePartidaDTO UnirsePartida(string codigoPartida, int idJugador); 
        [OperationContract]
        ResponsePartidaDTO ObtenerEstadoPartida(string codigoPartida);
        [OperationContract]
        ResponseResultadoDTO GuardarResultado(int idJugador, int idPartida, bool gano, bool esAbandono = false);
        [OperationContract]
        ResponseEstadisticaDTO ObtenerEstadisticasPorJugador(int idJugador);
        [OperationContract]
        ResponseDTO RegistrarJugador(JugadorDTO jugadorDTO);
        [OperationContract]
        ResponseDTO ActualizarJugador(JugadorDTO jugadorDTO);
        [OperationContract]
        ResponsePartidaDTO CancelarPartida(string codigoPartida);
    }
}
