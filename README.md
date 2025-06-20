# SMS Plugin for SCP: Secret Laboratory

A comprehensive SMS (Short Message Service) system for SCP: Secret Laboratory servers using the EXILED framework.

## Features

-   **Phone Number Assignment**: Each player gets a unique phone number after they are verified on the server.
-   **SMS Messaging**: Send private messages to other players using their phone numbers.
-   **Contact List**: View all online players and their phone numbers.
-   **Message History**: View recent messages sent and received.
-   **Admin Controls**: Enable or disable the entire SMS system on the fly.
-   **Discord Webhook Alerts**: Get notified in a Discord channel when players use blacklisted words.
-   **Configurable Blacklist**: Use built-in categories of sensitive words, and add your own custom words to the blacklist.

## Player Commands

All commands are used through the in-game console (press `~` key):

-   `sms` or `sms help` - Show the help message and your phone number.
-   `sms contacts` - Show all online players and their phone numbers.
-   `sms send <number> <message>` - Send an SMS to a player.
-   `sms history` - Show the last 5 messages in your history.

## Admin Commands

These commands are used through the Remote Admin console:

-   `sms_enable` - Enables the SMS system for all players.
-   `sms_disable` - Disables the SMS system for all players.
-   `sms_status` - Shows the current status of the plugin and its configuration.

## Configuration

The plugin can be configured through the `config.yml` file generated in your EXILED config folder.

```yaml
# Whether the plugin is enabled on startup.
is_enabled: true

# Enable debug logging to the server console.
debug: false

# Maximum message length allowed.
max_message_length: 200

# Number of recent messages to show in history.
history_message_count: 5

# How long the "new message" hint appears (in seconds).
notification_duration: 2.0

# Whether to save SMS history between rounds.
save_history_between_rounds: true

# The Discord webhook URL to send alerts to. Leave empty to disable.
discord_webhook_url: ""

# A list of blacklisted word categories to monitor.
# Available categories: Racial Slur, Homophobic Slur, Transphobic Slur, Ableist Slur, Antisemitic Slur
enabled_blacklisted_word_categories:
- Racial Slur
- Homophobic Slur
- Transphobic Slur
- Ableist Slur
- Antisemitic Slur

# A list of your own custom words to blacklist.
custom_blacklisted_words:
- custom_word_1
- custom_word_2
```

## Installation

1.  Build the project or download the latest release DLL.
2.  Copy the compiled DLL to your EXILED plugins folder.
3.  Restart your server. The plugin will automatically generate its configuration file.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 