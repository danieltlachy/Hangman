using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Biblioteca.DTO
{
    [DataContract]
    public class PartidaDTO
    {
        [DataMember]
        public int IdPartida { get; set; }
        [DataMember]
        public string Codigo { get; set; }
        [DataMember]
        public string Idioma { get; set; }
        [DataMember]
        public int IdPalabra { get; set; }
        [DataMember]
        public int IdRetador { get; set; }
        [DataMember]
        public int IdAdivinador { get; set; }
        [DataMember]
        public int IdEstadoPartida { get; set; }
        [DataMember]
        public int IdIdiomaPartida { get; set; }
        [DataMember]
        public string Nickname { get; set; }
        [DataMember]
        public string NicknameRetador { get; set; }
        [DataMember]
        public string Pista { get; set; }
        [DataMember]
        public string PistaEn { get; set; }
        [DataMember]
        public DateTime fecha { get; set; }
        [DataMember]
        public string FechaPartida { get; set; }
        [DataMember]
        public string Palabra { get; set; }
        [DataMember]
        public List<PartidaDTO> Partidas { get; set; }
    }
}
