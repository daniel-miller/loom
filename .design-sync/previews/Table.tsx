import {
  Badge,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  formatDate,
} from "@daniel-miller/loom";

const tickets = [
  { id: 48211, learner: "Jordan Lee", course: "H2S Alive", expires: "2028-11-05T00:00:00", status: "valid" },
  { id: 48190, learner: "Priya Raman", course: "Standard First Aid", expires: "2026-11-20T00:00:00", status: "expiring" },
  { id: 47702, learner: "Marc Tremblay", course: "Fall Protection", expires: "2026-09-14T00:00:00", status: "expired" },
] as const;

const statusBadge = {
  valid: <Badge variant="success">Valid</Badge>,
  expiring: <Badge variant="warning">Expiring</Badge>,
  expired: <Badge variant="destructive">Expired</Badge>,
};

export function Tickets() {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Ticket</TableHead>
          <TableHead>Learner</TableHead>
          <TableHead>Course</TableHead>
          <TableHead>Expires</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {tickets.map((t) => (
          <TableRow key={t.id}>
            <TableCell className="font-mono">{t.id}</TableCell>
            <TableCell>{t.learner}</TableCell>
            <TableCell>{t.course}</TableCell>
            <TableCell className="font-mono">{formatDate(t.expires)}</TableCell>
            <TableCell>{statusBadge[t.status]}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
