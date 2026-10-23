import { Checkbox, Input, Label } from "@daniel-miller/loom";

export function WithInput() {
  return (
    <div className="max-w-xs space-y-1.5">
      <Label htmlFor="email">Email</Label>
      <Input id="email" type="email" placeholder="jordan.lee@keyera.com" />
    </div>
  );
}

export function WithCheckbox() {
  return (
    <div className="flex items-center gap-2">
      <Checkbox id="expiry" defaultChecked />
      <Label htmlFor="expiry">Email me when a ticket expires</Label>
    </div>
  );
}
