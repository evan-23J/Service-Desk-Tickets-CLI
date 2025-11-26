using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ServiceDeskTickets;

/*Realizado por Evan Tellado y Gian Rivera
Universidad Interamericana de Puerto Rico, Recinto de Bayamon.*/

public class Program
{
    private bool loggedIn;

    public void SetLoggedIn(bool loggedIn)
    {
        this.loggedIn = loggedIn;
    }
    public bool GetLoggedIn()
    {
        return this.loggedIn;
    }
    public static string HashPass(string pass) //recibe plain txt
    {
        using (SHA256 sha256 = SHA256.Create()) //se crea instancia de SHA256
        {
            //Convierte el string en byte array
            byte[] bytes = Encoding.UTF8.GetBytes(pass);
            //Gets Hash
            byte[] hashbytes = sha256.ComputeHash(bytes);
            //Convertir a hexadecimal?
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hashbytes)
            {
                stringBuilder.Append(b.ToString("x2")); //convierte los bytes en texto en formato hexadecimal de 2 digitos
            }

            return stringBuilder.ToString(); //devolvemos la cadena a string
        }
    }

    public void ShowOpt(int opt)
    {
        if (opt == 1)
        {
            Console.WriteLine("1. Registrarse | 2. Log In | 3. Salir");
        }
        else if (opt == 2)
        {
            Console.WriteLine("1. Ver tus tickets | 2. Someter un ticket | 3. Editar un ticket | 4. Log Out");
        }
        else if (opt == 3)
        {
            //opciones admin/agent
            Console.WriteLine("1. Ver tickets | 2. Editar un ticket | 3. Log Out");
        }
    }

    public void CallOptions(User Nuser) //delegacion a otros metodos
    {
        if (Nuser.GetRole() == "User")
        {
            UserActions(Nuser);
        }
        else if (Nuser.GetRole() == "Agent")
        {
            AgentOpt(Nuser);
        }
        else if (Nuser.GetRole() == "Admin")
        {
            AdminOpt(Nuser);
        }
    }

    public void AgentOpt(User currentUser) 
    {
        Tickets newTicket = new Tickets();
        List<Tickets> tickets = newTicket.LoadAll();
        Tickets currentTicket;
        Console.WriteLine("*****-Agent Dashboard-*****");
        ShowOpt(3);
        ConsoleKey key = Console.ReadKey(true).Key;

        TryParseConsoleKey(key, out int opt);

        switch (opt)
        {
            case 1:
                if (tickets.Count > 0)
                {
                    Console.WriteLine("Showing All Tickets: ");
                    newTicket.ShowTickets();
                }
                else
                {
                    Console.WriteLine("No tickets.");
                }
                break;
            case 2:
                if (tickets.Count > 0)
                {
                    Console.WriteLine("Showing All Tickets: ");
                    newTicket.ShowTickets();
                    Console.WriteLine("Escoga el numero del ticket:");
                    int ticketIndex = GetValidOption(1, tickets.Count) - 1;
                    currentTicket = tickets.ElementAt(ticketIndex);

                    newTicket.EditTicket(currentUser, currentTicket, tickets); 
                }
                else
                {
                    Console.WriteLine("No tickets");
                }
                break;
            case 3:
                SetLoggedIn(false);
                break;
            default:
                Console.WriteLine("Hubo un error...");
                break;
        }

    }

    public void AdminOpt(User currentUser) 
    {
        Console.WriteLine("(1) Manejar Usuarios  | (2) Manejar Tickets | (3) Log Out");
        int inpt = GetValidOption(1, 3);

        if (inpt == 1)
        {
            ManageAgents();
        }
        else if (inpt == 2)
        {
            AgentOpt(currentUser);
        }
        else if (inpt == 3) 
        {
            SetLoggedIn(false);
        }
    }

    public void ManageAgents() //se llama metodo para crear o editar agentes si el role=="Admin"
    {
        Console.WriteLine("(1) Crear un agente  |  (2) Editar un agente");
        int inpt = GetValidOption(1, 2);
        bool cont;
        if (inpt == 1)
        {
            do
            {
                Console.WriteLine("Ingrese nombre de usuario del agente: ");
                string agent_username = Console.ReadLine().Trim();
                Console.WriteLine("Ingrese contrasena temporera del agente: ");
                string agent_pass = Console.ReadLine().Trim();

                if (ValidateInput(agent_username) && ValidateInput(agent_pass) && !checkUsername(agent_username))
                {
                    agent_pass = HashPass(agent_pass);
                    User newAgent = new User();
                    newAgent.SetUsername(agent_username);
                    newAgent.SetPass(agent_pass);
                    newAgent.SetRole("Agent");
                    User.SaveUser(newAgent);
                    Console.WriteLine("Agente creado exitosamente.");
                    cont = true;
                }
                else
                {
                    cont = false;
                }
            } while (!cont);

        }
        else if (inpt == 2)
        {
            List<User> agents = User.LoadAllAgents();
            User currentAgent = new User();
            User.ShowAgents(agents);
            int index = GetValidOption(1, agents.Count);
            currentAgent = agents[index - 1];

            User.EditAgent(currentAgent);
        }
    }

    public static bool ContainsInvalidTerm(string? input) //validacion contra SQLI
    {
        bool invalidTerm = false;
        string upperInput = input.ToUpper();

        string[] invalidTerms =
        {
            "*", ";", "SELECT", "WHERE", "OR", "=", "==", "<", ">"
        };

        if (string.IsNullOrEmpty(upperInput) || upperInput.Contains("*") || upperInput.Contains(";") || upperInput.Contains("=") )
        {
            invalidTerm = true;
        }
        else
        {
            foreach (string term in invalidTerms)
            {
                if (term == upperInput)
                {
                    invalidTerm = true;
                }
            }
        }

        return invalidTerm;
    }

    public int GetValidOption(int min, int max) // obliga al usuario a elegir una opcion valida dado un rango
    {
        int option = -1;
        bool isValid = false;

        do
        {
            Console.WriteLine($"Ingrese una opción entre {min} y {max}:");
            string input = Console.ReadLine();

            // Validar que sea numero y este en rango
            if (ValidateInput(input) && int.TryParse(input, out option) && option >= min && option <= max)
            {
                isValid = true;
            }
            else
            {
                Console.WriteLine("Input invalido. Intentelo nuevamente.");
            }

        } while (!isValid);

        return option;
    }


    public static bool ValidateInput(string? unvalidinput) //validacion contra null, SQLI y lenght para prevenir overflow
    {
        bool validinput = false;

        while (!validinput)
        {

            if (ContainsInvalidTerm(unvalidinput) || string.IsNullOrEmpty(unvalidinput) || unvalidinput.Length > 32)
            {
                Console.WriteLine("Input Invalido. Intentelo nuevamente: \n");
                unvalidinput = Console.ReadLine();
                validinput = false;
            }
            else
            {
                validinput = true;
            }

        }

        return validinput;
    }

    public static bool checkUsername(string unchecked_username) //valida que un username no se encuentre ya en el archivo json y obliga al usuario a elegir un username valido.
    {
        bool HasUsername;
        List<User> users = User.LoadAll();

        HasUsername = users.Exists(u => u.username == unchecked_username);

        while (HasUsername)
        {
            Console.WriteLine("User already taken!");
            unchecked_username = Console.ReadLine();
            ValidateInput(unchecked_username);
            HasUsername = users.Exists(u => u.username == unchecked_username);
        }

        return HasUsername;
    }

    public void UserActions(User user) //logica de las acciones que puede tomar un user con role=="User"
    {
        Tickets newTicket = new Tickets();
        List<Tickets> tickets;
        Tickets currentTicket;
        string[] flags = { "CRITICAL", "HIGH", "MEDIUM", "LOW" };

        Console.WriteLine("*****-User Menu-*****");
        ShowOpt(2);
        ConsoleKey key = Console.ReadKey(true).Key;

        TryParseConsoleKey(key, out int opt);

        switch (opt)
        {
            case 1: 
                tickets = newTicket.LoadUserTickets(user); //ver los tickets de ese user

                if (tickets.Count > 0)
                {
                    Console.WriteLine("Showing All Your Tickets: ");
                    newTicket.ShowUserTickets(user.GetUsername());
                }
                else
                {
                    Console.WriteLine("Someta un ticket para ver sus tickets.");
                }
                break;

            case 2:
                int i = 1;
                Console.WriteLine("Introduzca la descripcion del problema:"); //someter un ticket
                string desc = Console.ReadLine().Trim();
                Console.WriteLine("Escoga el flag:");
                foreach (string flag in flags)
                {
                    Console.Write(i + ") " + flag + " | ");
                    i++;
                }
                Console.WriteLine();
                int inpt = GetValidOption(1, flags.Count()) - 1;

                if (ValidateInput(desc))
                {
                    newTicket.SubmitTicket(DateTime.Now, desc, "Pendiente", user.GetUsername(), flags[inpt]);
                    Console.WriteLine("Ticket creado exitosamente!");
                }
                break;

            case 3:
                tickets = newTicket.LoadUserTickets(user); //se le permite escoger un ticket y se delega a EditTicket()
                if (tickets.Count > 0)
                {
                    newTicket.ShowUserTickets(user.GetUsername());
                    Console.WriteLine("Escoga el numero del ticket:");
                    int ticketNum = GetValidOption(1, tickets.Count()) - 1;
                    currentTicket = tickets.ElementAt(ticketNum);

                    newTicket.EditTicket(user, currentTicket, tickets); 
                }
                else
                {
                    Console.WriteLine("No tiene tickets.");
                }
                break;
            case 4:
                Console.WriteLine("Sesion finalizada.");
                SetLoggedIn(false);
                break;
            default:
                Console.WriteLine("Hubo un error...");
                break;
        }
    }

    public static bool TryParseConsoleKey(ConsoleKey key, out int number)
    {
        bool parsed = false;
        number = -1;
        if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
        {
            number = key - ConsoleKey.D0;
            parsed = true;
        }

        if (key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9)
        {
            number = key - ConsoleKey.NumPad0;
            parsed = true;
        }

        return parsed;
    }

    public bool Register(User user) //valida que el input sea valido, username unico, password valido y lo guarda en el archivo
    {
        Console.WriteLine("Introduzca Usuario: \n");
        string? unchecked_username = Console.ReadLine().Trim();
        bool HasUsername;
        bool registered = false;

        if (ValidateInput(unchecked_username))
        {
            if (File.Exists("userdata.json"))
            {
                List<User> users = User.LoadAll();

                HasUsername = users.Exists(u => u.username == unchecked_username);

                while (HasUsername)
                {
                    Console.WriteLine("User already taken!");
                    unchecked_username = Console.ReadLine().Trim();
                    ValidateInput(unchecked_username);
                    HasUsername = users.Exists(u => u.username == unchecked_username);
                }

                user.SetUsername(unchecked_username);
            }

        }
        else
        {
            Console.WriteLine("Nombre de usuario invalido.");
            registered = false;
        }

        Console.WriteLine("Introduzca Contrasena: \n");
        string unchecked_pass = Console.ReadLine().Trim();

        while (!ValidateInput(unchecked_pass)) //obliga al usuario a escoger una contrasena valida
        {
            Console.WriteLine("Password Invalido.");
            unchecked_pass = Console.ReadLine().Trim();
            ValidateInput(unchecked_pass);
        }
        string hashPass = HashPass(unchecked_pass);
        user.SetPass(hashPass);
        user.SetRole("User");

        if (!string.IsNullOrEmpty(user.GetUsername()) && !string.IsNullOrEmpty(user.GetPassword())) 
        {
            User.SaveUser(user);
            Console.WriteLine("Usuario registrado exitosamente.");
            registered = true;
        }

        return registered;
    }
    public bool LogIn(User LogUser)     //recibe la informacion del user y valida que existan en el archivo, si no devuelve falso
    {
        List<User> users = User.LoadAll();
        string hashedpass = "";
        bool loggedin = false;

        Console.WriteLine("Nombre de usuario: \n");
        string unchecked_username = Console.ReadLine().Trim();

        Console.WriteLine("Contrasena: \n");
        string unchecked_pass = Console.ReadLine().Trim();

        bool validpass = ValidateInput(unchecked_pass);
        bool validusername = ValidateInput(unchecked_username);

        if (validpass)
        {
            hashedpass = HashPass(unchecked_pass);  //se hace hash al password 
        }

        if (!validusername || !validpass)
        {
            Console.WriteLine("Error!");
            loggedin = false;
        }
        else
        {
            bool contain = users.Exists(u => u.username == unchecked_username && u.password == hashedpass); //se realiza la comparacion de hashes y usernames del archivo

            if (contain)
            {
                LogUser.SetUsername(unchecked_username);
                LogUser.SetPass(hashedpass);
                Console.WriteLine("LogIn exitoso!");
                loggedin = true;
            }
            else
            {
                Console.WriteLine("Usuario o contrasena incorrectos.");
                loggedin = false;
            }
        }

        return loggedin;
    }

   

    static void Main()
    {
        User Nuser = new User();
        Log log = new Log();
        Tickets userTicket = new Tickets();
        List<User> usersLoad = User.LoadAll();

        bool exit = false;

        Program program = new Program(); 
        
        if (usersLoad.Count() == 0) //se crea un admin si no existen users en el archivo
        {
            User Admin = new User();
            Admin.SetUsername("Admin");
            Admin.SetPass("60fe74406e7f353ed979f350f2fbb6a2e8690a5fa7d1b0c32983d1d8b3f95f67");
            Admin.SetRole("Admin");
            User.SaveUser(Admin);
            Console.WriteLine("Admin Creado.");
        }

        while (!exit)
        {
            if (!program.GetLoggedIn())
            {
                program.ShowOpt(1);
                ConsoleKey optKey = Console.ReadKey(true).Key;

                TryParseConsoleKey(optKey, out int opt);

                if (opt < 0 || opt > 3 || opt == 0)
                {
                    Console.WriteLine("Opcion Invalida.");
                }
                else
                {
                    if (opt == 1)
                    {
                        program.SetLoggedIn(program.Register(Nuser));
                        if (program.GetLoggedIn())
                        {
                            Log.log(Nuser, "registered");
                            program.CallOptions(Nuser);
                        }
                        else
                        {
                            Console.WriteLine("Error al registrar al usuario.");
                        }


                    }
                    else if (opt == 2)
                    {
                        program.SetLoggedIn(program.LogIn(Nuser));
                        if (program.GetLoggedIn())
                        {
                            Log.log(Nuser, "Logged In");
                            //Hacerle load a todos los valores del user
                            Nuser = User.LoadUser(Nuser);
                            program.CallOptions(Nuser);
                        }
                    }
                    else if (opt == 3)
                    {
                        Console.WriteLine("Hasta Luego.");
                        Log.log(Nuser, "session ended");
                        exit = true;

                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                program.CallOptions(Nuser);
            }
        }
    }
}
