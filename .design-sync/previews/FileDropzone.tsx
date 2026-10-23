import { FileDropzone } from "@daniel-miller/loom";

export function Default() {
  return (
    <div className="max-w-sm">
      <FileDropzone
        label="Drop a SCORM package or click to choose"
        hint="A .zip file up to 500 MB"
        accept=".zip"
        onFile={() => {}}
      />
    </div>
  );
}

export function Disabled() {
  return (
    <div className="max-w-sm">
      <FileDropzone
        label="Uploads paused"
        hint="The storage quota is full"
        disabled
        onFile={() => {}}
      />
    </div>
  );
}
