using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.DTO
{
    [DataContract]
    public class ResultadoDTO
    {
        [DataMember]
        public DateTime FechaPartida { get; set; }
        [DataMember]
        public string PalabraAdivinar { get; set; }
        [DataMember]
        public bool Gano { get; set; }
        [DataMember]
        public int Puntos { get; set; }
        [DataMember]
        public string UsuarioOponente { get; set; }
    }
}
