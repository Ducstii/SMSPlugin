using Exiled.API.Features;
using CommandSystem;
using System;
using System.Linq;

namespace SMSPLUGIN.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class SMSCommand : ICommand
    {
        public string Command => "sms";
        public string[] Aliases => new string[] { "text", "message" };
        public string Description => "SMS system commands";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Check if SMS system is enabled
            if (!SMSPlugin.Instance.SMSManager.IsSystemEnabled)
            {
                response = "SMS system is currently disabled.";
                return false;
            }

            // Get the player from the sender
            Player player = null;
            
            if (sender is Exiled.API.Features.Player playerSender)
            {
                player = playerSender;
            }
            else if (sender is CommandSender commandSender)
            {
                player = Player.Get(commandSender);
            }

            if (player == null)
            {
                response = "This command can only be used by players.";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = SMSPlugin.Instance.SMSManager.GetHelpMessage(player);
                return true;
            }

            string command = arguments.At(0).ToLower();

            switch (command)
            {
                case "help":
                    response = SMSPlugin.Instance.SMSManager.GetHelpMessage(player);
                    return true;

                case "contacts":
                    response = SMSPlugin.Instance.SMSManager.GetContactsMessage(player);
                    return true;

                case "send":
                    if (arguments.Count < 3)
                    {
                        response = "Usage: sms send <number> <message>";
                        return false;
                    }
                    
                    string receiverNumber = arguments.At(1);
                    string message = string.Join(" ", arguments.Skip(2));
                    
                    if (message.Length > SMSPlugin.Instance.Config.MaxMessageLength)
                    {
                        response = $"Message too long! Maximum {SMSPlugin.Instance.Config.MaxMessageLength} characters.";
                        return false;
                    }
                    
                    response = SMSPlugin.Instance.SMSManager.SendMessage(player, receiverNumber, message);
                    return true;

                case "history":
                    response = SMSPlugin.Instance.SMSManager.GetHistoryMessage(player);
                    return true;

                default:
                    response = "Unknown command. Use 'sms help' for available commands.";
                    return false;
            }
        }
    }
} 