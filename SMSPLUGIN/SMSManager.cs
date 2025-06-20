using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMSPLUGIN
{
    public class SMSManager : IDisposable
    {
        private static readonly Dictionary<string, string> _phoneNumbers = new();
        private static readonly Dictionary<string, List<SMSMessage>> _messageHistory = new();
        private const string SmsFilePath = "sms_history.json";
        private bool _isInitialized;
        private bool _disposed;
        private bool _systemEnabled = true; // System state that can be toggled

        private static readonly Dictionary<string, List<string>> _allBlacklistedWords = new Dictionary<string, List<string>>
        {
            { "Racial Slur", new List<string> { "nigger", "nigga", "n1gger", "n1gg3r", "chink", "gook", "spic" } },
            { "Homophobic Slur", new List<string> { "faggot", "fagot", "fggt", "dyke" } },
            { "Transphobic Slur", new List<string> { "tranny", "trannie", "shemale" } },
            { "Ableist Slur", new List<string> { "retard", "r3tard", "spastic" } },
            { "Antisemitic Slur", new List<string> { "kike" } }
        };

        public List<SMSMessage> SMSMessages { get; set; } = new List<SMSMessage>();
        
        public bool IsSystemEnabled => _systemEnabled;

        public void EnableSystem()
        {
            _systemEnabled = true;
            Log.Info("SMS system has been enabled.");
        }

        public void DisableSystem()
        {
            _systemEnabled = false;
            Log.Info("SMS system has been disabled.");
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            LoadSmsHistory();

            // Use the Verified event for assigning phone numbers
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        }

        public void Deinitialize()
        {
            if (!_isInitialized) return;
            _isInitialized = false;

            // Unsubscribe from the events
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            SaveSmsHistory();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Deinitialize();
            }

            _disposed = true;
        }

        private void OnPlayerVerified(VerifiedEventArgs ev)
        {
            // Assign a phone number to the player as soon as they are verified
            if (!_phoneNumbers.ContainsKey(ev.Player.UserId))
            {
                string phoneNumber = GeneratePhoneNumber();
                _phoneNumbers[ev.Player.UserId] = phoneNumber;
                _messageHistory[phoneNumber] = new List<SMSMessage>();
                if (SMSPlugin.Instance.Config.Debug)
                {
                    Log.Debug($"Assigned phone number {phoneNumber} to verified player {ev.Player.Nickname}.");
                }
            }
        }

        private void OnPlayerLeft(LeftEventArgs ev)
        {
            if (string.IsNullOrEmpty(ev.Player?.UserId))
                return;

            if (_phoneNumbers.TryGetValue(ev.Player.UserId, out string phoneNumber))
            {
                _phoneNumbers.Remove(ev.Player.UserId);
                _messageHistory.Remove(phoneNumber);
            }
        }

        public string GetHelpMessage(Player player)
        {
            string phoneNumber = GetPlayerPhoneNumber(player);
            
            // Debug logging
            if (SMSPlugin.Instance.Config.Debug)
            {
                Log.Debug($"GetHelpMessage called for player {player.Nickname} (UserId: {player.UserId})");
                Log.Debug($"Phone number retrieved: '{phoneNumber}'");
                Log.Debug($"Total phone numbers in dictionary: {_phoneNumbers.Count}");
            }
            
            string helpMessage = "===  SMS System ===\n" +
                               "Commands:\n" +
                               "sms help - Show this help message\n" +
                               "sms contacts - Show your contacts\n" +
                               "sms send <number> <message> - Send an SMS\n" +
                               "sms history - Show message history\n\n" +
                               $"Your phone number: {phoneNumber}";
            
            return helpMessage;
        }

        public string GetContactsMessage(Player player)
        {
            string phoneNumber = GetPlayerPhoneNumber(player);
            if (string.IsNullOrEmpty(phoneNumber)) return "Error: You don't have a phone number assigned!";

            var contacts = Player.List
                .Where(p => p.UserId != player.UserId && _phoneNumbers.TryGetValue(p.UserId, out _))
                .Select(p =>
                {
                    // Actively get the player again to ensure the nickname is up-to-date
                    var freshPlayer = Player.Get(p.UserId);
                    return new { Name = freshPlayer?.Nickname ?? p.Nickname, Number = _phoneNumbers[p.UserId] };
                })
                .ToList();

            string contactsMessage = "=== Your Contacts ===\n";
            foreach (var contact in contacts)
            {
                contactsMessage += $"{contact.Name}: {contact.Number}\n";
            }
            contactsMessage += $"\nYour number: {phoneNumber}";

            return contactsMessage;
        }

        public string SendMessage(Player sender, string receiverNumber, string message)
        {
            string senderNumber = GetPlayerPhoneNumber(sender);
            if (string.IsNullOrEmpty(senderNumber))
            {
                return "Error: You don't have a phone number assigned!";
            }

            var receiver = Player.List.FirstOrDefault(p => 
                _phoneNumbers.TryGetValue(p.UserId, out string number) && number == receiverNumber);

            if (receiver == null)
            {
                return "Error: Invalid phone number!";
            }

            // Check for blacklisted words
            CheckForBlacklistedWords(sender, message);

            var smsMessage = new SMSMessage
            {
                SenderNumber = senderNumber,
                ReceiverNumber = receiverNumber,
                Content = message,
                Timestamp = DateTime.Now
            };

            if (!_messageHistory.ContainsKey(senderNumber))
                _messageHistory[senderNumber] = new List<SMSMessage>();
            if (!_messageHistory.ContainsKey(receiverNumber))
                _messageHistory[receiverNumber] = new List<SMSMessage>();

            _messageHistory[senderNumber].Add(smsMessage);
            _messageHistory[receiverNumber].Add(smsMessage);
            SMSMessages.Add(smsMessage);

            // For the receiver: show a brief hint and send the message to their console
            receiver.ShowHint("You have a new message! Check your console.", 2f);
            receiver.SendConsoleMessage($"[<color=yellow>New SMS from {sender.Nickname} ({senderNumber})</color>]\n{message}", "yellow");

            // For the sender: return a formatted message that will be displayed in their console
            return $"[<color=cyan>SMS sent to {receiver.Nickname} ({receiverNumber})</color>]\n{message}";
        }

        private void CheckForBlacklistedWords(Player sender, string message)
        {
            if (string.IsNullOrEmpty(SMSPlugin.Instance.Config.DiscordWebhookUrl))
                return;

            string lowerCaseMessage = message.ToLower();

            // Check built-in categories
            var enabledCategories = SMSPlugin.Instance.Config.EnabledBlacklistedWordCategories;
            if (enabledCategories != null)
            {
                var enabledCategorySet = new HashSet<string>(enabledCategories, StringComparer.OrdinalIgnoreCase);

                foreach (var wordSet in _allBlacklistedWords)
                {
                    string category = wordSet.Key;
                    if (enabledCategorySet.Contains(category))
                    {
                        foreach (string word in wordSet.Value)
                        {
                            if (lowerCaseMessage.Contains(word.ToLower()))
                            {
                                SendBlacklistedWordAlert(sender, message, category, word);
                                return; // Found a match, no need to check further
                            }
                        }
                    }
                }
            }

            // Check custom words
            var customWords = SMSPlugin.Instance.Config.CustomBlacklistedWords;
            if (customWords != null)
            {
                foreach (string word in customWords)
                {
                    if (lowerCaseMessage.Contains(word.ToLower()))
                    {
                        SendBlacklistedWordAlert(sender, message, "Custom", word);
                        return; // Found a match
                    }
                }
            }
        }

        private async void SendBlacklistedWordAlert(Player sender, string message, string category, string matchedWord)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var webhookUrl = SMSPlugin.Instance.Config.DiscordWebhookUrl;

                    var embed = new
                    {
                        title = "Blacklisted Word Detected in SMS",
                        description = $"A player has used a blacklisted word in an SMS message.",
                        color = 15158332, // Red color
                        fields = new[]
                        {
                            new { name = "Sender", value = $"{sender.Nickname} ({sender.UserId})", inline = true },
                            new { name = "Category", value = category, inline = true },
                            new { name = "Matched Word", value = matchedWord, inline = false },
                            new { name = "Full Message", value = message, inline = false }
                        },
                        timestamp = DateTime.UtcNow.ToString("o")
                    };

                    var payload = new { embeds = new[] { embed } };
                    var jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    await httpClient.PostAsync(webhookUrl, content);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send Discord webhook alert: {ex}");
            }
        }

        public string GetHistoryMessage(Player player)
        {
            string phoneNumber = GetPlayerPhoneNumber(player);
            if (string.IsNullOrEmpty(phoneNumber) || !_messageHistory.ContainsKey(phoneNumber))
            {
                return "No message history found!";
            }

            var messages = _messageHistory[phoneNumber]
                .OrderByDescending(m => m.Timestamp)
                .Take(5)
                .ToList();

            string historyMessage = "=== Recent Messages ===\n";
            foreach (var msg in messages)
            {
                string senderName = Player.List
                    .FirstOrDefault(p => _phoneNumbers.TryGetValue(p.UserId, out string number) && number == msg.SenderNumber)
                    ?.Nickname ?? "Unknown";

                string receiverName = Player.List
                    .FirstOrDefault(p => _phoneNumbers.TryGetValue(p.UserId, out string number) && number == msg.ReceiverNumber)
                    ?.Nickname ?? "Unknown";

                string direction = msg.SenderNumber == phoneNumber ? "To" : "From";
                string otherName = msg.SenderNumber == phoneNumber ? receiverName : senderName;
                string otherNumber = msg.SenderNumber == phoneNumber ? msg.ReceiverNumber : msg.SenderNumber;

                historyMessage += $"\n{direction}: {otherName} ({otherNumber})\n" +
                                $"{msg.Content}\n" +
                                $"{msg.Timestamp:HH:mm:ss}\n";
            }

            return historyMessage;
        }

        private string GetPlayerPhoneNumber(Player player)
        {
            bool hasPhoneNumber = _phoneNumbers.TryGetValue(player.UserId, out string number);
            
            // Debug logging
            if (SMSPlugin.Instance.Config.Debug)
            {
                Log.Debug($"GetPlayerPhoneNumber called for player {player.Nickname} (UserId: {player.UserId})");
                Log.Debug($"Has phone number: {hasPhoneNumber}");
                Log.Debug($"Phone number: '{number}'");
                Log.Debug($"Available UserIds: {string.Join(", ", _phoneNumbers.Keys)}");
            }
            
            return hasPhoneNumber ? number : string.Empty;
        }

        private string GeneratePhoneNumber()
        {
            Random random = new Random();
            return $"555-{random.Next(100, 999)}-{random.Next(1000, 9999)}";
        }

        public void SaveSmsHistory()
        {
            try
            {
                string json = JsonConvert.SerializeObject(SMSMessages, Formatting.Indented);
                File.WriteAllText(SmsFilePath, json);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save SMS history: {ex}");
            }
        }

        private void LoadSmsHistory()
        {
            try
            {
                if (File.Exists(SmsFilePath))
                {
                    string json = File.ReadAllText(SmsFilePath);
                    SMSMessages = JsonConvert.DeserializeObject<List<SMSMessage>>(json) ?? new List<SMSMessage>();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load SMS history: {ex}");
            }
        }
    }

    public class SMSMessage
    {
        public string SenderNumber { get; set; }
        public string ReceiverNumber { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 