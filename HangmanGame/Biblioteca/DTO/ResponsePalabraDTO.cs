using System.Runtime.Serialization;

namespace Biblioteca.DTO
{
    [DataContract]
    public class ResponsePalabraDTO
    {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public PalabraDTO data { get; set; }
    }

    [DataContract]
    public class ResponsePalabrasConIdDTO
    {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public Palabras data { get; set; }
    }
}
