import { useState } from "react";
import Alert from "@mui/material/Alert";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";
import Typography from "@mui/material/Typography";
import { ClassificationTab } from "./ClassificationTab";
import { DataAssetsTab } from "./DataAssetsTab";
import { DataElementsTab } from "./DataElementsTab";
import { DataSourcesTab } from "./DataSourcesTab";
import { DiscoveryJobsTab } from "./DiscoveryJobsTab";

const TABS = ["sources", "jobs", "assets", "elements", "classification"] as const;
type TabKey = (typeof TABS)[number];

export function DataDiscoveryDashboardPage() {
  const [tab, setTab] = useState<TabKey>("sources");
  const [scopedDataSourceId, setScopedDataSourceId] = useState<string | null>(null);
  const [scopedDataAssetId, setScopedDataAssetId] = useState<string | null>(null);

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Data Discovery
      </Typography>
      <Alert severity="info" sx={{ py: 0.5 }}>
        Connectors read only schema metadata and a small masked sample from each source — customer data is never copied
        into this platform in bulk.
      </Alert>

      <Tabs value={tab} onChange={(_, value) => setTab(value)}>
        <Tab value="sources" label="Data Sources" />
        <Tab value="jobs" label="Discovery Jobs" />
        <Tab value="assets" label="Discovered Assets" />
        <Tab value="elements" label="Data Elements" />
        <Tab value="classification" label="Classification" />
      </Tabs>

      {tab === "sources" && (
        <DataSourcesTab
          onViewAssets={(dataSourceId) => {
            setScopedDataSourceId(dataSourceId);
            setScopedDataAssetId(null);
            setTab("assets");
          }}
        />
      )}

      {tab === "jobs" && (
        <Stack spacing={1}>
          {scopedDataSourceId && (
            <Chip label="Filtered by selected data source" onDelete={() => setScopedDataSourceId(null)} sx={{ alignSelf: "flex-start" }} />
          )}
          <DiscoveryJobsTab dataSourceId={scopedDataSourceId ?? undefined} />
        </Stack>
      )}

      {tab === "assets" && (
        <Stack spacing={1}>
          {scopedDataSourceId && (
            <Chip label="Filtered by selected data source" onDelete={() => setScopedDataSourceId(null)} sx={{ alignSelf: "flex-start" }} />
          )}
          <DataAssetsTab
            dataSourceId={scopedDataSourceId ?? undefined}
            onViewElements={(dataAssetId) => {
              setScopedDataAssetId(dataAssetId);
              setTab("elements");
            }}
          />
        </Stack>
      )}

      {tab === "elements" && (
        <Stack spacing={1}>
          {scopedDataAssetId && (
            <Chip label="Filtered by selected asset" onDelete={() => setScopedDataAssetId(null)} sx={{ alignSelf: "flex-start" }} />
          )}
          <DataElementsTab dataAssetId={scopedDataAssetId ?? undefined} />
        </Stack>
      )}

      {tab === "classification" && <ClassificationTab />}
    </Stack>
  );
}
