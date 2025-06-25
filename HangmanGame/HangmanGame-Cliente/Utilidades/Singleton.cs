using Biblioteca.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangmanGame_Cliente.Utilidades
{
    public class Singleton
    {
        // Instancia única de la clase Singleton (patrón Singleton)
        private static Singleton _instancia;
        private static readonly object _lock = new object();

        // Propiedad para almacenar el Jugador autenticado
        public JugadorDTO JugadorAutenticado { get; private set; }

        // Constructor privado para evitar instanciación directa
        private Singleton()
        {
            JugadorAutenticado = null; // Inicialmente no hay jugador autenticado
        }

        // Propiedad pública para acceder a la instancia única
        public static Singleton Instancia
        {
            get
            {
                // Doble verificación para thread-safety
                if (_instancia == null)
                {
                    lock (_lock)
                    {
                        if (_instancia == null)
                        {
                            _instancia = new Singleton();
                        }
                    }
                }
                return _instancia;
            }
        }

        // Método para establecer el jugador autenticado
        public void AutenticarJugador(JugadorDTO jugador)
        {
            JugadorAutenticado = jugador;
        }
    }
}
