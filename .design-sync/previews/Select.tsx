import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectSeparator,
  SelectTrigger,
  SelectValue,
} from "@daniel-miller/loom";

export function Open() {
  return (
    <Select defaultValue="calgary" defaultOpen>
      <SelectTrigger className="w-56" aria-label="Site">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>Alberta</SelectLabel>
          <SelectItem value="calgary">Calgary office</SelectItem>
          <SelectItem value="rimbey">Rimbey gas plant</SelectItem>
          <SelectItem value="strachan">Strachan gas plant</SelectItem>
        </SelectGroup>
        <SelectSeparator />
        <SelectGroup>
          <SelectLabel>British Columbia</SelectLabel>
          <SelectItem value="pipestone">Pipestone</SelectItem>
          <SelectItem value="closed" disabled>
            Fort St. John (closed)
          </SelectItem>
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}

export function Closed() {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <Select>
        <SelectTrigger className="w-56" aria-label="Status">
          <SelectValue placeholder="Any status" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="valid">Valid</SelectItem>
          <SelectItem value="expiring">Expiring</SelectItem>
          <SelectItem value="expired">Expired</SelectItem>
        </SelectContent>
      </Select>
      <Select disabled defaultValue="calgary">
        <SelectTrigger className="w-56" aria-label="Site, locked">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="calgary">Calgary office</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
}
