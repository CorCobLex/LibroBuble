using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;


namespace ProgMobile
{
    internal class Program
    {
        private static TelegramBotClient bot;
        private static readonly HttpClient client = new();

        private static void Main(string[] args)
        {
            bot = new TelegramBotClient("1997643100:AAHl-wsiHKFPXYabPxXQ47gdrIc3nADNqLo");

            bot.StartReceiving();

            bot.OnMessage += Bot_OnMessage;

            Console.ReadLine();
            bot.StopReceiving();
        }


        private static async void Bot_OnMessage(object? sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Text == "/start")
            {
                bot.SendTextMessageAsync(message.Chat.Id, "Отправьте мне ключевое слово и получите книгу");
                Reply_keybords_main(bot, message);
                return;
            }
              

            static async Task Reply_keybords_main(object? sender, Message e)
            {
                var replyKeyboardMarkUp = new ReplyKeyboardMarkup(
                    new KeyboardButton[][]

                    {
                        new KeyboardButton[] {"Донаты"},
                        new KeyboardButton[] {"Поиск"}
                    })

                {
                    ResizeKeyboard = true
                };

                await bot.SendTextMessageAsync(e.Chat.Id, "", replyMarkup: replyKeyboardMarkUp);
            }

            var action = message.Text.Split(' ').First() switch
            {
                "/keyboard" => Reply_keybords_main(bot, message),
                "Донаты" => Donut(bot, message),
                "Поиск" => FindText(bot, message),

                _ => FindBook(bot, message)
            };

            static async Task Donut(object? sender, Message message)
            {
                await bot.SendTextMessageAsync(message.Chat.Id,
                    "Карта привязана к номеру телeфона\nПринимаются сбербанк и тинькофф\nНомер телефона:89822106138\nСпасибо!");
            }

            static async Task FindText(object? sender, Message message)
            {
                await bot.SendTextMessageAsync(message.Chat.Id,
                    "Введите любой текст(кроме Донаты, Поиск и /keyboard) что бы найти книгу");
            }

            static async Task<Message> FindBook(object? sender, Message message)

            {
                var responce = await client.GetAsync($"https://www.googleapis.com/books/v1/volumes?q={message.Text}");
                var bookJson = await responce.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(bookJson);

                var countFoundBooks = jObject["items"].Children().Count();

                if (countFoundBooks == 0)
                    return await bot.SendTextMessageAsync(message.Chat.Id, $"Извините не удалось найти книг с названием {message.Text}");

                var rand = new Random();

                var bookJsonItem = jObject["items"][rand.Next(0, countFoundBooks - 1)]?["volumeInfo"];

                var book = JsonConvert.DeserializeObject<Book>(bookJsonItem.ToString());

                return await bot.SendTextMessageAsync(message.Chat.Id,
                    $"Название книги: {book.title}\nАвтор(ы) {book.authors?[0]}\nОписание:\n {book.description}\nСсылка на книгу: {book.previewLink}");
            }
        }
    }
}