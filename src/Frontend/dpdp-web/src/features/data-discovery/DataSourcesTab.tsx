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
import Switch from "@mui/material/Switch";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as api from "./api";
import { DATA_SOURCE_TYPES } from "./types";

const PAGE_SIZE = 20;

function CreateDataSourceDialog({ onClose, onCreated }: { onClose: () => void; onCreated: (id: string) => void }) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [sourceType, setSourceType] = useState("POSTGRESQL");
  const [host, setHost] = useState("");
  const [port, setPort] = useState("");
  const [databaseName, setDatabaseName] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [rootPath, setRootPath] = useState("");
  const [schemaFilter, setSchemaFilter] = useState("");
  const [serverError, setServerError] = useState<string | null>(null);

  const isFileSystem = sourceType === "FILE_SYSTEM";
  const mutation = useMutation({ mutationFn: api.createDataSource });

  const canSubmit = name.trim().length > 0 && (isFileSystem ? rootPath.trim().length > 0 : host.trim() && databaseName.trim() && username.trim() && password.trim());

  const onSubmit = async () => {
    setServerError(null);
    try {
      const created = await mutation.mutateAsync({
        name,
        description: description || null,
        sourceType,
        host: isFileSystem ? null : host,
        port: isFileSystem || !port ? null : Number(port),
        databaseName: isFileSystem ? null : databaseName,
        username: isFileSystem ? null : username,
        password: isFileSystem ? null : password,
        rootPath: isFileSystem ? rootPath : null,
        schemaFilter: isFileSystem ? null : schemaFilter || null,
      });
      onCreated(created.id);
    } catch {
      setServerError("Could not register this data source. Check the connection details and try again.");
    }
  };

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Register Data Source</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {serverError && <Alert severity="error">{serverError}</Alert>}
          <TextField label="Name" fullWidth required value={name} onChange={(e) => setName(e.target.value)} />
          <TextField label="Description" fullWidth value={description} onChange={(e) => setDescription(e.target.value)} />
          <TextField select label="Source Type" fullWidth value={sourceType} onChange={(e) => setSourceType(e.target.value)}>
            {DATA_SOURCE_TYPES.map((t) => (
              <MenuItem key={t} value={t}>
                {t.replace("_", " ")}
              </MenuItem>
            ))}
          </TextField>

          {isFileSystem ? (
            <TextField label="Root Path" fullWidth required value={rootPath} onChange={(e) => setRootPath(e.target.value)} helperText="Absolute path on the server to scan" />
          ) : (
            <>
              <Stack direction="row" spacing={2}>
                <TextField label="Host" fullWidth required value={host} onChange={(e) => setHost(e.target.value)} />
                <TextField label="Port" sx={{ width: 140 }} value={port} onChange={(e) => setPort(e.target.value)} />
              </Stack>
              <TextField label="Database Name" fullWidth required value={databaseName} onChange={(e) => setDatabaseName(e.target.value)} />
              <TextField label="Username" fullWidth required value={username} onChange={(e) => setUsername(e.target.value)} />
              <TextField label="Password" type="password" fullWidth required value={password} onChange={(e) => setPassword(e.target.value)} />
              <TextField
                label="Schema Filter (optional)"
                fullWidth
                value={schemaFilter}
                onChange={(e) => setSchemaFilter(e.target.value)}
                helperText="Restrict scanning to one schema instead of the whole database"
              />
            </>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" disabled={!canSubmit || mutation.isPending} onClick={onSubmit}>
          {mutation.isPending ? "Registering…" : "Register"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function DataSourcesTab({ onViewAssets }: { onViewAssets: (dataSourceId: string) => void }) {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [testingId, setTestingId] = useState<string | null>(null);
  const [testResult, setTestResult] = useState<{ id: string; succeeded: boolean; message: string | null } | null>(null);

  const canManage = hasPermission("datasources.manage");
  const canStartJob = hasPermission("discoveryjobs.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["data-sources", page, search],
    queryFn: () => api.getDataSources({ page, pageSize: PAGE_SIZE, search: search || undefined }),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["data-sources"] });

  const testMutation = useMutation({
    mutationFn: (id: string) => api.testDataSourceConnection(id),
    onSuccess: (result, id) => {
      setTestResult({ id, succeeded: result.succeeded, message: result.errorMessage });
      invalidate();
    },
    onSettled: () => setTestingId(null),
  });

  const startJobMutation = useMutation({
    mutationFn: (id: string) => api.startDiscoveryJob(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["discovery-jobs"] }),
  });

  const deleteMutation = useMutation({ mutationFn: api.deleteDataSource, onSuccess: invalidate });

  const toggleActiveMutation = useMutation({
    mutationFn: async (vars: { id: string; isActive: boolean }) => {
      // PUT replaces every editable field, so the current values must be
      // fetched first — passing only { isActive } would blank out
      // host/database/username for this source.
      const current = await api.getDataSourceById(vars.id);
      return api.updateDataSource(vars.id, {
        name: current.name,
        description: current.description,
        sourceType: current.sourceType,
        host: current.host,
        port: current.port,
        databaseName: current.databaseName,
        username: current.username,
        password: null,
        rootPath: current.rootPath,
        schemaFilter: current.schemaFilter,
        isActive: vars.isActive,
      });
    },
    onSuccess: invalidate,
  });

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
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
        {canManage && (
          <Button variant="contained" onClick={() => setCreateOpen(true)}>
            Register Data Source
          </Button>
        )}
      </Stack>

      {testResult && (
        <Alert severity={testResult.succeeded ? "success" : "error"} onClose={() => setTestResult(null)}>
          {testResult.succeeded ? "Connection succeeded." : `Connection failed: ${testResult.message}`}
        </Alert>
      )}

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load data sources.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Active</TableCell>
                  <TableCell>Last Test</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((s) => (
                  <TableRow key={s.id} hover>
                    <TableCell>
                      <Typography variant="body2" sx={{ cursor: "pointer" }} onClick={() => onViewAssets(s.id)}>
                        {s.name}
                      </Typography>
                    </TableCell>
                    <TableCell>{s.sourceType.replace("_", " ")}</TableCell>
                    <TableCell>
                      <Switch
                        size="small"
                        checked={s.isActive}
                        disabled={!canManage}
                        onChange={(e) => toggleActiveMutation.mutate({ id: s.id, isActive: e.target.checked })}
                      />
                    </TableCell>
                    <TableCell>
                      {s.lastTestedAt ? (
                        <Chip
                          size="small"
                          label={s.lastTestSucceeded ? "Succeeded" : "Failed"}
                          color={s.lastTestSucceeded ? "success" : "error"}
                        />
                      ) : (
                        "Never tested"
                      )}
                    </TableCell>
                    <TableCell>
                      <Stack direction="row" spacing={1}>
                        {canManage && (
                          <Button
                            size="small"
                            disabled={testingId === s.id}
                            onClick={() => {
                              setTestingId(s.id);
                              testMutation.mutate(s.id);
                            }}
                          >
                            Test
                          </Button>
                        )}
                        {canStartJob && s.isActive && (
                          <Button size="small" onClick={() => startJobMutation.mutate(s.id)} disabled={startJobMutation.isPending}>
                            Scan Now
                          </Button>
                        )}
                        <Button size="small" onClick={() => onViewAssets(s.id)}>
                          View Assets
                        </Button>
                        {canManage && (
                          <Button size="small" color="error" onClick={() => deleteMutation.mutate(s.id)}>
                            Delete
                          </Button>
                        )}
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      No data sources registered yet.
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
        <CreateDataSourceDialog
          onClose={() => setCreateOpen(false)}
          onCreated={() => {
            setCreateOpen(false);
            invalidate();
          }}
        />
      )}
    </Stack>
  );
}
