## #{GIT_TAG_NAME}

- SteamController: FauxLizardMode always enabled by default

## 0.7.4-G2

- Updater: Point updater to this repo

## 0.7.4-G1

- SteamController: Added Lizard Mouse software emulation as newer SteamOS versions break native Lizard Mouse on Windows. **Please enable FauxLizardMode in settings**.

## 0.7.4

- SteamController: Persist and expose in release Lizard{Buttons,Mouse}
- PerformanceOverlay: Battery-only setting in Performance Overlay (#193)

## 0.7.3

- SteamDeck LCD: Support BIOS F7A0131

## 0.7.2

- PowerControl: Add Charge Limit (70%, 80%, 90%, 100%)

## 0.7.1

- SteamDeck OLED: Support BIOS 107 with temperature readings
- SteamDeck OLED: Remove BIOS 105 support as it is buggy

## 0.7.0

- FanControl: Support for SteamDeck OLED
- PerformanceOverlay: Support the `AMD Custom GPU 0932` found in SteamDeck OLED
- PowerControl: Support `AMD Custom GPU 0932` with a SMU at `0x80600000-0x8067ffff` ver.: `0x063F0E00`
