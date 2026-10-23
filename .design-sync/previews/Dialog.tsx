import {
  Button,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  Input,
  Label,
} from "@daniel-miller/loom";

export function RenameCourse() {
  return (
    <Dialog defaultOpen>
      <DialogTrigger asChild>
        <Button>Rename course</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Rename course</DialogTitle>
          <DialogDescription>Learners see the new name on their next sign-in</DialogDescription>
        </DialogHeader>
        <div className="space-y-1.5">
          <Label htmlFor="course-name">Course name</Label>
          <Input id="course-name" defaultValue="H2S Alive" />
        </div>
        <DialogFooter>
          <Button variant="outline">Cancel</Button>
          <Button>Rename</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
