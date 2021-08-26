using DiegoG.TelegramBot;
using DiegoG.TelegramBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace D_API.TelegramBot
{
    [BotCommand]
    public class NewKey : IBotCommand
    {
        public TelegramBotCommandClient Processor { get; set; }

        public string HelpExplanation => "Generates a new key";

        public string HelpUsage => "/newkey (name) (roles)";

        public IEnumerable<OptionDescription>? HelpOptions { get; } = new OptionDescription[]
        {
            new("name", "The name of the claiming party"),
            new("roles", "A comma separated list of roles i.e.: admin,root,mod")
        };

        public string Trigger => "/newkey";

        public string? Alias => "/nk";

        public async Task<CommandResponse> Action(BotCommandArguments args)
        {
            var a = args.Arguments;
            var r = a.Length is > 2 ? $"```{Startup.Auth.Authenticate(a[1], a[2])}```" : "Not enough arguments";
            return new CommandResponse(false, b => b.SendTextMessageAsync(args.Message.Chat, r, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2));
        }

        public Task<CommandResponse> ActionReply(BotCommandArguments args)
        {
            throw new NotImplementedException();
        }

        public void Cancel(User user)
        {
            throw new NotImplementedException();
        }
    }
}
