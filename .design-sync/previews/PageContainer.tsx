import {
  Button,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  PageContainer,
  PageHeader,
} from "@daniel-miller/loom";

export function DefaultWidth() {
  return (
    <div className="bg-background">
      <PageContainer>
        <PageHeader
          title="Company settings"
          subtitle="Keyera Calgary"
          actions={<Button>Save changes</Button>}
        />
        <Card>
          <CardHeader>
            <CardTitle>Reminders</CardTitle>
          </CardHeader>
          <CardContent className="text-muted-foreground text-sm">
            Learners get an email 30 days before a ticket expires
          </CardContent>
        </Card>
      </PageContainer>
    </div>
  );
}

export function Narrow() {
  return (
    <div className="bg-background">
      <PageContainer width="narrow">
        <PageHeader title="Profile" subtitle="Narrow width for single-column forms" />
      </PageContainer>
    </div>
  );
}
