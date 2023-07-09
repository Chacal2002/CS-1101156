using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;



class Program
{

    static string Desktop = @"C:\Users\guill\Desktop";
    static string Documento = @"C:\Users\guill\Documents";
    static string MPP = @"C:\Users\guill\Desktop\Musica pa probar";
    static string SS = Path.Combine(Desktop, "SS");

    [DllImport("user32.dll")]

    public static extern int GetSystemMetrics(int nIndex);


    static void Main()
    {


        string ScreenShot = Path.Combine(Desktop, "SS");

        if (!Directory.Exists(SS))
        {
            Directory.CreateDirectory(SS);
            Console.WriteLine("Se ha creado el directorio 'SS'.");
        }


        int screenWidth = GetSystemMetrics(0);
        int screenHeight = GetSystemMetrics(1);

        //OrganizarArchivos(Documento);

        Task.Run(() => TomarCapturasPantallaParalelo(screenWidth, screenHeight));

        MoverArchivos(Desktop, @"C:\Users\guill\Desktop\CSPRUE 1");
        MoverArchivos(MPP, @"C:\Users\guill\Desktop\CSPRUE 1");

        OrganizarArchivosPorAnio(@"C:\Users\guill\Desktop\CSPRUE 1");

        MoverArchivos(@"C:\Users\guill\Desktop\CSPRUE 1", @"C:\Users\guill\Desktop\CSPRUE 1\General");

        Console.WriteLine("Proceso de organización finalizado.");



    }

    static void MoverArchivos(string Origen, string Destino)
    {
        string[] elementos = Directory.GetFileSystemEntries(Origen);

        List<Task> tareas = new List<Task>();

        foreach (string elemento in elementos)
        {
            if (File.Exists(elemento) && Path.GetExtension(elemento).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                string directorioDestino = Destino;
                Directory.CreateDirectory(directorioDestino);
                string nuevoNombre = Path.Combine(directorioDestino, Path.GetFileName(elemento));
                File.Move(elemento, nuevoNombre);
                Console.WriteLine($"Archivo {Path.GetFileName(elemento)} organizado correctamente.");
            }
            else if (Directory.Exists(elemento))
            {
                tareas.Add(Task.Run(() => MoverArchivos(elemento, elemento)));
            }
        }

        Task.WaitAll(tareas.ToArray());
    }

    static void OrganizarArchivosPorAnio(string Directorio)
    {
        if (!Directory.Exists(Directorio))
        {
            Console.WriteLine("El directorio de origen no existe.");
            return;
        }

        Directory.CreateDirectory(Directorio);

        string[] archivos = Directory.GetFiles(Directorio, "*.mp3");

        Parallel.ForEach(archivos, archivo =>
        {
            try
            {
                TagLib.File file = TagLib.File.Create(archivo);
                string album = file.Tag.Album;
                int anio = (int)file.Tag.Year;

                if (string.IsNullOrEmpty(album) || anio == 0)
                {
                    Console.WriteLine($"El archivo {Path.GetFileName(archivo)} no tiene información suficiente.");
                    return;
                }

                string directorioAnio = Path.Combine(Directorio, anio.ToString());
                Directory.CreateDirectory(directorioAnio);

                string directorioAlbum = Path.Combine(directorioAnio, album);
                Directory.CreateDirectory(directorioAlbum);

                string nuevoArchivo = Path.Combine(directorioAlbum, Path.GetFileName(archivo));
                File.Copy(archivo, nuevoArchivo);

                Console.WriteLine($"Archivo {Path.GetFileName(archivo)} organizado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar el archivo {Path.GetFileName(archivo)}: {ex.Message}");
            }
        });
    }

    static void TomarCapturasPantallaParalelo(int screenWidth, int screenHeight)
    {
        while (true)
        {
            CapturarPantalla(screenWidth, screenHeight);
            Thread.Sleep(1);
        }
    }

    static void CapturarPantalla(int screenWidth, int screenHeight)
    {
        string nombreArchivo = $"Screenshot_{DateTime.Now:yyyyMMddHHmmss}.png";
        string rutaArchivo = Path.Combine(SS, nombreArchivo);

        using (var screenShot = new Bitmap(screenWidth, screenHeight))
        {
            using (var graphics = Graphics.FromImage(screenShot))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight));
            }

            screenShot.Save(rutaArchivo);
            Console.WriteLine($"Captura de pantalla guardada: {nombreArchivo}");
        }
    }
}