using System.ComponentModel;
using System.Configuration;

namespace SteamController
{
    [Category("1. Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed partial class SettingsDebug : CommonHelpers.BaseSettings
    {
        public static readonly SettingsDebug Default = new SettingsDebug();

        public SettingsDebug() : base("SettingsDebug")
        {
        }

        [Description("Use Lizard Buttons instead of emulated. This option is only for testing purposes.")]
        public bool LizardButtons
        {
            get { return Get<bool>("LizardButtons", false); }
            set { Set("LizardButtons", value); }
        }

        [Description("Use Lizard Mouse instead of emulated. This option is only for testing purposes.")]
        public bool LizardMouse
        {
            get { return Get<bool>("LizardMouse", false); }
            set { Set("LizardMouse", value); }
        }

        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [Description("Pressure below which a Faux Lizard Mode trackpad press is released. Default: 6900.")]
        public ushort FauxPadReleaseThreshold
        {
            get { return Get<ushort>("FauxPadReleaseThreshold", 6900); }
            set
            {
                int release = Math.Min(65435, (int)value);

                // Press must be at least 100 above release
                if (FauxPadPressThreshold < release + 100)
                    Set("FauxPadPressThreshold", (ushort)(release + 100));

                Set("FauxPadReleaseThreshold", (ushort)release);
            }
        }

        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [Description("Pressure required to register a Faux Lizard Mode trackpad press. Default: 9500.")]
        public ushort FauxPadPressThreshold
        {
            get { return Get<ushort>("FauxPadPressThreshold", 9500); }
            set
            {
                int press = Math.Max(100, (int)value);

                // Release must be at least 100 below press
                if (FauxPadReleaseThreshold > press - 100)
                    Set("FauxPadReleaseThreshold", (ushort)(press - 100));

                Set("FauxPadPressThreshold", (ushort)press);
            }
        }

        [Description("Emulate Lizard controls in software. LizardButtons and LizardMouse must be disabled for this to take effect.")]
        public bool FauxLizardMode
        {
            get { return Get<bool>("FauxLizardMode", true); }
            set { Set("FauxLizardMode", value); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
