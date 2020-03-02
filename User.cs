using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Telegram.Bot;

namespace WeatherBot
{
    public class User
    {
        
        public User() { }
        public User(string name, long id)
        {
            Name = name;
            Id = id;
        }
        public bool IsRunning { get; set; } = false;
        public string Name { get; set; }

        public long Id { get; set; }

        public bool workCode { get; set; } = false;
        public int Delay { get; set; } = -1;


        public async void RunMessages(TelegramBotClient botClient, string weatherString)
        {
            Console.WriteLine($"Thread of {Id} user started!");
                while (IsRunning)
                {
                    workCode = true;
                    if (!IsRunning)
                    {
                        workCode = false;
                        break;
                    }
                Console.WriteLine($"Thread of {Id} user isRunning!");
                await botClient.SendTextMessageAsync(
                                chatId: Id,
                                 text: "Hello " + Name + ", here`s your weather:\n " + weatherString
                        );
                    Thread.Sleep(Delay);
                }
            Console.WriteLine($"Thread of {Id} user has ended!");

        }
    }
}
