#Requires AutoHotkey v2.0
#SingleInstance Force

; Ctrl+Space starts whisper.cpp microphone capture.
; Wait for "Speak now", dictate, then press Ctrl+Space again to paste into the original window.

whisperDir := "C:\Users\leona\Downloads\whisper-bin-x64"
whisperExe := whisperDir "\whisper-stream.exe"
modelPath := whisperDir "\models\ggml-base.bin"

language := "en"
captureDevice := -1 ; -1 = Windows default. Use 0 for Realtek mic array, 1 for EDIFIER headset.
hotkeyTipMs := 1600
flushMs := 2500

stateDir := A_Temp "\codex-claude-whisper"
outFile := stateDir "\dictation.txt"
logFile := stateDir "\whisper.log"

isRecording := false
targetWindow := 0
whisperPid := 0

^Space::{
    ToggleDictation()
    KeyWait "Space"
}

ToggleDictation() {
    global isRecording

    if isRecording {
        StopDictation()
    } else {
        StartDictation()
    }
}

StartDictation() {
    global whisperExe, modelPath, language, captureDevice, stateDir, outFile, logFile
    global isRecording, targetWindow, whisperPid, hotkeyTipMs

    if !FileExist(whisperExe) {
        MsgBox "whisper-stream.exe nao foi encontrado em:`n" whisperExe
        return
    }

    if !FileExist(modelPath) {
        MsgBox "Modelo Whisper nao foi encontrado em:`n" modelPath
        return
    }

    StopWhisper()
    DirCreate stateDir
    TryDelete(outFile)
    TryDelete(logFile)

    targetWindow := WinExist("A")

    args := Format(
        '/c ""{1}" --model "{2}" --language {3} --capture {4} --step 1000 --length 5000 --keep 200 --max-tokens 128 --vad-thold 0.35 --file "{5}" --no-gpu > "{6}" 2>&1"',
        whisperExe,
        modelPath,
        language,
        captureDevice,
        outFile,
        logFile
    )

    Run A_ComSpec " " args, whisperDir, "Hide", &whisperPid
    isRecording := true

    ToolTip "Starting microphone..."
    if WaitForWhisperReady() {
        SoundBeep 900, 120
        ToolTip "Speak now. Ctrl+Space to finish"
        SetTimer ClearToolTip, -hotkeyTipMs
    } else {
        SoundBeep 350, 250
        ToolTip "Whisper is still loading. Wait for Start speaking in whisper.log."
        SetTimer ClearToolTip, -3000
    }
}

StopDictation() {
    global isRecording, targetWindow, outFile, flushMs, hotkeyTipMs

    if !isRecording {
        return
    }

    ToolTip "Finishing transcription..."
    Sleep flushMs

    text := ReadDictation(outFile)
    StopWhisper()
    isRecording := false

    if !text {
        ToolTip "No transcript yet. Speak after the beep and wait 2 sec before stopping."
        SetTimer ClearToolTip, -3000
        return
    }

    if targetWindow {
        WinActivate "ahk_id " targetWindow
        Sleep 120
    }

    A_Clipboard := text
    ClipWait 1
    Send "^v"

    ToolTip "Dictation pasted"
    SetTimer ClearToolTip, -hotkeyTipMs
}

WaitForWhisperReady() {
    global logFile

    deadline := A_TickCount + 15000
    while A_TickCount < deadline {
        if FileExist(logFile) {
            logText := FileRead(logFile, "UTF-8")
            if InStr(logText, "Start speaking") {
                return true
            }
        }
        Sleep 100
    }

    return false
}

ReadDictation(path) {
    if !FileExist(path) {
        return ""
    }

    raw := FileRead(path, "UTF-8")
    raw := RegExReplace(raw, "\R+", "`n")
    raw := RegExReplace(raw, "m)^\s*\[[^\]]+\]\s*", "")
    raw := RegExReplace(raw, "m)^\s+$", "")

    lines := StrSplit(raw, "`n")
    cleaned := []
    seen := Map()

    for line in lines {
        line := Trim(line)
        if !line {
            continue
        }

        key := StrLower(line)
        if seen.Has(key) {
            continue
        }

        seen[key] := true
        cleaned.Push(line)
    }

    return JoinLines(cleaned)
}

JoinLines(lines) {
    text := ""

    for line in lines {
        if text {
            text .= "`n"
        }
        text .= line
    }

    return Trim(text)
}

StopWhisper() {
    global whisperPid

    if whisperPid && ProcessExist(whisperPid) {
        RunWait A_ComSpec ' /c taskkill /T /F /PID ' whisperPid, , "Hide"
    }

    whisperPid := 0
}

TryDelete(path) {
    try {
        if FileExist(path) {
            FileDelete path
        }
    }
}

ClearToolTip() {
    ToolTip
}
