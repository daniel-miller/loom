import { Button, DangerZone, DangerZoneItem } from "@daniel-miller/loom";

export function CompanySettings() {
  return (
    <DangerZone className="max-w-2xl">
      <DangerZoneItem
        title="Archive company"
        description="Learners lose access; records are kept"
        action={
          <Button variant="outline" size="sm">
            Archive
          </Button>
        }
      />
      <DangerZoneItem
        title="Delete company"
        description="Removes every learner, course, and ticket. This cannot be undone."
        action={
          <Button variant="destructive" size="sm">
            Delete
          </Button>
        }
      />
    </DangerZone>
  );
}
