import { Checkbox, Label } from "@daniel-miller/loom";

export function States() {
  return (
    <div className="flex flex-wrap items-center gap-6">
      <div className="flex items-center gap-2">
        <Checkbox id="c1" />
        <Label htmlFor="c1">Unchecked</Label>
      </div>
      <div className="flex items-center gap-2">
        <Checkbox id="c2" defaultChecked />
        <Label htmlFor="c2">Checked</Label>
      </div>
      <div className="flex items-center gap-2">
        <Checkbox id="c3" checked="indeterminate" />
        <Label htmlFor="c3">Indeterminate</Label>
      </div>
      <div className="flex items-center gap-2">
        <Checkbox id="c4" disabled defaultChecked />
        <Label htmlFor="c4">Disabled</Label>
      </div>
    </div>
  );
}

export function SettingsList() {
  return (
    <div className="max-w-sm space-y-3">
      <div className="flex items-start gap-2">
        <Checkbox id="s1" defaultChecked className="mt-0.5" />
        <div className="space-y-0.5">
          <Label htmlFor="s1">Send expiry reminders</Label>
          <p className="text-muted-foreground text-xs">30 days before a ticket expires</p>
        </div>
      </div>
      <div className="flex items-start gap-2">
        <Checkbox id="s2" className="mt-0.5" />
        <div className="space-y-0.5">
          <Label htmlFor="s2">Copy supervisors</Label>
          <p className="text-muted-foreground text-xs">Each learner's direct supervisor</p>
        </div>
      </div>
    </div>
  );
}
