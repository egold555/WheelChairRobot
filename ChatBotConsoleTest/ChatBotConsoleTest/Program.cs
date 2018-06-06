using AIMLbot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            new ChatBot().run();
        }
    }

    class ChatBot
    {
        const string UserId = "user";
        private Bot AimlBot;
        private User myUser;

        public void run()
        {
            Console.WriteLine("Program > Loading bot...");
            AimlBot = new Bot();
            myUser = new User(UserId, AimlBot);

            AimlBot.loadSettings();
            AimlBot.isAcceptingUserInput = false;

            //https://docs.google.com/document/d/1wNT25hJRyupcG51aO89UcQEiG-HkXRXusukADpFnDs4/pub
            AimlBot.loadAIMLFromFiles();
            AimlBot.isAcceptingUserInput = true;
            Console.WriteLine("Bot > Hi");
            while (true)
            {
                var input = Console.ReadLine();
               
                if (input != null && input.Length != 0)
                {
                    if(input == "exit")
                    {
                        
                        break;
                    }
                    Console.WriteLine("User > " + input);
                    String output = getOutput(input);
                    Console.WriteLine("Bot > " + output);
                    Console.WriteLine(" ");
                }

            }
        }

        private String getOutput(String input)
        {
            Request r = new Request(input, myUser, AimlBot);
            Result res = AimlBot.Chat(r);
            return (res.Output);
        }
    }
}
