using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

//Realizado por Evan Tellado y Gian Rivera

namespace ServiceDeskTickets
{
    public class Tickets
    {
        private string path = "tickets.json";
        public DateTime creationDate { get; set; }
        public string desc { get; set; }
        public string ticketStatus { get; set; }
        public string username { get; set; }
        public string ticketFlag { get; set; }
        public string ticket_agent { get; set; }


        private string[] statusopt = { "Completado", "En Proceso", "Pendiente", "Cerrado" };
        private string[] flags = { "CRITICAL", "HIGH", "MEDIUM", "LOW" };

        public List<Tickets> LoadAll() // devuelve todos los tickets encontrados en el archivo tickets.json
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Tickets>>(json) ?? new List<Tickets>();
        }

        public List<Tickets> LoadUserTickets(User user) //devuelve una lista con todos los tickets bajo un mismo username
        {
            string json = File.ReadAllText(path);
            List<Tickets> tickets = JsonSerializer.Deserialize<List<Tickets>>(json) ?? new List<Tickets>();
            List<Tickets> userTickets = new List<Tickets>();
            foreach (Tickets ticket in tickets)
            {
                if(user.GetUsername() == ticket.username)
                {
                    userTickets.Add(ticket);
                }
            }
            return userTickets;
        }

        public void SaveAll(List<Tickets> tickets) //graba los cambios en el archivo
        {
            string json = JsonSerializer.Serialize(tickets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public void SetDescription(string description)
        {
            this.desc = description;
        }

        public void SetStatus(int i)
        {
            ticketStatus = statusopt[i -1];
        }

        public void SetTicketAgent(string agent_username)
        {
            ticket_agent = agent_username;
        }
        public void SetFlag(int i)
        {
            ticketFlag = flags[i - 1];
        }

        public string GetStatus()
        {
            return ticketStatus;
        }

        public string[] GetStatusOpt()
        {
            return statusopt;
        }


        public string GetFlag()
        {
            return ticketFlag;
        }

        public string[] GetFlagOpt()
        {
            return flags;
        }

        public void ShowStatusOpt()
        {
            int j = 1;
            foreach (string status in GetStatusOpt())
            {
                Console.WriteLine(j++.ToString() + " " + status);
            }
        }

        public void ShowFlagOpt()
        {
            int j = 1;
            foreach (string flag in GetFlagOpt())
            {
                Console.WriteLine(j++.ToString() + " " + flag);
            }
        }

        public override string ToString()
        {
            return $"User {username}, Flag: {ticketFlag}, Description: {desc}, Status: {ticketStatus}, Date: {creationDate.ToString()}, Assigned Agent: {ticket_agent} ";
        }

        public Tickets()
        {
            if (!File.Exists(path)) //verifica si el archivo existe, si no lo crea
            {
                File.WriteAllText(path, "[]"); //crear archivo
                Console.WriteLine("Archivo tickets.json creado correctamente");
            }
        }

        public void ShowTickets()
        {
            int i = 1;
            List<Tickets> tickets = LoadAll();
            foreach (Tickets entry in tickets)
            {
                Console.WriteLine(i + " " + entry.ToString());
                i++;
            }
        }

        public void ShowUserTickets(string username)
        {
            int i = 1;
            List<Tickets> tickets = LoadAll();
            foreach (Tickets entry in tickets)
            {
                if (entry.username == username)
                {
                    Console.WriteLine(i + " " + entry.ToString());
                    i++;
                }
            }
        }


        public void SubmitTicket(DateTime creationDate, string desc, string status, string assignedUser, string flag) //crea el ticket y lo graba al archivo
        {
            List<Tickets> tickets = LoadAll();

            Tickets ticket = new Tickets
            {
                creationDate = creationDate,
                desc = desc,
                ticketStatus = status,
                username = assignedUser,
                ticketFlag = flag
            };

            tickets.Add(ticket);
            SaveAll(tickets);
        }

        public void EditTicket(User user, Tickets currentTicket, List<Tickets> tickets) //logica sobre las distintas acciones que pueden tomar distintos roles en la edicion de los campos de un ticket
        {
            string inpt;

            if (user.GetRole() == "User")
            {
                Console.WriteLine("1. Actualizar Descripción");
            }
            else if (user.GetRole() == "Agent")
            {
                Console.WriteLine("1. Actualizar Descripción | 2. Actualizar Status | 3. Actualizar Flag");
            }
            else if (user.GetRole() == "Admin")
            {
                Console.WriteLine("1. Actualizar Descripción | 2. Actualizar Status | 3. Actualizar Flag | 4. Borrar | 5. Asignar Agente");
            }

            inpt = Console.ReadLine();

            if (int.TryParse(inpt, out int opt))
            {
                switch (opt)
                {
                    case 1: 
                        Console.WriteLine("Ingrese la nueva descripción:"); //actualizar descripcion
                        string newDesc = Console.ReadLine();
                        if (Program.ValidateInput(newDesc))
                        {
                            currentTicket.SetDescription(newDesc);
                            Console.WriteLine("Descripción actualizada.");
                        }
                        break;

                    case 2: 
                        if (user.GetRole() == "Admin" || user.GetRole() == "Agent") //actualizar status
                        {
                            currentTicket.ShowStatusOpt();
                            Console.WriteLine("Escoja el estado:");
                            string statusInpt = Console.ReadLine();

                            if (int.TryParse(statusInpt, out int statopt) && statopt > 0 && statopt <= currentTicket.GetStatusOpt().Length)
                            {
                                currentTicket.SetStatus(statopt);
                                Console.WriteLine("Estado actualizado.");
                            }
                        }
                        break;

                    case 3: // Flag (solo Agent/Admin)
                        if (user.GetRole() == "Admin" || user.GetRole() == "Agent")
                        {
                            currentTicket.ShowFlagOpt();
                            Console.WriteLine("Escoja la bandera:");
                            string flagInpt = Console.ReadLine();

                            if (int.TryParse(flagInpt, out int flagopt) && flagopt > 0 && flagopt <= currentTicket.GetFlagOpt().Length)
                            {
                                currentTicket.SetFlag(flagopt);
                                Console.WriteLine("Bandera actualizada.");
                                
                            }
                        }
                        break;

                    case 4: // Borrar / Admin
                        if (user.GetRole() == "Admin")
                        {
                            tickets.Remove(currentTicket);
                            Console.WriteLine("Ticket eliminado.");
                        }
                        break;

                    case 5: // Asignar agente /Admin
                        if (user.GetRole() == "Admin")
                        {
                            List<string> agents = User.GetAllAgents();
                            int k = 1;
                            foreach (string agent in agents)
                            {
                                Console.WriteLine($"{k++}. {agent}");
                            }

                            Console.WriteLine("Seleccione el agente:");
                            string agentInpt = Console.ReadLine();

                            if (int.TryParse(agentInpt, out int a) && a > 0 && a <= agents.Count)
                            {
                                currentTicket.SetTicketAgent(agents[a - 1]);
                                Console.WriteLine("Agente asignado exitosamente.");
                            } else
                            {
                                Console.WriteLine("Ocurrio un error al asignar al agente.");
                            }
                        }
                        break;

                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
                currentTicket.SaveAll(tickets); //se guardan los cambios en el archivo json
            }
        }

    }
}
