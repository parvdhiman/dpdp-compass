import { useState } from "react";
import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";
import * as api from "./api";
import { GenericCatalogManager, type CatalogApi } from "../data-inventory/GenericCatalogManager";
import { DATA_PRINCIPAL_REQUEST_TYPES, DATA_SUBJECT_CATEGORIES } from "./types";

const CATALOGS = ["principals", "purposes", "sla"] as const;
type CatalogKey = (typeof CATALOGS)[number];

export function CatalogsTab() {
  const [catalog, setCatalog] = useState<CatalogKey>("principals");

  return (
    <>
      <Tabs value={catalog} onChange={(_, value) => setCatalog(value)} sx={{ mb: 2 }}>
        <Tab value="principals" label="Data Principals" />
        <Tab value="purposes" label="Consent Purposes" />
        <Tab value="sla" label="SLA Policies" />
      </Tabs>

      {catalog === "principals" && (
        <GenericCatalogManager
          storageKey="data-principals"
          title="Data Principal"
          permission="dataprincipals.manage"
          api={api.dataPrincipalsApi as unknown as CatalogApi}
          fields={[
            { key: "externalReferenceId", label: "External Reference ID", type: "text", required: true },
            { key: "referenceCategory", label: "Reference Category", type: "select", options: DATA_SUBJECT_CATEGORIES },
            { key: "notes", label: "Notes", type: "text" },
          ]}
          columns={[
            { key: "externalReferenceId", label: "External Reference ID" },
            { key: "referenceCategory", label: "Category" },
            { key: "notes", label: "Notes" },
          ]}
        />
      )}

      {catalog === "purposes" && (
        <GenericCatalogManager
          storageKey="consent-purposes"
          title="Consent Purpose"
          permission="consentpurposes.manage"
          api={api.consentPurposesApi as unknown as CatalogApi}
          fields={[
            { key: "name", label: "Name", type: "text", required: true },
            { key: "description", label: "Description", type: "text" },
          ]}
          columns={[
            { key: "name", label: "Name" },
            { key: "description", label: "Description" },
            { key: "dataCategoryName", label: "Data Category" },
          ]}
        />
      )}

      {catalog === "sla" && (
        <GenericCatalogManager
          storageKey="sla-policies"
          title="SLA Policy"
          permission="datarequests.manage"
          api={api.slaPoliciesApi as unknown as CatalogApi}
          fields={[
            { key: "name", label: "Name", type: "text", required: true },
            { key: "description", label: "Description", type: "text" },
            { key: "requestType", label: "Request Type (blank = catch-all)", type: "select", options: DATA_PRINCIPAL_REQUEST_TYPES },
            { key: "responseDueDays", label: "Response Due (days)", type: "number", required: true },
          ]}
          columns={[
            { key: "name", label: "Name" },
            { key: "requestType", label: "Request Type" },
            { key: "responseDueDays", label: "Due (days)" },
          ]}
        />
      )}
    </>
  );
}
