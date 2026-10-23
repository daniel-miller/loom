import { ConfirmDialog } from "@daniel-miller/loom";

const deleteCopy = {
  title: "Delete course?",
  description:
    "H2S Alive and its 214 enrolments are removed. Issued tickets stay on each learner's record.",
  confirmLabel: "Delete course",
  busyLabel: "Deleting...",
};

export function Destructive() {
  return <ConfirmDialog open onOpenChange={() => {}} onConfirm={() => {}} {...deleteCopy} />;
}

export function DefaultTone() {
  return (
    <ConfirmDialog
      open
      onOpenChange={() => {}}
      tone="default"
      title="Publish revision 4?"
      description="Revision 3 is retired. Learners already enrolled finish on it."
      confirmLabel="Publish revision"
      onConfirm={() => {}}
    />
  );
}

export function Busy() {
  return <ConfirmDialog open busy onOpenChange={() => {}} onConfirm={() => {}} {...deleteCopy} />;
}
