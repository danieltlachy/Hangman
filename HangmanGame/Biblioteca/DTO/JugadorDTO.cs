using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.DTO
{
    [DataContract]
    public class JugadorDTO
    {
        [DataMember]
        public int id_jugador { get; set; }
        [DataMember]
        public string correo { get; set; }
        [DataMember]
        public string contraseña { get; set; } // Solo para la solicitud
        [DataMember]
        public string usuario { get; set; }
        [DataMember]
        public string nombre { get; set; }
        [DataMember]
        public DateTime fecha_nacimiento { get; set; }
        [DataMember]
        public string telefono { get; set; }
        [DataMember]
        public int puntuacion { get; set; }
    }
}
