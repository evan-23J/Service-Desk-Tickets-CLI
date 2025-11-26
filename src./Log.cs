
using System.Text.Json;

//Realizado por Evan Tellado y Gian Rivera

namespace ServiceDeskTickets
{
    public class Log
    {
        
        private static string path = "log.json";
        public string username { get; set; }
        public string action { get; set; }
        public string editor { get; set; }
        public DateTime time { get; set; }

        public Log() //Evan 
        {
            if (!File.Exists(path)) //verifica si el archivo existe, si no lo crea
            {
                File.WriteAllText(path, "[]"); //crear archivo
                Console.WriteLine("Archivo log.json creado correctamente");
            }
        }

        public static void log(User user, string action) //metodo para crear un log
        {
            List<Log> logs = LoadAll();

            logs.Add(new Log
            {
                username = user.GetUsername(),
                action = action,
                time = DateTime.Now
            });

            SaveAll(logs);
        }

        private static List<Log> LoadAll() //devuelve todos los logs
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Log>>(json) ?? new List<Log>();
        }

        private static void SaveAll(List<Log> logs) //graba los logs en el archivo
        {
            string json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
