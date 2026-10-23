import { Button, PageHeader } from "@daniel-miller/loom";

export function WithActions() {
  return (
    <PageHeader
      title="Learners"
      subtitle="214 learners across 6 sites"
      actions={
        <>
          <Button variant="outline">
            <i className="fa-sharp fa-regular fa-download" aria-hidden="true" />
            Export
          </Button>
          <Button>
            <i className="fa-sharp fa-regular fa-plus" aria-hidden="true" />
            Add learner
          </Button>
        </>
      }
    />
  );
}

export function Restricted() {
  return (
    <PageHeader
      title="Billing"
      subtitle="Invoices and payment methods"
      restricted={["Company administrators only", "Enforced by the API, not just hidden in the menu"]}
    />
  );
}

export function TitleOnly() {
  return <PageHeader title="Courses" />;
}
