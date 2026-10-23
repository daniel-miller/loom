import { CopyableId } from "@daniel-miller/loom";

export function Ids() {
  return (
    <div className="flex flex-wrap gap-6">
      <CopyableId label="Learner" value="100482" />
      <CopyableId label="Company" value="8f3c2a1e-4b7d-4e9a-b1c6-2d5f7a9e0c13" />
    </div>
  );
}
