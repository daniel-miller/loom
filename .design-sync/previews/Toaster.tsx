import { useEffect } from "react";
import { Button, Toaster, toast } from "@daniel-miller/loom";

// Mount the Toaster once near the app root; call toast() from anywhere.
export function WithAction() {
  useEffect(() => {
    toast("Learner archived", {
      description: "Jordan Lee no longer appears in the learner list",
      action: { label: "Undo", onClick: () => {} },
      duration: Infinity,
    });
  }, []);
  return (
    <div className="min-h-80">
      <Button variant="outline" onClick={() => toast("Course renamed")}>
        Show a toast
      </Button>
      <Toaster theme="light" />
    </div>
  );
}
