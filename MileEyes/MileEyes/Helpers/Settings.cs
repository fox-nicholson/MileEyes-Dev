// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace MileEyes.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = string.Empty;

        private const string InvoicedDefaultKey = "invoiced_key";
        private static readonly bool InvoicedDefaultDefault = false;

        private const string DefaulPassngersKey = "passengers_key";
        private static readonly int DefaultPassengersDefault = 0;
        #endregion


        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault<string>(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue<string>(SettingsKey, value);
            }
        }

        public static bool InvoicedDefault
        {
            get { return AppSettings.GetValueOrDefault<bool>(InvoicedDefaultKey, InvoicedDefaultDefault); }
            set { AppSettings.AddOrUpdateValue<bool>(InvoicedDefaultKey, value); }
        }

        public static int DefaultPassengers
        {
            get { return AppSettings.GetValueOrDefault<int>(DefaulPassngersKey, DefaultPassengersDefault); }
            set { AppSettings.AddOrUpdateValue<int>(DefaulPassngersKey, value); }
        }

    }
}