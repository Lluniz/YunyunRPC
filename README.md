# YunyunRPC

Enhanced Discord Rich Presence for Yunyun Syndrome!? Rhythm Psychosis.

The aim of this project is to improve the Rich Presence of Yunyun Syndrome by adding detailed in-game status information to your Discord profile. Built as a mod using C# and Harmony, bringing real-time game activity directly to Discord.

Shows current game state, active loop, phase details, and status during gameplay sessions.

## Installation

1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5).
2. Download the [latest release](https://github.com/your-username/YunyunRPC/releases) and extract the contents into the `BepInEx/plugins` folder in your Yunyun Syndrome!? installation directory.

## Usage

Just play the game. The Discord Activity will show up automatically.

**Please make sure your Discord desktop app is open before starting the game.** If you open Discord after launching the game, you may need to restart the game for the Discord status to work properly.

## Features

- **Real-Time State:** Automatically tracks and displays game states and loop events.
- **Dynamic Updates:** Uses state patches to maintain live synchronization with Discord.
- **Lightweight:** Built specifically for Yunyun Syndrome!? with minimal performance footprint.

## Work in progress

  - **Location Detection:**
  - Title Screen status
  - In-game Desktop detection
  - Open application tracking (within the game's OS)
  - Q's House status
  - **Mod Loader Support:** Implement native support for [YunyunLoader](https://github.com/EBro912/YunYunLoader) in future updates (if applicable).

## Thanks

This mod is inspired by [MDRPC](https://github.com/Braasileiro/MDRPC).

## Showcase

<img width="735" height="585" alt="preview1" src="https://github.com/user-attachments/assets/fca87e58-ce05-4874-8ef1-0bbd030487ae" />
<img width="735" height="585" alt="preview" src="https://github.com/user-attachments/assets/714e74f4-57b6-4692-83d9-7f0a112b158c" />
