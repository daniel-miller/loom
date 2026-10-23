import { Badge } from "@daniel-miller/loom";

export function Variants() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Badge>Default</Badge>
      <Badge variant="secondary">Draft</Badge>
      <Badge variant="outline">Archived</Badge>
    </div>
  );
}

export function Status() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Badge variant="success">Valid</Badge>
      <Badge variant="warning">Expiring</Badge>
      <Badge variant="destructive">Expired</Badge>
    </div>
  );
}

export function WithIcon() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Badge variant="success">
        <i className="fa-sharp fa-solid fa-circle-check" aria-hidden="true" />
        Compliant
      </Badge>
      <Badge variant="destructive">
        <i className="fa-sharp fa-solid fa-circle-xmark" aria-hidden="true" />
        Overdue
      </Badge>
    </div>
  );
}
