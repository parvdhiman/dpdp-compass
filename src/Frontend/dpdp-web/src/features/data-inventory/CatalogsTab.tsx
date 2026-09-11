import { useState } from "react";
import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";
import * as api from "./api";
import { GenericCatalogManager, type CatalogApi } from "./GenericCatalogManager";
import {
  DATA_COLLECTION_SOURCE_TYPES, CLASSIFICATION_CATEGORIES, IT_SYSTEM_TYPES,
  RECIPIENT_TYPES, RETENTION_PERIOD_UNITS,
} from "./types";

const CATALOGS = ["categories", "systems", "sources", "processors", "recipients", "retention"] as const;
type CatalogKey = (typeof CATALOGS)[number];

export function CatalogsTab() {
  const [catalog, setCatalog] = useState<CatalogKey>("categories");

  return (
    <>
      <Tabs value={catalog} onChange={(_, value) => setCatalog(value)} sx={{ mb: 2 }}>
        <Tab value="categories" label="Data Categories" />
        <Tab value="systems" label="Systems" />
        <Tab value="sources" label="Data Sources" />
        <Tab value="processors" label="Processors" />
        <Tab value="recipients" label="Recipients" />
        <Tab value="retention" label="Retention Policies" />
      </Tabs>

      {catalog === "categories" && (
        <GenericCatalogManager
          storageKey="data-categories"
          title="Data Category"
          permission="datainventory.manage"
          api={api.dataCategoriesApi as unknown as CatalogApi}
          fields={[
            { key: "name", label: "Name", type: "text", required: true },
            { key: "description", label: "Description", type: "text" },
            { key: "classificationCategory", label: "Classification", type: "select", options: CLASSIFICATION_CATEGORIES },
          ]}
          columns={[
            { key: "name", label: "Name" },
            { key: "description", label: "Description" },
            { key: "classificationCategory", label: "Classification" },
          ]}
        />
      )}

      {catalog === "systems" && (
        <GenericCatalogManager
          storageKey="it-systems"
          title="System"
          permission="datainventory.manage"
          api={api.itSystemsApi as unknown as CatalogApi}
          fields={[
            { key: "name", label: "Name", type: "text", required: true },
            { key: "description", label: "Description", type: "text" },
            { key: "systemType", label: "System Type", type: "select", options: IT_SYSTEM_TYPES, required: true },
          ]}
          columns={[
            { key: "name", label: "Name" },
            { key: "systemType", label: "Type" },
            { key: "ownerName", label: "Owner" },
          ]}
        />
      )}

      {catalog === "sources" && (
        <GenericCatalogManager
          storageKey="data-collection-sources"
          title="Data Collection Source"
          permission="datainventory.manage"
          api={api.dataCollectionSourcesApi as unknown as CatalogApi}
          fields={[
            { key: "name", label: "Name", type: "text", required: true },
            { key: "description", label: "Description", type: "text" },
            { key: "sourceType", label: "Source Type", type: "select", options: DATA_COLLECTION_SOURCE_TYPES, required: true },
          ]}
          columns={[
            { key: "name", label: "Name" },
            { key: "sourceType", label: "Type" },
            { key: "description", label: "Description" },
          ]}
        />
      )}

      {catalog === "processors" && (
        <GenericCatalogManager
          storageKey="processors"
          title="Processor"
          permission="datainventory.manage"
          api={api.processorsApi as unknown as CatalogApi}
          fields={[
            { key: "name", label: "Name", type: "text", required: true },
            { key: "description", label: "Description", type: "text" },
            { key: "contactEmail", label: "Contact Email", type: "text" },
            { key: "country", label: "Country", type: "text" },
          ]}
          columns={[
            { key: "name", label: "Name" },
            { key: "country", label: "Country" },
            { key: "contactEmail", label: "Contact" },
          ]}
        />
      )}

      {catalog === "recipients" && (
        <GenericCatalogManager
          storageKey="recipients"
          title="Recipient"
          permission="datainventory.manage"
          api={api.recipientsApi as unknown as CatalogApi}
          fields={[
            { key: "name", label: "Name", type: "text", required: true },
            { key: "description", label: "Description", type: "text" },
            { key: "recipientType", label: "Recipient Type", type: "select", options: RECIPIENT_TYPES, required: true },
          ]}
          columns={[
            { key: "name", label: "Name" },
            { key: "recipientType", label: "Type" },
            { key: "description", label: "Description" },
          ]}
        />
      )}

      {catalog === "retention" && (
        <GenericCatalogManager
          storageKey="retention-policies"
          title="Retention Policy"
          permission="datainventory.manage"
          api={api.retentionPoliciesApi as unknown as CatalogApi}
          fields={[
            { key: "name", label: "Name", type: "text", required: true },
            { key: "description", label: "Description", type: "text" },
            { key: "retentionPeriodValue", label: "Period", type: "number", required: true },
            { key: "retentionPeriodUnit", label: "Unit", type: "select", options: RETENTION_PERIOD_UNITS, required: true },
            { key: "triggerEvent", label: "Trigger Event", type: "text" },
          ]}
          columns={[
            { key: "name", label: "Name" },
            { key: "retentionPeriodValue", label: "Period" },
            { key: "retentionPeriodUnit", label: "Unit" },
            { key: "triggerEvent", label: "Trigger" },
          ]}
        />
      )}
    </>
  );
}
