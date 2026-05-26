"use client";

import { useEffect, useState } from "react";

interface TypeWriterProps {
  text: string;
  speed?: number;
}

export function TypeWriter({ text, speed = 100 }: TypeWriterProps) {
  const [displayed, setDisplayed] = useState("");

  useEffect(() => {
    if (displayed.length >= text.length) return;

    const timeout = setTimeout(() => {
      setDisplayed(text.slice(0, displayed.length + 1));
    }, speed);

    return () => clearTimeout(timeout);
  }, [displayed, text, speed]);

  return (
    <span>
      {displayed}
      <span className="ml-0.5 inline-block h-[1.1em] w-2.5 animate-[blink_1s_step-end_infinite] bg-current align-middle" />
    </span>
  );
}
