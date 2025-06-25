using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.DTO
{
    [DataContract]
    public class EstadisticaDTO
    {
        [DataMember]
        public string Nickname { get; set; } // Usuario del oponente
        [DataMember]
        public string Resultado { get; set; } // "Ganó" o "Perdió"
        [DataMember]
        public int Puntos { get; set; }
        [DataMember]
        public DateTime FechaCreacion { get; set; }
        [DataMember]
        public List<EstadisticaDTO> Estadisticas { get; set; }
    }
}
