# Whisper dictation for Codex and Claude Code

This AutoHotkey script uses `whisper-stream.exe` from the local `whisper.cpp` build at:

```text
C:\Users\leona\Downloads\whisper-bin-x64
```

## Requirements

- AutoHotkey v2.
- `C:\Users\leona\Downloads\whisper-bin-x64\whisper-stream.exe`
- `C:\Users\leona\Downloads\whisper-bin-x64\models\ggml-base.bin`

## Usage

1. Run `codex-claude-whisper.ahk`.
2. Put the cursor in Codex, Claude Code, or any text field.
3. Press `Ctrl+Space` to start dictation.
4. Wait for the beep / `Speak now` tooltip.
5. Speak for a few seconds.
6. Wait about 2 seconds, then press `Ctrl+Space` again to stop and paste the transcribed text.

The script records from the default Windows microphone and pastes into the window that was active when dictation started.

Temporary audio, transcript, and logs are written to:

```text
%TEMP%\codex-claude-whisper
```

## Configuration

Edit these values near the top of `codex-claude-whisper.ahk` if needed:

```autohotkey
whisperDir := "C:\Users\leona\Downloads\whisper-bin-x64"
language := "en"
captureDevice := -1
```

Use `language := "auto"` for automatic language detection, or `language := "pt"` for Portuguese.
Use `captureDevice := 0` for the Realtek microphone array or `captureDevice := 1` for the EDIFIER headset.

## Startup shortcut

To start it automatically with Windows, press `Win+R`, run:

```text
shell:startup
```

Then add a shortcut to `codex-claude-whisper.ahk` in that folder.
