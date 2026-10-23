import { Label, Textarea } from "@daniel-miller/loom";

export function WithLabel() {
  return (
    <div className="max-w-md space-y-1.5">
      <Label htmlFor="notes">Notes</Label>
      <Textarea id="notes" placeholder="Anything the assessor should know" rows={3} />
    </div>
  );
}

export function Filled() {
  return (
    <div className="max-w-md space-y-1.5">
      <Label htmlFor="reason">Reason for extension</Label>
      <Textarea
        id="reason"
        rows={3}
        defaultValue="Jordan was on rotation at Rimbey and missed the October session. The next one is 2026-11-12."
      />
    </div>
  );
}

export function Disabled() {
  return (
    <div className="max-w-md space-y-1.5">
      <Label htmlFor="locked-notes">Assessor comments</Label>
      <Textarea id="locked-notes" rows={2} disabled defaultValue="Locked after sign-off" />
    </div>
  );
}
