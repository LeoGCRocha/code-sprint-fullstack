"use client";

import { useEffect, useRef } from "react";
import { Editor } from "@monaco-editor/react";
import type { editor } from "monaco-editor";

type CodeEditorProps = {
  language: string;
  value: string;
  onChange: (value: string) => void;
  height?: string;
};

export function CodeSolution({ language, value, onChange, height = "400px" }: CodeEditorProps) {
  const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null);
  const monacoRef = useRef<typeof import("monaco-editor") | null>(null);

  useEffect(() => {
    if (!editorRef.current || !monacoRef.current) return;
    const model = editorRef.current.getModel();
    if (model) monacoRef.current.editor.setModelLanguage(model, language);
  }, [language]);

  return (
    <Editor
      height={height}
      theme="vs-dark"
      language={language}
      value={value}
      keepCurrentModel
      onChange={(v) => onChange(v ?? "")}
      onMount={(editor, monaco) => {
        editorRef.current = editor;
        monacoRef.current = monaco;
      }}
      options={{
        fontSize: 14,
        minimap: { enabled: false },
        scrollBeyondLastLine: false,
        quickSuggestions: false,
        suggest: { preview: false, showWords: false },
        hover: { enabled: false },
        parameterHints: { enabled: false },
        wordBasedSuggestions: "off",
        occurrencesHighlight: "off",
        selectionHighlight: false,
        codeLens: false,
        links: false,
        colorDecorators: false,
      }}
    />
  );
}
