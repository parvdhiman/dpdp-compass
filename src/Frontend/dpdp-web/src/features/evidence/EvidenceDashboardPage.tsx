import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
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
import { useAuth } from "../auth/AuthProvider";
import * as evidenceApi from "./api";
import { EVIDENCE_STATUSES, EVIDENCE_TYPES } from "./types";
import { UploadEvidenceDialog } from "./UploadEvidenceDialog";

const PAGE_SIZE = 20;

const STATUS_COLORS: Record<string, "success" | "warning" | "error" | "info" | "default"> = {
  UPLOADED: "info",
  UNDER_REVIEW: "warning",
  APPROVED: "success",
  REJECTED: "error",
  EXPIRED: "error",
  ARCHIVED: "default",
};

export function EvidenceDashboardPage() {
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [evidenceType, setEvidenceType] = useState("");
  const [expiredOnly, setExpiredOnly] = useState(false);
  const [uploadOpen, setUploadOpen] = useState(false);

  const { data, isPending, isError } = useQuery({
    queryKey: ["evidence", page, search, status, evidenceType, expiredOnly],
    queryFn: () =>
      evidenceApi.getEvidenceList({
        page,
        pageSize: PAGE_SIZE,
        search: search || undefined,
        status: status || undefined,
        evidenceType: evidenceType || undefined,
        expiredOnly: expiredOnly || undefined,
      }),
  });

  const approvedCount = data?.items.filter((e) => e.status === "APPROVED").length ?? 0;
  const underReviewCount = data?.items.filter((e) => e.status === "UNDER_REVIEW").length ?? 0;
  const expiredCount = data?.items.filter((e) => e.isExpired).length ?? 0;

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Evidence Dashboard
        </Typography>
        {hasPermission("evidence.upload") && (
          <Button variant="contained" onClick={() => setUploadOpen(true)}>
            Upload Evidence
          </Button>
        )}
      </Stack>

      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
        <Card variant="outlined" sx={{ minWidth: 160 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Approved (this page)
            </Typography>
            <Typography variant="h4">{approvedCount}</Typography>
          </CardContent>
        </Card>
        <Card variant="outlined" sx={{ minWidth: 160 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Under Review (this page)
            </Typography>
            <Typography variant="h4">{underReviewCount}</Typography>
          </CardContent>
        </Card>
        <Card variant="outlined" sx={{ minWidth: 160 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Expired (this page)
            </Typography>
            <Typography variant="h4" color={expiredCount > 0 ? "error.main" : undefined}>
              {expiredCount}
            </Typography>
          </CardContent>
        </Card>
      </Stack>

      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
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
          label="Status"
          size="small"
          sx={{ minWidth: 180 }}
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All statuses</MenuItem>
          {EVIDENCE_STATUSES.map((s) => (
            <MenuItem key={s} value={s}>
              {s.replace("_", " ")}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          label="Type"
          size="small"
          sx={{ minWidth: 180 }}
          value={evidenceType}
          onChange={(e) => {
            setEvidenceType(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All types</MenuItem>
          {EVIDENCE_TYPES.map((t) => (
            <MenuItem key={t} value={t}>
              {t.replace("_", " ")}
            </MenuItem>
          ))}
        </TextField>
        <Button
          variant={expiredOnly ? "contained" : "outlined"}
          size="small"
          onClick={() => {
            setExpiredOnly((v) => !v);
            setPage(1);
          }}
        >
          Expired only
        </Button>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load evidence.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>ID</TableCell>
                  <TableCell>Title</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Owner</TableCell>
                  <TableCell>Reviewer</TableCell>
                  <TableCell>Expiry</TableCell>
                  <TableCell>Version</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((e) => (
                  <TableRow key={e.id} hover sx={{ cursor: "pointer" }} onClick={() => navigate(`/evidence/${e.id}`)}>
                    <TableCell>{e.evidenceNumber}</TableCell>
                    <TableCell>{e.title}</TableCell>
                    <TableCell>{e.evidenceType.replace("_", " ")}</TableCell>
                    <TableCell>
                      <Chip label={e.status.replace("_", " ")} size="small" color={STATUS_COLORS[e.status] ?? "default"} />
                    </TableCell>
                    <TableCell>{e.ownerName ?? "Unassigned"}</TableCell>
                    <TableCell>{e.reviewerName ?? "—"}</TableCell>
                    <TableCell>
                      {e.expiryDate ?? "—"} {e.isExpired && <Chip label="Expired" size="small" color="error" />}
                    </TableCell>
                    <TableCell>v{e.currentVersionNumber}</TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      No evidence found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ display: "flex", justifyContent: "center" }}>
            <Pagination count={data.totalPages} page={data.page} onChange={(_, value) => setPage(value)} color="primary" />
          </Box>
        </>
      )}

      {uploadOpen && (
        <UploadEvidenceDialog
          onClose={() => setUploadOpen(false)}
          onUploaded={(id) => {
            setUploadOpen(false);
            queryClient.invalidateQueries({ queryKey: ["evidence"] });
            navigate(`/evidence/${id}`);
          }}
        />
      )}
    </Stack>
  );
}
