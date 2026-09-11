# Playtime · 时长统计

A small, local Windows game playtime tracker. Add a game once, then launch it normally.

简洁的 Windows 本地游戏时长统计器。添加游戏后照常启动，后台自动记录。没有账号、遥测、联网请求或复杂图表。

## Features

- Drop an EXE or `.lnk`; resolve the actual shortcut target, arguments and working directory.
- Background detection of configured games only; tray pause, resume and quit.
- Total playtime, play count and session history; manual time entries and removable sessions.
- Simplified Chinese, Traditional Chinese, English and Japanese, with system-language selection.
- JSON data, mergeable backups and explicit legacy import.
- Optional start at Windows sign-in, disabled by default. No service or scheduled task.

## Run

Extract the Windows x64 ZIP into a writable folder and run `Playtime.exe`. Target: Windows 10/11 x64 with .NET Framework 4.8 or later. No .NET SDK or third-party runtime is needed for users whose Windows installation already provides that framework. The binary is unsigned.

The window closes to the system tray. Use **Quit** in the tray to stop tracking. Automatic sign-in startup is opt-in. Do not run directly from inside the ZIP or place the app under a read-only system directory.

普通用户每 3 秒检查；管理员权限下尝试进程事件，失败时回退。管理员权限不是运行要求。是否使用事件取决于 Windows 的权限与组件可用性。空闲事件模式每 15 秒校验，活动会话每 3 秒更新；游戏范围由进程名和目录共同限定。

## Data and accuracy

`data/settings.json` stores games and language. `data/sessions/*.json` stores completed segments; `data/active/*.json` stores heartbeats. Data is never bundled in releases. Startup registration is a current-user Run key, enabled only by the user's checkbox.

This measures **process runtime**, including background/idle time. It is not an active-input or attention tracker. Detection starts at observation, not retroactively. Polling can undercount by roughly 3 seconds at each boundary (about 6 seconds for a complete session). A six-second absence grace period prevents immediate churn; a briefly inaccessible process that returns with the same identity within that grace period is treated as continuous. Games shorter than the polling interval can be missed. Sleep notifications end the current segment; a scheduling gap over 20 seconds also ends it conservatively. Heartbeats are saved about every 6 seconds while active. Long pauses in OS scheduling, inaccessible processes and unusual launchers can affect results. Administrator mode may block drag-and-drop from a normal Explorer window; use Browse instead.

Pause/resume and restart segments share a process identity so they do not inflate play count. Manual entries increase duration but not count. Backups merge by game/session ID, keep existing definitions on ID conflicts, and do not alter language or startup preferences. Import paths are never executed automatically. Removing a game keeps history. Deleted sessions move to `data/trash`.

Update: quit the tray app, back up `data`, replace application files, preserve `data`. To uninstall, disable start at sign-in, quit and remove the directory.

## Build and verify

```powershell
.\build.ps1
.\tests\test.ps1
.\build.ps1 -Package
```

The build uses the Windows .NET Framework C# compiler and WPF assemblies. No NuGet dependencies. CI builds and tests on Windows and uploads a portable ZIP plus SHA-256 checksum. Source is C# 5 compatible. `--background` hides the initial window; `--data-dir PATH` isolates test data.

## Release status

Version 0.1.0 is an initial preview. Local verification is described in the delivery report; do not infer support for all games from one successful game test. Administrator event mode, sleep on different hardware and protected/anti-cheat processes need broader validation. No auto-updater is included.

## Languages and contribution

UI strings are centralized in `src/Language.cs`; help is in `docs/help.*.txt`. All four languages must cover the same UI keys. Translation review from native speakers is welcome. Keep contributions focused on easy setup, reliable tracking, accessibility and portability instead of complex analytics.

MIT license. See `LICENSE`.
