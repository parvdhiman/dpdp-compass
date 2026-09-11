import { useState } from "react";
import Stack from "@mui/material/Stack";
import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";
import Typography from "@mui/material/Typography";
import { CatalogsTab } from "./CatalogsTab";
import { DataFlowsTab } from "./DataFlowsTab";
import { DataInventoryItemsTab } from "./DataInventoryItemsTab";
import { ProcessingActivitiesTab } from "./ProcessingActivitiesTab";

const TABS = ["items", "activities", "flows", "catalogs"] as const;
type TabKey = (typeof TABS)[number];

export function DataInventoryDashboardPage() {
  const [tab, setTab] = useState<TabKey>("items");

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Data Inventory &amp; Processing Activities
      </Typography>

      <Tabs value={tab} onChange={(_, value) => setTab(value)}>
        <Tab value="items" label="Data Inventory" />
        <Tab value="activities" label="Processing Activities" />
        <Tab value="flows" label="Data Flows" />
        <Tab value="catalogs" label="Catalogs" />
      </Tabs>

      {tab === "items" && <DataInventoryItemsTab />}
      {tab === "activities" && <ProcessingActivitiesTab />}
      {tab === "flows" && <DataFlowsTab />}
      {tab === "catalogs" && <CatalogsTab />}
    </Stack>
  );
}
