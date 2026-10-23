import { useState } from "react";
import {
  Button,
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@daniel-miller/loom";

export function Actions() {
  const [showArchived, setShowArchived] = useState(false);
  const [sort, setSort] = useState("name");
  return (
    <DropdownMenu defaultOpen modal={false}>
      <DropdownMenuTrigger asChild>
        <Button variant="outline">
          Options
          <i className="fa-sharp fa-regular fa-chevron-down text-xs" aria-hidden="true" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="start" className="w-56">
        <DropdownMenuLabel>Learner</DropdownMenuLabel>
        <DropdownMenuItem>
          <i className="fa-sharp fa-regular fa-pen" aria-hidden="true" />
          Edit profile
        </DropdownMenuItem>
        <DropdownMenuItem>
          <i className="fa-sharp fa-regular fa-envelope" aria-hidden="true" />
          Send invitation
        </DropdownMenuItem>
        <DropdownMenuItem disabled>
          <i className="fa-sharp fa-regular fa-key" aria-hidden="true" />
          Reset password
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuCheckboxItem checked={showArchived} onCheckedChange={setShowArchived}>
          Show archived
        </DropdownMenuCheckboxItem>
        <DropdownMenuSeparator />
        <DropdownMenuLabel>Sort by</DropdownMenuLabel>
        <DropdownMenuRadioGroup value={sort} onValueChange={setSort}>
          <DropdownMenuRadioItem value="name">Name</DropdownMenuRadioItem>
          <DropdownMenuRadioItem value="expiry">Expiry date</DropdownMenuRadioItem>
        </DropdownMenuRadioGroup>
        <DropdownMenuSeparator />
        <DropdownMenuItem className="text-danger">
          <i className="fa-sharp fa-regular fa-box-archive" aria-hidden="true" />
          Archive learner
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
