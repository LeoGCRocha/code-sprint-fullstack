type NoteBoxProps = {
  content: string;
};

export function NoteBox({ content }: NoteBoxProps) {
  return (
    <div className="mt-2 rounded-xl border-2 border-primary-500 bg-primary-100 p-2">
      <h3 className="font-black leading-tight text-primary-500">Note</h3>
      <span>{content}</span>
    </div>
  );
}
