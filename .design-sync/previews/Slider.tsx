import { useState } from "react";
import { Label, Slider } from "@daniel-miller/loom";

export function PassMark() {
  const [score, setScore] = useState([80]);
  return (
    <div className="max-w-sm space-y-2">
      <div className="flex items-center justify-between">
        <Label>Pass mark</Label>
        <span className="text-sm tabular-nums">{score[0]} percent</span>
      </div>
      <Slider value={score} onValueChange={setScore} min={50} max={100} step={5} />
    </div>
  );
}

export function Disabled() {
  return <Slider className="max-w-sm" defaultValue={[60]} disabled />;
}
