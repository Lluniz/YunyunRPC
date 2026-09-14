# YunyunRPC

Enhanced Discord Rich Presence for Yunyun Syndrome!? Rhythm Psychosis.

The aim of this project is to improve the Rich Presence of Yunyun Syndrome by adding detailed in-game status information to your Discord profile. Built as a mod using C# and Harmony, bringing real-time game activity directly to Discord.

Shows current game state, active loop, phase details, and status during gameplay sessions.

## Installation

1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5).
2. Download the [latest release](https://github.com/Lluniz/YunyunRPC/releases) and extract the contents into the `BepInEx/plugins` folder in your Yunyun Syndrome!? installation directory.

## Usage

Just play the game. The Discord Activity will show up automatically.

> **Note:** If you launch Discord *after* starting the game, you may need to restart the game for Rich Presence to connect properly.

## Features

- **Real-Time Gameplay Tracking:** Displays live score, current combo, max combo, and song information.
- **Detailed Performance Ratings:** Accurately displays your live **Accuracy (%)**, **Score**, **Difficulty** (Normal, Pulsing, Bursting, Degenerate), and final **Rank** (S, A, B, C, D).
- **YunyunLoader Compatible:** Native support for YunyunLoader and standard BepInEx mods.
- **Dynamic State Synchronization:** Automatically handles play, pause, song end, and result screen transitions.
- **Lightweight:** Built specifically for Yunyun Syndrome!? with minimal performance footprint.

## Work in progress

  - **Location & UI Detection:**
  - Title Screen status
  - In-game Desktop detection
  - Open application tracking (within the game's OS)
  - Q's House status

## Thanks

- This mod is inspired by [MDRPC](https://github.com/Braasileiro/MDRPC).
- **[EBro912](https://github.com/EBro912)** - Creator of [YunYunLoader](https://github.com/EBro912/YunYunLoader), for guidance on loader integration and extracting song titles, artists, and difficulty levels.

## Showcase

<p align="center">
  <img width="600" alt="preview1" src="https://github.com/user-attachments/assets/12a962aa-7f30-4cc8-81f9-f3f5b0714778" />
  <br><br>
  <img width="450" alt="preview2" src="https://github.com/user-attachments/assets/2bba8051-314d-4b03-b88c-0d536c26e3c0" />
</p>
<p align="center">
  (Modded Song by the Discord User <b>sageshrooms</b> available on <a href="https://github.com/EBro912/YunYunLoader">YunYunLoader</a> Discord server).
</p>
