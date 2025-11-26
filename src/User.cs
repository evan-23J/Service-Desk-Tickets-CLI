
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

//Realizado por Evan Tellado y Gian Rivera

namespace ServiceDeskTickets
{
    public class User
    {
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; } = "User";
        private static string path = "userdata.json";

        public User() 
        {
            username = "Guest";
            password = "password";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "[]");
                Console.WriteLine("Archivo userdata creado correctamente.");
            }
        }

        public User(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
        

        public void SetPass(string pass)
        {
            password = pass;
        }

        public void SetUsername(string username)
        {
            this.username = username;
        }

        public void SetRole(string role)
        {
            this.role = role;
        }

        public string GetRole()
        {
            return this.role;
        }
        public string GetPassword()
        {
            return this.password;
        }

        public string GetUsername()
        {
            return this.username;
        }
        public static List<User> LoadAll()
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public static User LoadUser(User user)
        {
            List<User> users = LoadAll();
            User findUser = null;
            foreach (User u in users)
            {
                if (u.username == user.username)
                {
                    findUser = u;
                }
            }
            return findUser;
        }
        public static void SaveUser(User user) //Guarda los cambios al usuario en el archivo
        {
            List<User> users = LoadAll();
            users.Add(user);

            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        public void SaveAll(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        public override string ToString()
        {
            return string.Format($"username= {username}, password={password}, role= {role} \n");
        }

        public static List<User> LoadAllAgents() //devuelve todos los usuarios con role=="Agent"
        {
            List<User> agents = new List<User>();
            List<User> AllUsers = LoadAll();

            foreach (User user in AllUsers)
            {
                if (user.GetRole() == "Agent")
                {
                    agents.Add(user);
                }
            }

            return agents;
        }
        public static List<string> GetAllAgents()
        {

            List<User> users = LoadAll();
            List<string> agentUsernames = new List<string>();

            foreach (User user in users)
            {
                if (user.GetRole() == "Agent")
                {
                    agentUsernames.Add(user.GetUsername());
                }
            }

            return agentUsernames;
        }

        public static void ShowAgents(List<User> agents)
        {
            int i = 1;
            foreach (User agent in agents)
            {
                Console.WriteLine(i + ") " + agent.GetUsername());
                i++;
            }
        }
        public static void EditAgent(User agent) //metodo que se invoca si role=="Admin" para logica de la edicion de agents 
        {
            List<User> users = LoadAll();
            int index = -1;
            foreach (User user in users)
            {
                if (user.GetUsername() == agent.GetUsername())
                {
                    index = users.IndexOf(user);
                }
            }

            Console.WriteLine("(1) Cambiar Contrasena | (2) Editar rol");
            ConsoleKey key = Console.ReadKey(true).Key;
            Program.TryParseConsoleKey(key, out int opt);

            switch (opt)
            {
                case 1:
                    string newPassword;
                    Console.WriteLine("Introduzca la nueva contrasena:");
                    newPassword = Console.ReadLine();

                    if (Program.ValidateInput(newPassword))
                    {
                        agent.SetPass(Program.HashPass(newPassword));
                        Console.WriteLine("Contrasena actualizada.");
                    }
                    else
                    {
                        Console.WriteLine("Contrasena invalida");
                    }
                    break;
                case 2:
                    string[] roles = { "Admin", "Agent", "User" };
                    int i = 1;
                    foreach (string role in roles)
                    {
                        Console.WriteLine(i + ") " + role);
                        i++;
                    }
                    Console.WriteLine("Escoga el rol nuevo:");
                    ConsoleKey roleKey = Console.ReadKey(true).Key;

                    Program.TryParseConsoleKey(roleKey, out int indx);

                    agent.SetRole(roles[indx - 1]);
                    Console.WriteLine("Rol actualizado.");
                    break;
                default:
                    Console.WriteLine("Input invalido");
                    break;
            }

            if (index >= 0 && index < users.Count)
            {
                users[index] = agent;
            }
            else
            { 
                Console.WriteLine("Ocurrio un error al editar al agente."); 
            }

            agent.SaveAll(users);
        }
    }
}
