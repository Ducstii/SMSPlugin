# SMS Plugin for SCP: Secret Laboratory

A comprehensive SMS (Short Message Service) system for SCP: Secret Laboratory servers using the EXILED framework.

## Features

- **Phone Number Assignment**: Each player gets a unique phone number when they join
- **SMS Messaging**: Send private messages to other players using their phone numbers
- **Contact List**: View all online players and their phone numbers
- **Message History**: View recent messages sent and received
- **Persistent Storage**: Messages are saved between server restarts
- **Real-time Notifications**: Players receive instant notifications when they get new messages

## Commands

All commands are used through the Remote Admin console (press `~` key in-game):

- `sms help` - Show help message and available commands
- `sms contacts` - Show all online players and their phone numbers
- `sms send <number> <message>` - Send an SMS to a player
- `sms history` - Show recent message history

## Configuration

The plugin can be configured through the `config.yml` file:

```yaml
sms_system:
  # Whether the SMS plugin is enabled
  is_enabled: true
  
  # Whether to debug the plugin
  debug: false
  
  # Maximum message length allowed
  max_message_length: 200
  
  # Number of recent messages to show in history
  history_message_count: 5
  
  # Welcome message duration in seconds
  welcome_message_duration: 10.0
  
  # New message notification duration in seconds
  notification_duration: 5.0
  
  # Whether to save SMS history between rounds
  save_history_between_rounds: true
```

## Installation

1. Download the DLL from the latest release
2. Copy the compiled DLL to your EXILED plugins folder
3. Restart your SCP:SL server
4. The plugin will automatically create a configuration file

## Usage Example

1. Join the server - you'll receive a welcome message with your phone number
2. Use `sms contacts` to see other players' phone numbers
3. Use `sms send 555-123-4567 Hello there!` to send a message
4. The recipient will receive a notification with your message
5. Use `sms history` to view recent conversations

## File Structure

- `SMSPlugin.cs` - Main plugin class
- `SMSManager.cs` - Core SMS functionality and message handling
- `SMSCommands.cs` - Command system integration
- `Config.cs` - Plugin configuration
- `sms_history.json` - Persistent message storage (created automatically)

## Requirements

- SCP: Secret Laboratory Dedicated Server
- EXILED Framework
- .NET 9.0 or later

## License

This plugin is provided as-is for educational and entertainment purposes. 