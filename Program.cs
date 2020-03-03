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
        static TelegramBotClient botClient = new TelegramBotClient("BOT_TOKEN");
        static List<User> users = new List<User>();
        static string weatherString = new string("");
        public static void WeatherThread()
        {
            while (true)
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
                    {   if (user.IsRunning && !user.workCode)
                        {
                            user.RunMessages(botClient, weatherString);
                        }
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
            Console.WriteLine($"Successfully initialized!");
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            WeatherThread();
            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
           
            try { 
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

                        if (!users.Exists(x => x.Id == e.Message.Chat.Id)) {
                            var user = new User(e.Message.Chat.FirstName, e.Message.Chat.Id);
                            users.Add(user);
                            await botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: $"You're successfully registered! " +
                                $"\nEnter command /delay and number of minutes to set delay and then enter command /run_messages."
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

                        if (users.Exists(x => x.Id == e.Message.Chat.Id))
                        {
                            users.Remove(users.Find(x => x.Id == e.Message.Chat.Id));
                            await botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat,
                             text: $"You're successfully unregistered, now you won't receive messages!"
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

                    if (e.Message.Text.Contains("/delay"))
                    {
                        int result = 0;
                        int.TryParse(e.Message.Text.Replace("/delay", "").Trim(), out result);
                        // Int32 limit is 2.147.483.647, so (result * 1000* 60 < 2.147.483.647) must be true
                        if ((result <= 0) || (result > 35500))
                        {
                            await botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat,
                             text: "Number is wrong!You will receive messages with previous delay when you enter /run_messages !"
                             );
                        }
                        else
                        {
                            users.Find(x => x.Id == e.Message.Chat.Id).Delay = result * 1000 * 60;
                            users.Find(x => x.Id == e.Message.Chat.Id).IsRunning = false;
                            await botClient.SendTextMessageAsync(
                               chatId: e.Message.Chat,
                                text: $"Delay set!Delay is now {users.Find(x => x.Id == e.Message.Chat.Id).Delay / 1000 / 60} minutes! You won't receive messages untill you enter /run_messages!"
                                );
                        }
                    }

                    if (e.Message.Text == "/run_messages")
                    {
                        if (users.Exists(x => x.Id == e.Message.Chat.Id)){

                            if (users.Find(x => x.Id == e.Message.Chat.Id).Delay == -1)
                            {
                                await botClient.SendTextMessageAsync(
                               chatId: e.Message.Chat,
                                text: "You need to enter /delay at first!"
                                );
                            }
                            users.Find(x => x.Id == e.Message.Chat.Id).IsRunning = true;

                            await botClient.SendTextMessageAsync(
                               chatId: e.Message.Chat,
                                text: $"Weather now is: {weatherString}. You will receive messages every {users.Find(x => x.Id == e.Message.Chat.Id).Delay / 1000 / 60} minutes!"
                                );
                        }
                    }

                    if (e.Message.Text == "/stop_messages")
                    {
                        if (users.Exists(x => x.Id == e.Message.Chat.Id))
                        {
                            users.Find(x => x.Id == e.Message.Chat.Id).IsRunning = false;

                            await botClient.SendTextMessageAsync(
                               chatId: e.Message.Chat,
                                text: $"Weather now is: {weatherString}. You won't receive messages anymore!"
                                );
                        }
                    }

                    if (e.Message.Text.Contains("/weather")) 
                    {
                        await botClient.SendTextMessageAsync(
                           chatId: e.Message.Chat,
                            text: $"Weather now is: {weatherString}."
                            );
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
