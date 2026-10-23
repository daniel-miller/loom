import { Button, Popover, PopoverContent, PopoverTrigger } from "@daniel-miller/loom";

export function ExpiryExplainer() {
  return (
    <Popover defaultOpen>
      <PopoverTrigger asChild>
        <Button variant="outline">
          <i className="fa-sharp fa-regular fa-circle-info" aria-hidden="true" />
          Why is this expiring?
        </Button>
      </PopoverTrigger>
      <PopoverContent align="start" className="w-72 text-sm">
        H2S Alive is valid for three years from the issue date. This ticket was issued{" "}
        <span className="font-mono">2023-11-20</span>, so it lapses in six weeks.
      </PopoverContent>
    </Popover>
  );
}
