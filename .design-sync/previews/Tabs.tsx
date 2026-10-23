import { Tabs, TabsContent, TabsList, TabsTrigger } from "@daniel-miller/loom";

export function Pill() {
  return (
    <Tabs defaultValue="tickets">
      <TabsList>
        <TabsTrigger value="tickets">Tickets</TabsTrigger>
        <TabsTrigger value="courses">Courses</TabsTrigger>
        <TabsTrigger value="history">History</TabsTrigger>
      </TabsList>
    </Tabs>
  );
}

export function Underline() {
  return (
    <Tabs defaultValue="overview" className="w-full max-w-md">
      <TabsList variant="underline">
        <TabsTrigger variant="underline" value="overview">
          Overview
        </TabsTrigger>
        <TabsTrigger variant="underline" value="members">
          Members
        </TabsTrigger>
        <TabsTrigger variant="underline" value="settings">
          Settings
        </TabsTrigger>
      </TabsList>
      <TabsContent value="overview" className="text-muted-foreground pt-3 text-sm">
        214 learners across 6 sites
      </TabsContent>
      <TabsContent value="members" className="text-muted-foreground pt-3 text-sm">
        12 administrators
      </TabsContent>
      <TabsContent value="settings" className="text-muted-foreground pt-3 text-sm">
        Company settings
      </TabsContent>
    </Tabs>
  );
}
