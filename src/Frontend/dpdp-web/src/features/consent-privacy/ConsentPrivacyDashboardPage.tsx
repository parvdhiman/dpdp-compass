import { useState } from "react";
import Stack from "@mui/material/Stack";
import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";
import Typography from "@mui/material/Typography";
import { CatalogsTab } from "./CatalogsTab";
import { ConsentRecordsTab } from "./ConsentRecordsTab";
import { DataPrincipalRequestsTab } from "./DataPrincipalRequestsTab";
import { PrivacyNoticesTab } from "./PrivacyNoticesTab";

const TABS = ["notices", "consents", "requests", "catalogs"] as const;
type TabKey = (typeof TABS)[number];

export function ConsentPrivacyDashboardPage() {
  const [tab, setTab] = useState<TabKey>("notices");

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Consent &amp; Privacy Operations
      </Typography>

      <Tabs value={tab} onChange={(_, value) => setTab(value)}>
        <Tab value="notices" label="Privacy Notices" />
        <Tab value="consents" label="Consent Records" />
        <Tab value="requests" label="Data Principal Requests" />
        <Tab value="catalogs" label="Catalogs" />
      </Tabs>

      {tab === "notices" && <PrivacyNoticesTab />}
      {tab === "consents" && <ConsentRecordsTab />}
      {tab === "requests" && <DataPrincipalRequestsTab />}
      {tab === "catalogs" && <CatalogsTab />}
    </Stack>
  );
}
