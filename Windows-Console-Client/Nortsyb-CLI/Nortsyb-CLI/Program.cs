using Nortsyb.Lib;
using Nortsyb.Lib.Internal.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nortsyb_CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            start:
            NortsybClient client = new NortsybClient();
            var uname = Prompt("Username: ");
            var password = Prompt("Password: ");

            var success = await client.LoginAsync(uname, password);
            if (!success)
            {
                Console.WriteLine("Error while logging in!");
                Thread.Sleep(4000);
                Console.Clear();
                goto start;
            }

           
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Users:");
            Console.ForegroundColor = ConsoleColor.White;
            var users = await client.ListUsers();
            foreach(var user in users)
            {
                if(user.Username != uname)
                Console.WriteLine(user.Username);
            }

            client.MessageRecieved += new WebSocketSharp.SocketMessageHandler((o, msg) =>
            {
                if (msg.StartsWith("sender:"))
                {
                    var ev = msg.Replace("\0", "");
                    var senderId = int.Parse(ev.Substring(7).Split('m')[0]);
                    var sender = users.Where(x => x.Id == senderId).FirstOrDefault().Username;
                    var message = ev.Substring(ev.IndexOf("msg:") + 4);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{sender}: {message}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            });

            Console.WriteLine("\nSelect a user with :username\n");
            User selectedUser = null;
            while (true)
            {
                var input = Console.ReadLine();
                if (input.StartsWith(":"))
                {
                    var newUser = users.Where(x => x.Username == input.Substring(1)).FirstOrDefault();
                    if (newUser == null) Console.WriteLine("User " + input.Substring(1) + " not found!");
                    else selectedUser = newUser;
                    continue;
                }
                if(selectedUser == null)
                {
                    Console.WriteLine("No user selected!");
                    continue;
                }
                success = await client.SendMessage(selectedUser.Id, input);
                if (!success) Console.WriteLine("Failed to send message");
            }
        }

        private string Prompt(string text)
        {
            Console.Write(text);
            return Console.ReadLine();
        }
    }
}
