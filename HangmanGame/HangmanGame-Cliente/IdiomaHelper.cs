using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HangmanGame_Cliente
{
    public static class IdiomaHelper
    {
        public static string IdiomaActual { get; private set; } = "es";

        public static void CambiarIdioma(string idioma)
        {
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(idioma))
                        throw new ArgumentNullException(nameof(idioma), "El valor del idioma no puede ser nulo ni vacío.");

                    var culture = new CultureInfo(idioma); 

                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;

                    Application.Current.Resources.MergedDictionaries.Clear();
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri($"/Dictionary-{idioma}.xaml", UriKind.Relative)
                    };
                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CambiarIdioma] Error al cambiar el idioma: {ex.Message}");
                    //MessageBox.Show($"[CambiarIdioma] Error al cambiar el idioma: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static void AlternarIdioma()
        {
        IdiomaActual = IdiomaActual == "es" ? "en" : "es";

        if (string.IsNullOrWhiteSpace(IdiomaActual))
            throw new InvalidOperationException("IdiomaActual no puede ser nulo o vacío.");

        CambiarIdioma(IdiomaActual);
        }
    }
}
