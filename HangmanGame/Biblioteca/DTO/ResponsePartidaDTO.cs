using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.DTO
{
    [DataContract]
    public class ResponsePartidaDTO
    {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public PartidaDTO data { get; set; }
        [DataMember]
        public PartidaDTO partida { get; set; } 
    }

    [DataContract]
    public class PartidasDTO
    {
        [DataMember]
        public List<PartidaDTO> Partidas { get; set; }
    }
}
