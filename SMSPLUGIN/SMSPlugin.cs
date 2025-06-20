using Exiled.API.Features;
using System;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SMSPLUGIN
{
    public class SMSPlugin : Plugin<Config>
    {
        public static SMSPlugin Instance { get; private set; }
        public SMSManager SMSManager { get; private set; }

        public override string Name => "SMS System";
        public override string Author => "Ducstii";
        public override Version Version => new Version(1, 0, 0);

        // Set this to your current release date (UTC)
        private readonly DateTime CurrentReleaseDate = new DateTime(2025, 6, 20);

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

            CheckForUpdates();

            Log.Info("SMS Plugin has been enabled!");
        }

        private void CheckForUpdates()
        {
            try
            {
                string apiUrl = "https://api.github.com/repos/Ducstii/SMSPlugin/releases";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
                request.UserAgent = "SMSPlugin";

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    var releases = JArray.Parse(json);

                    foreach (var release in releases)
                    {
                        bool isPrerelease = release["prerelease"]?.ToObject<bool>() ?? false;
                        if (!isPrerelease)
                        {
                            string tag = release["tag_name"]?.ToString();
                            string publishedAt = release["published_at"]?.ToString();
                            DateTime publishedDate = DateTime.Parse(publishedAt).ToUniversalTime();

                            if (publishedDate > CurrentReleaseDate)
                            {
                                Log.Warn($"A new version of SMSPlugin is available: {tag} (Published {publishedDate:yyyy-MM-dd}). You are running a version from {CurrentReleaseDate:yyyy-MM-dd}.");
                                Log.Warn("Download it at: https://github.com/Ducstii/SMSPlugin/releases/latest");
                            }
                            else
                            {
                                Log.Info("SMSPlugin is up to date.");
                            }
                            break; // Only check the latest non-prerelease
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"Could not check for updates: {ex.Message}");
            }
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