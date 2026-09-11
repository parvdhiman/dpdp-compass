import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import FormControlLabel from "@mui/material/FormControlLabel";
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
import { itSystemsApi, dataCollectionSourcesApi, processorsApi, recipientsApi, dataCategoriesApi } from "./api";
import type { UpsertDataFlowPayload } from "./types";

const PAGE_SIZE = 20;

function FlowFormDialog({ onClose, onSave, saving }: { onClose: () => void; onSave: (payload: UpsertDataFlowPayload) => void; saving: boolean }) {
  const { data: systems } = useQuery({ queryKey: ["it-systems", ""], queryFn: () => itSystemsApi.list() });
  const { data: sources } = useQuery({ queryKey: ["data-collection-sources", ""], queryFn: () => dataCollectionSourcesApi.list() });
  const { data: processors } = useQuery({ queryKey: ["processors", ""], queryFn: () => processorsApi.list() });
  const { data: recipients } = useQuery({ queryKey: ["recipients", ""], queryFn: () => recipientsApi.list() });
  const { data: categories } = useQuery({ queryKey: ["data-categories", ""], queryFn: () => dataCategoriesApi.list() });

  const [name, setName] = useState("");
  const [dataCategoryId, setDataCategoryId] = useState("");
  const [fromItSystemId, setFromItSystemId] = useState("");
  const [fromDataCollectionSourceId, setFromDataCollectionSourceId] = useState("");
  const [fromDescription, setFromDescription] = useState("");
  const [toItSystemId, setToItSystemId] = useState("");
  const [toProcessorId, setToProcessorId] = useState("");
  const [toRecipientId, setToRecipientId] = useState("");
  const [toDescription, setToDescription] = useState("");
  const [transferMechanism, setTransferMechanism] = useState("");
  const [isCrossBorder, setIsCrossBorder] = useState(false);
  const [crossBorderCountry, setCrossBorderCountry] = useState("");

  const canSave = name.trim() && fromDescription.trim() && toDescription.trim() && (!isCrossBorder || crossBorderCountry.trim());

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>New Data Flow</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField label="Name" fullWidth required value={name} onChange={(e) => setName(e.target.value)} />
          <TextField select label="Data Category" fullWidth value={dataCategoryId} onChange={(e) => setDataCategoryId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {categories?.map((c) => (
              <MenuItem key={String(c.id)} value={String(c.id)}>
                {String(c.name)}
              </MenuItem>
            ))}
          </TextField>

          <TextField select label="From System (optional)" fullWidth value={fromItSystemId} onChange={(e) => setFromItSystemId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {systems?.map((s) => (
              <MenuItem key={String(s.id)} value={String(s.id)}>
                {String(s.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="From Data Source (optional)" fullWidth value={fromDataCollectionSourceId} onChange={(e) => setFromDataCollectionSourceId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {sources?.map((s) => (
              <MenuItem key={String(s.id)} value={String(s.id)}>
                {String(s.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField label="From Description" fullWidth required value={fromDescription} onChange={(e) => setFromDescription(e.target.value)} />

          <TextField select label="To System (optional)" fullWidth value={toItSystemId} onChange={(e) => setToItSystemId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {systems?.map((s) => (
              <MenuItem key={String(s.id)} value={String(s.id)}>
                {String(s.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="To Processor (optional)" fullWidth value={toProcessorId} onChange={(e) => setToProcessorId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {processors?.map((p) => (
              <MenuItem key={String(p.id)} value={String(p.id)}>
                {String(p.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="To Recipient (optional)" fullWidth value={toRecipientId} onChange={(e) => setToRecipientId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {recipients?.map((r) => (
              <MenuItem key={String(r.id)} value={String(r.id)}>
                {String(r.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField label="To Description" fullWidth required value={toDescription} onChange={(e) => setToDescription(e.target.value)} />

          <TextField label="Transfer Mechanism" fullWidth value={transferMechanism} onChange={(e) => setTransferMechanism(e.target.value)} placeholder="e.g. API, SFTP, manual export" />
          <FormControlLabel control={<Checkbox checked={isCrossBorder} onChange={(e) => setIsCrossBorder(e.target.checked)} />} label="Cross-border transfer" />
          {isCrossBorder && <TextField label="Destination Country" fullWidth required value={crossBorderCountry} onChange={(e) => setCrossBorderCountry(e.target.value)} />}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!canSave || saving}
          onClick={() =>
            onSave({
              name,
              dataCategoryId: dataCategoryId || null,
              fromItSystemId: fromItSystemId || null,
              fromDataCollectionSourceId: fromDataCollectionSourceId || null,
              fromDescription,
              toItSystemId: toItSystemId || null,
              toProcessorId: toProcessorId || null,
              toRecipientId: toRecipientId || null,
              toDescription,
              transferMechanism: transferMechanism || null,
              isCrossBorder,
              crossBorderCountry: isCrossBorder ? crossBorderCountry : null,
            })
          }
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function DataFlowsTab() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [crossBorderOnly, setCrossBorderOnly] = useState(false);
  const [createOpen, setCreateOpen] = useState(false);

  const canManage = hasPermission("dataflows.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["data-flows", page, search, crossBorderOnly],
    queryFn: () => api.getDataFlows({ page, pageSize: PAGE_SIZE, search: search || undefined, crossBorderOnly: crossBorderOnly || undefined }),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["data-flows"] });
  const createMutation = useMutation({ mutationFn: api.createDataFlow, onSuccess: invalidate });
  const deleteMutation = useMutation({ mutationFn: api.deleteDataFlow, onSuccess: invalidate });

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: "space-between", flexWrap: "wrap", gap: 2 }}>
        <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
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
          <FormControlLabel
            control={<Checkbox checked={crossBorderOnly} onChange={(e) => { setCrossBorderOnly(e.target.checked); setPage(1); }} />}
            label="Cross-border only"
          />
        </Stack>
        {canManage && (
          <Button variant="contained" onClick={() => setCreateOpen(true)}>
            New Data Flow
          </Button>
        )}
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load data flows.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Flow #</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>From</TableCell>
                  <TableCell>To</TableCell>
                  <TableCell>Cross-Border</TableCell>
                  {canManage && <TableCell>Actions</TableCell>}
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((f) => (
                  <TableRow key={f.id} hover>
                    <TableCell>{f.flowNumber}</TableCell>
                    <TableCell>{f.name}</TableCell>
                    <TableCell>{f.fromItSystemName ?? f.fromDataCollectionSourceName ?? f.fromDescription}</TableCell>
                    <TableCell>{f.toItSystemName ?? f.toProcessorName ?? f.toRecipientName ?? f.toDescription}</TableCell>
                    <TableCell>{f.isCrossBorder && <Chip size="small" label={f.crossBorderCountry ?? "Yes"} color="warning" />}</TableCell>
                    {canManage && (
                      <TableCell>
                        <Button size="small" color="error" onClick={() => deleteMutation.mutate(f.id)}>
                          Delete
                        </Button>
                      </TableCell>
                    )}
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      No data flows recorded yet.
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

      {createOpen && (
        <FlowFormDialog
          saving={createMutation.isPending}
          onClose={() => setCreateOpen(false)}
          onSave={(payload) => {
            createMutation.mutate(payload);
            setCreateOpen(false);
          }}
        />
      )}
    </Stack>
  );
}
