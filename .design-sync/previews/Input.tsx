import { Input, Label } from "@daniel-miller/loom";

export function Fields() {
  return (
    <div className="grid max-w-xl gap-4 sm:grid-cols-2">
      <div className="space-y-1.5">
        <Label htmlFor="name">Full name</Label>
        <Input id="name" placeholder="Jordan Lee" />
      </div>
      <div className="space-y-1.5">
        <Label htmlFor="handle">Handle</Label>
        <Input id="handle" defaultValue="keyera-calgary" className="font-mono" />
      </div>
      <div className="space-y-1.5">
        <Label htmlFor="issued">Issued</Label>
        <Input id="issued" type="date" defaultValue="2025-11-05" />
      </div>
      <div className="space-y-1.5">
        <Label htmlFor="locked">Employee number</Label>
        <Input id="locked" defaultValue="100482" disabled className="font-mono" />
      </div>
    </div>
  );
}
