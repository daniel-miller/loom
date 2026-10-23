import {
  Button,
  Input,
  Label,
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@daniel-miller/loom";

export function Filters() {
  return (
    <Sheet defaultOpen>
      <SheetTrigger asChild>
        <Button variant="outline">Open filters</Button>
      </SheetTrigger>
      <SheetContent>
        <SheetHeader>
          <SheetTitle>Filters</SheetTitle>
          <SheetDescription>Narrow the ticket list</SheetDescription>
        </SheetHeader>
        <div className="space-y-1.5">
          <Label htmlFor="sheet-search">Learner name</Label>
          <Input id="sheet-search" placeholder="Jordan Lee" />
        </div>
      </SheetContent>
    </Sheet>
  );
}
