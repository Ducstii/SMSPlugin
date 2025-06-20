using Exiled.API.Interfaces;
using System.ComponentModel;
using System.Collections.Generic;

namespace SMSPLUGIN
{
    public class Config : IConfig
    {
        [Description("Whether the SMS plugin is enabled")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether to debug the plugin")]
        public bool Debug { get; set; } = false;

        [Description("Maximum message length allowed")]
        public int MaxMessageLength { get; set; } = 200;

        [Description("Number of recent messages to show in history")]
        public int HistoryMessageCount { get; set; } = 5;

        [Description("Welcome message duration in seconds")]
        public float WelcomeMessageDuration { get; set; } = 10f;

        [Description("New message notification duration in seconds")]
        public float NotificationDuration { get; set; } = 5f;

        [Description("Whether to save SMS history between rounds")]
        public bool SaveHistoryBetweenRounds { get; set; } = true;

        [Description("The Discord webhook URL to send alerts to. Leave empty to disable.")]
        public string DiscordWebhookUrl { get; set; } = string.Empty;

        [Description("A list of blacklisted word categories to monitor. Available categories: Racial Slur, Homophobic Slur, Transphobic Slur, Ableist Slur, Antisemitic Slur")]
        public List<string> EnabledBlacklistedWordCategories { get; set; } = new List<string>
        {
            "Racial Slur",
            "Homophobic Slur",
            "Transphobic Slur",
            "Ableist Slur",
            "Antisemitic Slur"
        };

        [Description("A list of custom words to blacklist. These are checked in addition to the enabled categories.")]
        public List<string> CustomBlacklistedWords { get; set; } = new List<string> { "custom_word_1", "custom_word_2" };
    }
} 