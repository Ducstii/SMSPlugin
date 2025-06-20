using Exiled.API.Features;
using CommandSystem;
using System;

namespace SMSPLUGIN.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SMSEnableCommand : ICommand
    {
        public string Command => "sms_enable";
        public string[] Aliases => new string[] { "enable_sms" };
        public string Description => "Enable the SMS system";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            SMSPlugin.Instance.SMSManager.EnableSystem();
            response = "SMS system has been enabled.";
            return true;
        }
    }

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SMSDisableCommand : ICommand
    {
        public string Command => "sms_disable";
        public string[] Aliases => new string[] { "disable_sms" };
        public string Description => "Disable the SMS system";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            SMSPlugin.Instance.SMSManager.DisableSystem();
            response = "SMS system has been disabled.";
            return true;
        }
    }

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SMSStatusCommand : ICommand
    {
        public string Command => "sms_status";
        public string[] Aliases => new string[] { "sms_info" };
        public string Description => "Show SMS system status";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var config = SMSPlugin.Instance.Config;
            var smsManager = SMSPlugin.Instance.SMSManager;
            response = $"SMS System Status:\n" +
                      $"System Enabled: {smsManager.IsSystemEnabled}\n" +
                      $"Debug: {config.Debug}\n" +
                      $"Max Message Length: {config.MaxMessageLength}\n" +
                      $"History Message Count: {config.HistoryMessageCount}\n" +
                      $"Save History Between Rounds: {config.SaveHistoryBetweenRounds}";
            return true;
        }
    }
} 