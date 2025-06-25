using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Biblioteca.DTO
{
    [DataContract]
    public class ResponseResultadoDTO
    {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public ResultadoDTO data { get; set; }
    }
}
