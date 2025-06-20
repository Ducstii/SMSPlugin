using Exiled.API.Features;
using System;

namespace SMSPLUGIN
{
    public class SMSPlugin : Plugin<Config>
    {
        public static SMSPlugin Instance { get; private set; }
        public SMSManager SMSManager { get; private set; }

        public override string Name => "SMS System";
        public override string Author => "Ducstii";
        public override Version Version => new Version(1, 0, 0);

        // Explicit constructor to help with loading
        public SMSPlugin()
        {
        }

        public override void OnEnabled()
        {
            Log.Info("SMSPlugin is starting to load...");
            Instance = this;
            SMSManager = new SMSManager();
            SMSManager.Initialize();
            
            Log.Info("SMS Plugin has been enabled!");
        }

        public override void OnDisabled()
        {
            SMSManager?.Dispose();
            SMSManager = null;
            Instance = null;
            
            Log.Info("SMS Plugin has been disabled!");
        }
    }
} 