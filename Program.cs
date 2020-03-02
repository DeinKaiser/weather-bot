using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace WeatherBot
{
   
    class Program
    {
        const int Delay = 15000;
        static ITelegramBotClient botClient;
        static Dictionary<string, long> users = new Dictionary<string, long>();
        static string weatherString = new string("");
        public  async static void WeatherThread()
        {
            while(true)
            {
                try
                {
                    string url = "https://www.gismeteo.ua/ua/weather-chernivtsi-4972/now/";
                    var web = new HtmlAgilityPack.HtmlWeb();
                    HtmlDocument doc = web.Load(url);

                    string temparature = doc.DocumentNode.SelectNodes("/html/body/section/div[2]/div/div[1]/div/div[2]/div[1]/div[1]/div/div/div[1]/div[3]/div[1]/span[1]/span")[0].InnerText;
                    string feelsLikeTemparature = doc.DocumentNode.SelectNodes("/html/body/section/div[2]/div/div[1]/div/div[2]/div[1]/div[1]/div/div/div[1]/div[3]/div[2]/span/span[1]")[0].InnerText;
                    string humidity = doc.DocumentNode.SelectNodes("/html/body/section/div[2]/div/div[1]/div/div[2]/div[1]/div[2]/div/div[6]/div[3]/div/div[2]")[0].InnerText;
                    string wind = doc.DocumentNode.SelectNodes("/html/body/section/div[2]/div/div[1]/div/div[2]/div[1]/div[2]/div/div[6]/div[1]/div/div[2]/div[1]")[0].InnerText;
                    weatherString = "Temparature now is " + temparature.Trim() + ", it feels like " + feelsLikeTemparature.Trim() +
                            ". Humidity is " + humidity.Trim() + "% and wind is " + wind.Trim() + "m/s.";
                    Console.WriteLine(weatherString);
                    foreach (var user in users)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: user.Value,
                             text: "Hello " + user.Key + ", here`s your weather:\n " + weatherString
                    );
                    }
                    Thread.Sleep(Delay);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("1126393989:AAF36fKABGN07pWkoHwWBdN_jUGE-qtKobs");
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            WeatherThread();
            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                
                Console.WriteLine($"Received a text message {e.Message.Text} in chat {e.Message.Chat.Id}.");

                if (e.Message.Text == "/start") 
                {
                    await botClient.SendTextMessageAsync(
                       chatId: e.Message.Chat,
                        text: $"Hello {e.Message.Chat.FirstName}! \nWrite /register to start recieving weather messages."
                        );
                }

                if (e.Message.Text == "/register")
                {
                    if (!users.ContainsValue(e.Message.Chat.Id)) {
                        users.Add(e.Message.Chat.FirstName, e.Message.Chat.Id);
                        await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                         text: $"You're successfully registered, now you will receive messages every {Delay} milliseconds!"
                         );
                      }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                         text: "You're already registered!"
                         );
                    }
                
                }

                if (e.Message.Text == "/unregister")
                {
                    if (users.ContainsValue(e.Message.Chat.Id))
                    {
                        users.Remove(e.Message.Chat.FirstName);
                        await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                         text: $"You're successfully unregistered, now you won't receive messages every {Delay} milliseconds!"
                         );
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                         text: "You aren't registered yet!"
                         );
                    }

                }
            }
        }
    }
}
