//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HangmanGame_Servidor.Modelo
{
    using System;
    using System.Collections.Generic;
    
    public partial class resultado
    {
        public int id_resultado { get; set; }
        public int id_jugador { get; set; }
        public int id_partida { get; set; }
        public bool gano { get; set; }
        public int puntos { get; set; }
    
        public virtual jugador jugador { get; set; }
        public virtual partida partida { get; set; }
    }
}
