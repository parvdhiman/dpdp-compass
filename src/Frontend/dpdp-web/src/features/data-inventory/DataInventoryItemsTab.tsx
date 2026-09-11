import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import Pagination from "@mui/material/Pagination";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TextField from "@mui/material/TextField";
import { useAuth } from "../auth/AuthProvider";
import * as api from "./api";
import { dataCategoriesApi, itSystemsApi, processorsApi, retentionPoliciesApi, dataCollectionSourcesApi } from "./api";
import { CLASSIFICATION_CATEGORIES, RISK_LEVELS, type DataInventoryItem, type UpsertDataInventoryItemPayload } from "./types";

const PAGE_SIZE = 20;

const RISK_COLORS: Record<string, "success" | "warning" | "error" | "default"> = {
  LOW: "success", MEDIUM: "warning", HIGH: "error", CRITICAL: "error",
};

function ItemFormDialog({ item, onClose, onSave, saving }: { item: DataInventoryItem | null; onClose: () => void; onSave: (payload: UpsertDataInventoryItemPayload) => void; saving: boolean }) {
  const { data: categories } = useQuery({ queryKey: ["data-categories", ""], queryFn: () => dataCategoriesApi.list() });
  const { data: systems } = useQuery({ queryKey: ["it-systems", ""], queryFn: () => itSystemsApi.list() });
  const { data: sources } = useQuery({ queryKey: ["data-collection-sources", ""], queryFn: () => dataCollectionSourcesApi.list() });
  const { data: processors } = useQuery({ queryKey: ["processors", ""], queryFn: () => processorsApi.list() });
  const { data: retentionPolicies } = useQuery({ queryKey: ["retention-policies", ""], queryFn: () => retentionPoliciesApi.list() });

  const [dataCategoryId, setDataCategoryId] = useState(item?.dataCategoryId ?? "");
  const [dataElementName, setDataElementName] = useState(item?.dataElementName ?? "");
  const [classification, setClassification] = useState(item?.classification ?? "");
  const [dataCollectionSourceId, setDataCollectionSourceId] = useState(item?.dataCollectionSourceId ?? "");
  const [itSystemId, setItSystemId] = useState(item?.itSystemId ?? "");
  const [purpose, setPurpose] = useState(item?.purpose ?? "");
  const [retentionPolicyId, setRetentionPolicyId] = useState(item?.retentionPolicyId ?? "");
  const [sharingDescription, setSharingDescription] = useState(item?.sharingDescription ?? "");
  const [processorId, setProcessorId] = useState(item?.processorId ?? "");
  const [riskLevel, setRiskLevel] = useState(item?.riskLevel ?? "");

  const canSave = dataCategoryId.length > 0 && dataElementName.trim().length > 0;

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{item ? "Edit Data Inventory Item" : "Add Data Inventory Item"}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField label="Data Element" fullWidth required value={dataElementName} onChange={(e) => setDataElementName(e.target.value)} />
          <TextField select label="Data Category" fullWidth required value={dataCategoryId} onChange={(e) => setDataCategoryId(e.target.value)}>
            {categories?.map((c) => (
              <MenuItem key={String(c.id)} value={String(c.id)}>
                {String(c.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Classification" fullWidth value={classification} onChange={(e) => setClassification(e.target.value)}>
            <MenuItem value="">Not personal data</MenuItem>
            {CLASSIFICATION_CATEGORIES.map((c) => (
              <MenuItem key={c} value={c}>
                {c.replace(/_/g, " ")}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Source" fullWidth value={dataCollectionSourceId} onChange={(e) => setDataCollectionSourceId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {sources?.map((s) => (
              <MenuItem key={String(s.id)} value={String(s.id)}>
                {String(s.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="System" fullWidth value={itSystemId} onChange={(e) => setItSystemId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {systems?.map((s) => (
              <MenuItem key={String(s.id)} value={String(s.id)}>
                {String(s.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField label="Purpose" fullWidth multiline rows={2} value={purpose} onChange={(e) => setPurpose(e.target.value)} />
          <TextField select label="Retention Policy" fullWidth value={retentionPolicyId} onChange={(e) => setRetentionPolicyId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {retentionPolicies?.map((p) => (
              <MenuItem key={String(p.id)} value={String(p.id)}>
                {String(p.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField label="Sharing" fullWidth multiline rows={2} value={sharingDescription} onChange={(e) => setSharingDescription(e.target.value)} />
          <TextField select label="Processor" fullWidth value={processorId} onChange={(e) => setProcessorId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {processors?.map((p) => (
              <MenuItem key={String(p.id)} value={String(p.id)}>
                {String(p.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Risk Level" fullWidth value={riskLevel} onChange={(e) => setRiskLevel(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {RISK_LEVELS.map((r) => (
              <MenuItem key={r} value={r}>
                {r}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!canSave || saving}
          onClick={() =>
            onSave({
              dataCategoryId,
              dataElementName,
              classification: classification || null,
              dataCollectionSourceId: dataCollectionSourceId || null,
              itSystemId: itSystemId || null,
              purpose: purpose || null,
              retentionPolicyId: retentionPolicyId || null,
              sharingDescription: sharingDescription || null,
              processorId: processorId || null,
              riskLevel: riskLevel || null,
            })
          }
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function DataInventoryItemsTab() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [riskLevel, setRiskLevel] = useState("");
  const [dialogState, setDialogState] = useState<{ mode: "create" } | { mode: "edit"; item: DataInventoryItem } | null>(null);

  const canManage = hasPermission("datainventory.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["data-inventory-items", page, search, riskLevel],
    queryFn: () => api.getDataInventoryItems({ page, pageSize: PAGE_SIZE, search: search || undefined, riskLevel: riskLevel || undefined }),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["data-inventory-items"] });

  const createMutation = useMutation({ mutationFn: api.createDataInventoryItem, onSuccess: invalidate });
  const updateMutation = useMutation({ mutationFn: (vars: { id: string; payload: UpsertDataInventoryItemPayload }) => api.updateDataInventoryItem(vars.id, vars.payload), onSuccess: invalidate });
  const deleteMutation = useMutation({ mutationFn: api.deleteDataInventoryItem, onSuccess: invalidate });

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: "space-between", flexWrap: "wrap", gap: 2 }}>
        <Stack direction="row" spacing={2}>
          <TextField
            label="Search"
            size="small"
            sx={{ minWidth: 220 }}
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
          />
          <TextField
            select
            label="Risk Level"
            size="small"
            sx={{ minWidth: 160 }}
            value={riskLevel}
            onChange={(e) => {
              setRiskLevel(e.target.value);
              setPage(1);
            }}
          >
            <MenuItem value="">All risk levels</MenuItem>
            {RISK_LEVELS.map((r) => (
              <MenuItem key={r} value={r}>
                {r}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" onClick={() => api.exportDataInventory({ search: search || undefined, riskLevel: riskLevel || undefined })}>
            Export CSV
          </Button>
          {canManage && (
            <Button variant="contained" onClick={() => setDialogState({ mode: "create" })}>
              Add Item
            </Button>
          )}
        </Stack>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load data inventory items.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Item #</TableCell>
                  <TableCell>Data Element</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>System</TableCell>
                  <TableCell>Processor</TableCell>
                  <TableCell>Risk</TableCell>
                  {canManage && <TableCell>Actions</TableCell>}
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((item) => (
                  <TableRow key={item.id} hover>
                    <TableCell>{item.itemNumber}</TableCell>
                    <TableCell>{item.dataElementName}</TableCell>
                    <TableCell>{item.dataCategoryName}</TableCell>
                    <TableCell>{item.itSystemName ?? "—"}</TableCell>
                    <TableCell>{item.processorName ?? "—"}</TableCell>
                    <TableCell>{item.riskLevel && <Chip size="small" label={item.riskLevel} color={RISK_COLORS[item.riskLevel]} />}</TableCell>
                    {canManage && (
                      <TableCell>
                        <Stack direction="row" spacing={1}>
                          <Button size="small" onClick={() => setDialogState({ mode: "edit", item })}>
                            Edit
                          </Button>
                          <Button size="small" color="error" onClick={() => deleteMutation.mutate(item.id)}>
                            Delete
                          </Button>
                        </Stack>
                      </TableCell>
                    )}
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      No data inventory items found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
          <Stack direction="row" sx={{ justifyContent: "center" }}>
            <Pagination count={data.totalPages} page={data.page} onChange={(_, value) => setPage(value)} color="primary" />
          </Stack>
        </Stack>
      )}

      {dialogState && (
        <ItemFormDialog
          item={dialogState.mode === "edit" ? dialogState.item : null}
          saving={createMutation.isPending || updateMutation.isPending}
          onClose={() => setDialogState(null)}
          onSave={(payload) => {
            if (dialogState.mode === "create") {
              createMutation.mutate(payload);
            } else {
              updateMutation.mutate({ id: dialogState.item.id, payload });
            }
            setDialogState(null);
          }}
        />
      )}
    </Stack>
  );
}
