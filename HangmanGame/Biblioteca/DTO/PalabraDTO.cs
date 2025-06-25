using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.DTO
{
    [DataContract]
    public class PalabraDTO
    {
        [DataMember]
        public int IdPalabra { get; set; }
        [DataMember]
        public string nombre { get; set; }
        [DataMember]
        public string nombre_en { get; set; }
        [DataMember]
        public string pista { get; set; }
        [DataMember]
        public string descripcion { get; set; }
        [DataMember]
        public string descripcion_en { get; set; }
        [DataMember]
        public List<string> Palabras { get; set; }
    }

    [DataContract]
    public class Palabras
    {
        [DataMember]
        public List<PalabraDTO> PalabrasConId { get; set; }
    }
}
