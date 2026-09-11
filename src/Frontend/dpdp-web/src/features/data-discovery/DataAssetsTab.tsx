import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
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
import Typography from "@mui/material/Typography";
import * as api from "./api";
import { DATA_ASSET_TYPES } from "./types";

const PAGE_SIZE = 20;

export function DataAssetsTab({ dataSourceId, onViewElements }: { dataSourceId?: string; onViewElements: (dataAssetId: string) => void }) {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [assetType, setAssetType] = useState("");

  const { data, isPending, isError } = useQuery({
    queryKey: ["data-assets", page, search, assetType, dataSourceId],
    queryFn: () => api.getDataAssets({ page, pageSize: PAGE_SIZE, search: search || undefined, assetType: assetType || undefined, dataSourceId }),
  });

  return (
    <Stack spacing={2}>
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
          label="Type"
          size="small"
          sx={{ minWidth: 160 }}
          value={assetType}
          onChange={(e) => {
            setAssetType(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All types</MenuItem>
          {DATA_ASSET_TYPES.map((t) => (
            <MenuItem key={t} value={t}>
              {t}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load data assets.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Asset</TableCell>
                  <TableCell>Data Source</TableCell>
                  <TableCell>Schema</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Row Count</TableCell>
                  <TableCell>Elements</TableCell>
                  <TableCell>Personal Data Elements</TableCell>
                  <TableCell>Last Discovered</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((a) => (
                  <TableRow key={a.id} hover sx={{ cursor: "pointer" }} onClick={() => onViewElements(a.id)}>
                    <TableCell>{a.assetName}</TableCell>
                    <TableCell>{a.dataSourceName}</TableCell>
                    <TableCell>{a.schemaName ?? "—"}</TableCell>
                    <TableCell>
                      <Chip size="small" label={a.assetType} />
                    </TableCell>
                    <TableCell>{a.estimatedRowCount ?? "—"}</TableCell>
                    <TableCell>{a.elementCount}</TableCell>
                    <TableCell>
                      {a.personalDataElementCount > 0 ? (
                        <Chip size="small" label={a.personalDataElementCount} color="warning" />
                      ) : (
                        <Typography variant="body2" color="text.secondary">
                          0
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>{a.lastDiscoveredAt ? new Date(a.lastDiscoveredAt).toLocaleString() : "—"}</TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      No data assets discovered yet.
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
    </Stack>
  );
}
