import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@daniel-miller/loom";

export function TicketCards() {
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>H2S Alive</CardTitle>
          <CardDescription>Energy Safety Canada, renewed every three years</CardDescription>
        </CardHeader>
        <CardContent className="text-sm">
          <p>
            Issued <span className="font-mono">2025-11-05</span>, expires{" "}
            <span className="font-mono">2028-11-05</span>.
          </p>
        </CardContent>
        <CardFooter className="justify-between">
          <Badge variant="success">Valid</Badge>
          <Button variant="outline" size="sm">
            View ticket
          </Button>
        </CardFooter>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Standard First Aid</CardTitle>
          <CardDescription>Red Cross, renewed every three years</CardDescription>
        </CardHeader>
        <CardContent className="text-sm">
          <p>
            Expired <span className="font-mono">2026-09-14</span>. Book a refresher to restore the
            ticket.
          </p>
        </CardContent>
        <CardFooter className="justify-between">
          <Badge variant="destructive">Expired</Badge>
          <Button size="sm">Book refresher</Button>
        </CardFooter>
      </Card>
    </div>
  );
}
