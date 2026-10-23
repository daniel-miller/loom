import { Button } from "@daniel-miller/loom";

export function Variants() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Button>Save changes</Button>
      <Button variant="outline">Cancel</Button>
      <Button variant="ghost">Skip</Button>
      <Button variant="destructive">Delete course</Button>
    </div>
  );
}

export function Sizes() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Button size="sm">Small</Button>
      <Button>Default</Button>
      <Button size="lg">Large</Button>
      <Button size="icon" variant="outline" aria-label="Settings">
        <i className="fa-sharp fa-regular fa-gear" aria-hidden="true" />
      </Button>
    </div>
  );
}

export function WithIcon() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Button>
        <i className="fa-sharp fa-regular fa-plus" aria-hidden="true" />
        Add learner
      </Button>
      <Button variant="outline">
        <i className="fa-sharp fa-regular fa-download" aria-hidden="true" />
        Export
      </Button>
    </div>
  );
}

export function Disabled() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Button disabled>Save changes</Button>
      <Button variant="outline" disabled>
        Cancel
      </Button>
    </div>
  );
}
