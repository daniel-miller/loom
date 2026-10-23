import { Button, Tooltip, TooltipContent, TooltipTrigger } from "@daniel-miller/loom";

export function IconButton() {
  return (
    <Tooltip defaultOpen>
      <TooltipTrigger asChild>
        <Button variant="outline" size="icon" aria-label="Download certificate">
          <i className="fa-sharp fa-regular fa-download" aria-hidden="true" />
        </Button>
      </TooltipTrigger>
      <TooltipContent side="right">Download certificate</TooltipContent>
    </Tooltip>
  );
}
