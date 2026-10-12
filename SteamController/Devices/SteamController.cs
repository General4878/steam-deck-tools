using System.Diagnostics;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController : IDisposable
    {
        public const ushort VendorID = 0x28DE;
        public const ushort ProductID = 0x1205;
        private const int ReadTimeout = 50;
        public const int MaxFailures = 10;

        private hidapi.HidDevice neptuneDevice;
        private Stopwatch stopwatch = new Stopwatch();
        private TimeSpan? lastUpdate;
        private int failures;
        public long ElapsedMilliseconds { get => stopwatch.ElapsedMilliseconds; }
        public double DeltaTime { get; private set; }

        // Faux Lizard Mode parameters
        private bool fauxLeftPadPressed = false;
        private bool fauxRightPadPressed = false;

        internal SteamController()
        {
            InitializeButtons();
            InitializeActions();

            OpenDevice();

            stopwatch.Start();
        }

        ~SteamController()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            using (neptuneDevice) { }
        }

        public bool Updated { get; private set; }

        private void OpenDevice()
        {
            using (neptuneDevice) { }
            neptuneDevice = new hidapi.HidDevice(VendorID, ProductID, 64);
            neptuneDevice.OpenDevice();
        }

        internal void Fail(bool immediately = false)
        {
            foreach (var action in AllActions)
                action.Reset();

            // Try to re-open every MaxFailures
            failures++;
            if (failures % MaxFailures == 0 || immediately)
            {
                OpenDevice();
                failures = 0;
            }
        }

        private void BeforeUpdate(byte[] buffer)
        {
            ApplyFauxTrackpadPresses(buffer);

            foreach (var action in AllActions)
                action.BeforeUpdate(buffer);
        }

        internal void BeforeUpdate()
        {
            var ts = stopwatch.Elapsed;
            DeltaTime = lastUpdate is not null ? (ts - lastUpdate.Value).TotalSeconds : 0.0;
            DeltaTime = Math.Min(DeltaTime, 0.1); // max update is 100ms
            lastUpdate = ts;

            LizardButtons = true;
            LizardMouse = true;

            try
            {
                byte[] data = neptuneDevice.Read(ReadTimeout);
                if (data == null)
                {
                    Fail();
                    Updated = false;
                    return;
                }

                BeforeUpdate(data);
                Updated = true;
            }
            catch (hidapi.HidDeviceInvalidException)
            {
                // Steam might disconnect device
                Fail();
                Updated = false;
            }
            catch (Exception e)
            {
                TraceException("STEAM", "BeforeUpdate", e);
                Fail();
                Updated = false;
            }
        }

        internal void Update()
        {
            foreach (var action in AllActions)
                action.Update();

            try
            {
                UpdateLizardButtons();
                UpdateLizardMouse();
            }
            catch (hidapi.HidDeviceInvalidException)
            {
                // Steam might disconnect device
                Fail();
            }
            catch (Exception e)
            {
                // Steam have disconnected device, which triggered exception
                if (e.Message == "Could not send report to hid device. Error: -1")
                    DebugException("STEAM", "Update", e);
                else
                    TraceException("STEAM", "Update", e);
                Fail();
            }
        }

        // Sets track pads button press bit if specified pressures are detected
        private void ApplyFauxTrackpadPresses(byte[] buffer)
        {
            // Obtain presure thresholds
            ushort pressThreshold =
                SettingsDebug.Default.FauxPadPressThreshold;

            ushort releaseThreshold =
                SettingsDebug.Default.FauxPadReleaseThreshold;

            // Only enable if Faux Lizard Mode is active
            bool enabled =
                SettingsDebug.Default.FauxLizardMode &&
                !SettingsDebug.Default.LizardMouse &&
                !SettingsDebug.Default.LizardButtons;

            if (!enabled)
            {
                fauxLeftPadPressed = false;
                fauxRightPadPressed = false;
                return;
            }

            // Ensure the HID report contains both pressure readings
            if (buffer.Length < 0x3C)
                return;

            // Read pressure values from the Steam Deck HID report.
            ushort leftPressure = BitConverter.ToUInt16(buffer, 0x38);
            ushort rightPressure = BitConverter.ToUInt16(buffer, 0x3A);

            // Check whether each trackpad is actually being touched.
            bool leftTouched = (buffer[0x0A] & 0x08) != 0;
            bool rightTouched = (buffer[0x0A] & 0x10) != 0;

            // Left trackpad: press and release
            if (!leftTouched)
            {
                fauxLeftPadPressed = false;
            }
            else if (!fauxLeftPadPressed)
            {
                fauxLeftPadPressed = leftPressure >= pressThreshold;
            }
            else
            {
                fauxLeftPadPressed = leftPressure > releaseThreshold;
            }

            // Right trackpad: press and release
            if (!rightTouched)
            {
                fauxRightPadPressed = false;
            }
            else if (!fauxRightPadPressed)
            {
                fauxRightPadPressed = rightPressure >= pressThreshold;
            }
            else
            {
                fauxRightPadPressed = rightPressure > releaseThreshold;
            }

            // Replace the two trackpad press bits
            buffer[0x0A] = (byte)(
                (buffer[0x0A] & ~0x06) |
                (fauxLeftPadPressed ? 0x02 : 0) |
                (fauxRightPadPressed ? 0x04 : 0)
            );
        }
    }
}
