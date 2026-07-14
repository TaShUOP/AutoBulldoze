# Cities: Skylines 2 - Auto Bulldoze Mod

A high-performance C# code mod for Cities: Skylines II that automatically removes abandoned buildings from your city, saving you the hassle of manually bulldozing them.

## Features
- Periodically scans the game for abandoned buildings.
- Safely removes the buildings and their associated child entities (sub-areas, networks, lanes).
- Built using the Unity Entity Component System (ECS) and Burst compiler for zero impact on game performance.

## Installation (For Players)
This mod is officially available on Paradox Mods! 
To install it:
1. Launch **Cities: Skylines II**.
2. Open the **Paradox Mods** menu from the main screen.
3. Search for **"Auto Bulldoze"** (or look under your subscribed playsets).
4. Click **Add to Playset** and make sure it is enabled.

## How to Build (For Developers)

### Prerequisites
- [Cities: Skylines II](https://store.steampowered.com/app/949230/Cities_Skylines_II/)
- The official Cities: Skylines II **Modding Toolchain** installed via the in-game options menu.
- **.NET 6.0 SDK or Runtime**

### Compiling
1. Clone this repository.
2. Open the folder in a terminal.
3. Run the following command:
   ```bash
   dotnet build
   ```
4. The toolchain will automatically compile the DLLs (for Windows, Mac, and Linux) and copy them into your local Cities: Skylines II `Mods` folder.

## Publishing
To publish to Paradox Mods:
1. Ensure your Paradox account credentials are saved in `../../Common/pdx_account.txt` relative to this project (Line 1: email, Line 2: password).
2. Open `AutoBulldoze.csproj` in Visual Studio 2022 or JetBrains Rider.
3. Right-click the project and select **Publish**.
