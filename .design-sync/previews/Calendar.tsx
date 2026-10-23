import { useState } from "react";
import { Calendar } from "@daniel-miller/loom";

// The flex parent lets the bordered frame shrink to the month grid.
export function Single() {
  const [date, setDate] = useState<Date | undefined>(new Date(2026, 9, 14));
  return (
    <div className="flex">
      <div className="border-border rounded-lg border">
        <Calendar mode="single" selected={date} onSelect={setDate} defaultMonth={date} />
      </div>
    </div>
  );
}

export function Dropdowns() {
  return (
    <div className="flex">
      <div className="border-border rounded-lg border">
        <Calendar
          mode="single"
          captionLayout="dropdown"
          startMonth={new Date(2020, 0)}
          endMonth={new Date(2030, 11)}
          defaultMonth={new Date(2026, 9)}
        />
      </div>
    </div>
  );
}
