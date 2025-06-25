using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HangmanGame_Cliente
{
    public static class Seguridad
    {
        private const int MaximoCaracteresContrasena = 45;

        private const int MaximoCaracteresNombreUsuario = 15;

        private const int MilisegundosMaximosParaExpresionRegular = 100;

        private const string PatronContrasena = "^(?=\\w*\\d)(?=\\w*[A-Z])(?=\\w*[a-z])\\S{8,}$";

        private const string PatronNombre = @"^[a-zA-Z0-9]+(?:\s[a-zA-Z0-9]+)?$";

        private const string PatronNombreUsuario = @"^[a-zA-Z0-9]+(?:\s[a-zA-Z0-9]+)?$";

        private const string PatronCorreo = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        private const string PatronTexto = @"^[a-zA-Z\s\-]+$";


        public static bool ExisteLongitudExcedidaEnContrasena(string contrasena)
        {
            bool camposExcedidos = false;

            if (contrasena.Length > MaximoCaracteresContrasena)
            {
                camposExcedidos = true;
            }

            return camposExcedidos;
        }

        public static bool ExisteLongitudExcedidaEnNombreUsuario(string nombreUsuario)
        {
            bool camposExcedidos = false;

            if (nombreUsuario.Length > MaximoCaracteresNombreUsuario)
            {
                camposExcedidos = true;
            }

            return camposExcedidos;
        }

        public static bool ExistenCaracteresInvalidosParaContrasena(string contrasena)
        {
            bool contrasenaInvalida = false;

            if (contrasena.Length < 10 || !Regex.IsMatch(contrasena, PatronContrasena, RegexOptions.None,
                TimeSpan.FromMilliseconds(MilisegundosMaximosParaExpresionRegular)))
            {
                contrasenaInvalida = true;
            }

            return contrasenaInvalida;
        }

        public static bool ExistenCaracteresInvalidosParaNombreUsuario(string nombreUsuario)
        {
            bool resultado = false;

            if (nombreUsuario.Length < 10 || !Regex.IsMatch(nombreUsuario, PatronNombreUsuario, RegexOptions.None,
                TimeSpan.FromMilliseconds(MilisegundosMaximosParaExpresionRegular)))
            {
                resultado = true;
            }

            return resultado;
        }

        public static bool ExistenCaracteresInvalidosParaNombre(string nombre)
        {
            bool resultado = false;

            if (!Regex.IsMatch(nombre, PatronNombre, RegexOptions.None,
                TimeSpan.FromMilliseconds(MilisegundosMaximosParaExpresionRegular)))
            {
                resultado = true;
            }

            return resultado;
        }

        public static bool ExistenCaracteresInvalidosParaCorreo(string correo)
        {
            bool resultado = false;

            if (!Regex.IsMatch(correo, PatronCorreo, RegexOptions.None,
                TimeSpan.FromMilliseconds(MilisegundosMaximosParaExpresionRegular)))
            {
                resultado = true;
            }

            return resultado;
        }

        public static bool ExistenCaracteresInvalidosParaDescripcion(string descripcion)
        {
            bool resultado = false;

            if (!Regex.IsMatch(descripcion, PatronTexto, RegexOptions.None,
                TimeSpan.FromMilliseconds(MilisegundosMaximosParaExpresionRegular)))
            {
                resultado = true;
            }

            return resultado;
        }

        public static bool SonLaMismaContrasena(string contrasena, string contrasenaConfirmacion)
        {

            if (contrasena.Equals(contrasenaConfirmacion))
            {
                return true;
            }

            return false;
        }

        public static bool EsCadenaVacia(string cadena)
        {
            bool resultado = false;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                resultado = true;
            }

            return resultado;
        }
    }
}
