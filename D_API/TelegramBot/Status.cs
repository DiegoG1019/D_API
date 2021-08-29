using DiegoG.TelegramBot;
using DiegoG.TelegramBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Humanizer;

namespace D_API.TelegramBot
{
    [BotCommand]
    public class Status : IBotCommand
    {
        public TelegramBotCommandClient Processor { get; set; }

        public string HelpExplanation => "Obtains the current status of D_API";

        public string HelpUsage => "/status";

        public IEnumerable<OptionDescription>? HelpOptions => null;

        public string Trigger => "/status";

        public string? Alias => null;

        public Task<CommandResponse> Action(BotCommandArguments args)
            => Task.FromResult(new CommandResponse(args, false, $"Alive and well for {Program.Runtime.Humanize(3)}\nStartup took {Program.StartupTime.Humanize(3)} to complete\n\nRunning D_API v.{Program.Version}"));

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
