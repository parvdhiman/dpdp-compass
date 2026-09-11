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

export interface CatalogField {
  key: string;
  label: string;
  type: "text" | "select" | "number";
  options?: readonly string[];
  required?: boolean;
}

export interface CatalogColumn {
  key: string;
  label: string;
  render?: (value: unknown, row: Record<string, unknown>) => string;
}

export interface CatalogApi {
  list: (search?: string, isActive?: boolean) => Promise<Record<string, unknown>[]>;
  create: (payload: Record<string, unknown>) => Promise<Record<string, unknown>>;
  update: (id: string, payload: Record<string, unknown>) => Promise<Record<string, unknown>>;
  remove: (id: string) => Promise<void>;
}

function CatalogFormDialog({
  title, fields, initialValues, onClose, onSave, saving,
}: {
  title: string;
  fields: CatalogField[];
  initialValues: Record<string, unknown>;
  onClose: () => void;
  onSave: (values: Record<string, unknown>) => void;
  saving: boolean;
}) {
  const [values, setValues] = useState<Record<string, unknown>>(initialValues);

  const canSave = fields.every((f) => !f.required || String(values[f.key] ?? "").trim().length > 0);

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {fields.map((field) =>
            field.type === "select" ? (
              <TextField
                key={field.key}
                select
                label={field.label}
                fullWidth
                required={field.required}
                value={values[field.key] ?? ""}
                onChange={(e) => setValues((v) => ({ ...v, [field.key]: e.target.value }))}
              >
                {!field.required && <MenuItem value="">—</MenuItem>}
                {field.options?.map((opt) => (
                  <MenuItem key={opt} value={opt}>
                    {opt.replace(/_/g, " ")}
                  </MenuItem>
                ))}
              </TextField>
            ) : (
              <TextField
                key={field.key}
                type={field.type === "number" ? "number" : "text"}
                label={field.label}
                fullWidth
                required={field.required}
                value={values[field.key] ?? ""}
                onChange={(e) => setValues((v) => ({ ...v, [field.key]: field.type === "number" ? Number(e.target.value) : e.target.value }))}
              />
            ),
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" disabled={!canSave || saving} onClick={() => onSave(values)}>
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function GenericCatalogManager({
  storageKey, title, api, fields, columns, permission,
}: {
  storageKey: string;
  title: string;
  api: CatalogApi;
  fields: CatalogField[];
  columns: CatalogColumn[];
  permission: string;
}) {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [dialogState, setDialogState] = useState<{ mode: "create" } | { mode: "edit"; item: Record<string, unknown> } | null>(null);

  const canManage = hasPermission(permission);
  const queryKey = [storageKey, search];

  const { data, isPending, isError } = useQuery({ queryKey, queryFn: () => api.list(search || undefined) });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: [storageKey] });

  const createMutation = useMutation({ mutationFn: api.create, onSuccess: invalidate });
  const updateMutation = useMutation({ mutationFn: (vars: { id: string; payload: Record<string, unknown> }) => api.update(vars.id, vars.payload), onSuccess: invalidate });
  const deleteMutation = useMutation({ mutationFn: api.remove, onSuccess: invalidate });

  const emptyValues = () => Object.fromEntries(fields.map((f) => [f.key, f.type === "number" ? 0 : ""]));

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <TextField label="Search" size="small" sx={{ minWidth: 220 }} value={search} onChange={(e) => setSearch(e.target.value)} />
        {canManage && (
          <Button variant="contained" onClick={() => setDialogState({ mode: "create" })}>
            Add {title}
          </Button>
        )}
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load {title.toLowerCase()} records.</Alert>}

      {data && (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                {columns.map((c) => (
                  <TableCell key={c.key}>{c.label}</TableCell>
                ))}
                <TableCell>Status</TableCell>
                {canManage && <TableCell>Actions</TableCell>}
              </TableRow>
            </TableHead>
            <TableBody>
              {data.map((row) => (
                <TableRow key={String(row.id)} hover>
                  {columns.map((c) => (
                    <TableCell key={c.key}>{c.render ? c.render(row[c.key], row) : String(row[c.key] ?? "—")}</TableCell>
                  ))}
                  <TableCell>
                    <Chip size="small" label={row.isActive ? "Active" : "Inactive"} color={row.isActive ? "success" : "default"} />
                  </TableCell>
                  {canManage && (
                    <TableCell>
                      <Stack direction="row" spacing={1}>
                        <Button size="small" onClick={() => setDialogState({ mode: "edit", item: row })}>
                          Edit
                        </Button>
                        <Button size="small" color="error" onClick={() => deleteMutation.mutate(String(row.id))}>
                          Delete
                        </Button>
                      </Stack>
                    </TableCell>
                  )}
                </TableRow>
              ))}
              {data.length === 0 && (
                <TableRow>
                  <TableCell colSpan={columns.length + 2} align="center">
                    No {title.toLowerCase()} records yet.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {dialogState && (
        <CatalogFormDialog
          title={dialogState.mode === "create" ? `Add ${title}` : `Edit ${title}`}
          fields={fields}
          initialValues={dialogState.mode === "create" ? { ...emptyValues(), isActive: true } : dialogState.item}
          saving={createMutation.isPending || updateMutation.isPending}
          onClose={() => setDialogState(null)}
          onSave={(values) => {
            if (dialogState.mode === "create") {
              createMutation.mutate(values);
            } else {
              updateMutation.mutate({ id: String(dialogState.item.id), payload: { ...values, isActive: dialogState.item.isActive ?? true } });
            }
            setDialogState(null);
          }}
        />
      )}
    </Stack>
  );
}
