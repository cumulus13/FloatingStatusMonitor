  # Floating Status Monitor

A lightweight Windows desktop application that displays real-time CPU usage, memory usage, and CPU temperature in a floating transparent window.  
It is designed for quick monitoring without taking much screen space and includes customizable appearance settings.


[![Screenshot_1](https://raw.githubusercontent.com/cumulus13/FloatingStatusMonitor/master/fsm_1.png)](https://raw.githubusercontent.com/cumulus13/FloatingStatusMonitor/master/fsm_1.png)

[![Screenshot_2](https://raw.githubusercontent.com/cumulus13/FloatingStatusMonitor/master/fsm_2.png)](https://raw.githubusercontent.com/cumulus13/FloatingStatusMonitor/master/fsm_2.png)

---

## Features
- **Real-time monitoring** of:
  - CPU usage (%)
  - Memory usage (two methods: Committed Bytes and Global Memory Status)
  - CPU temperature (via LibreHardwareMonitor)
- **Floating transparent window** that stays on top.
- **Customizable fonts, colors, and opacity** via `config.json`.
- **Tray icon** with quick options:
  - Save current window geometry
  - Exit application
- **Automatic configuration reload** when `config.json` changes.
- **Drag to move** and hotkeys:
  - `Q` → Save position and quit
  - `S` → Save position

---

## Requirements
- **Windows OS**
- **.NET Framework / .NET Core**
- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)
- Newtonsoft.Json (`Json.NET`)

---

## Configuration
Configuration is stored in `config.json`. Example:

```json
{
  "FontName": "Consolas",
  "FontSize": 11.0,
  "TextColor": "#FFFFFF",
  "BackColor": "#000000",
  "Opacity": 0.7,
  "WindowWidth": 120,
  "WindowHeight": 85,
  "WindowX": -1388,
  "WindowY": 740,
  "CpuHighBgColor": "#0000FF",
  "CpuCriticalBgColor": "#FF0000",
  "CpuHighTextColor": "#FFFFFF",
  "CpuCriticalTextColor": "#FFFFFF",
  "RamTempColor0": "#00FF00",
  "RamTempColor30": "#00FFFF",
  "RamTempColor50": "#FFA500",
  "RamTempColor70": "#FFFF00",
  "RamTempColor80": "#FFFF00",
  "RamTempColor90": "#FF00FB",
  "RamTempColor98": "#FFFFFF",
  "RamTempColor99": "#FFFFFF",
  "RamTempBgColor98": "#0000FF",
  "RamTempBgColor99": "#FF0000"
}
```

- **FontName**: Font family used in the floating window.
- **Opacity**: Transparency of the window (`0.0` to `1.0`).
- **WindowX, WindowY**: Initial window position.
- **WindowWidth, WindowHeight**: Initial window size.
- **Color settings**: Custom threshold colors for CPU, RAM, and temperature.

---

## Build & Run
1. Clone or download the source.
2. Open the project in Visual Studio or use CLI:
   ```bash
   dotnet build
   dotnet run
   ```
3. Place `config.json` in the same directory as the executable.

---

## License
MIT License. See [LICENSE](./LICENSE) for details.

## author
[Hadi Cahyadi](mailto:cumulus13@gmail.com)
    

[![Buy Me a Coffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/cumulus13)

[![Donate via Ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/cumulus13)

[Support me on Patreon](https://www.patreon.com/cumulus13)