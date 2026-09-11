import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import Accordion from "@mui/material/Accordion";
import AccordionDetails from "@mui/material/AccordionDetails";
import AccordionSummary from "@mui/material/AccordionSummary";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import MenuItem from "@mui/material/MenuItem";
import Pagination from "@mui/material/Pagination";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as risksApi from "./api";
import { RiskDialog } from "./RiskDialog";
import { RISK_LEVELS, RISK_STATUSES } from "./types";

const PAGE_SIZE = 20;

const LEVEL_COLORS: Record<string, "error" | "warning" | "info" | "success"> = {
  CRITICAL: "error",
  HIGH: "error",
  MEDIUM: "warning",
  LOW: "success",
};

export function RiskRegisterPage() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [riskLevel, setRiskLevel] = useState("");
  const [status, setStatus] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const canManage = hasPermission("risks.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["risks", page, search, riskLevel, status],
    queryFn: () => risksApi.getRisks({ page, pageSize: PAGE_SIZE, search: search || undefined, riskLevel: riskLevel || undefined, status: status || undefined }),
  });

  const { data: editingRisk } = useQuery({
    queryKey: ["risk", editingId],
    queryFn: () => risksApi.getRiskById(editingId!),
    enabled: !!editingId,
  });

  const handleSaved = () => {
    setDialogOpen(false);
    setEditingId(null);
    queryClient.invalidateQueries({ queryKey: ["risks"] });
  };

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Risk Register
        </Typography>
        {canManage && (
          <Button
            variant="contained"
            onClick={() => {
              setEditingId(null);
              setDialogOpen(true);
            }}
          >
            New Risk
          </Button>
        )}
      </Stack>

      <Typography variant="body2" color="text.secondary">
        Risk methodology (Likelihood × Impact × Data Sensitivity × Exposure) is configurable — see docs/RISK_METHODOLOGY.md.
      </Typography>

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
          label="Risk Level"
          size="small"
          sx={{ minWidth: 160 }}
          value={riskLevel}
          onChange={(e) => {
            setRiskLevel(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All levels</MenuItem>
          {RISK_LEVELS.map((l) => (
            <MenuItem key={l} value={l}>
              {l}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          label="Status"
          size="small"
          sx={{ minWidth: 160 }}
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All statuses</MenuItem>
          {RISK_STATUSES.map((s) => (
            <MenuItem key={s} value={s}>
              {s}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load risks.</Alert>}

      {data && (
        <>
          <Stack spacing={1}>
            {data.items.map((risk) => (
              <Accordion key={risk.id}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Stack direction="row" spacing={2} sx={{ alignItems: "center", width: "100%" }}>
                    <Typography variant="caption" color="text.secondary" sx={{ minWidth: 90 }}>
                      {risk.riskNumber}
                    </Typography>
                    <Typography sx={{ flex: 1 }}>{risk.title}</Typography>
                    <Chip label={`${risk.calculatedRiskLevel} (${risk.calculatedRiskScore.toFixed(0)})`} size="small" color={LEVEL_COLORS[risk.calculatedRiskLevel] ?? "default"} />
                    <Chip label={risk.status} size="small" variant="outlined" />
                  </Stack>
                </AccordionSummary>
                <AccordionDetails>
                  <Stack spacing={1.5}>
                    <Stack direction="row" spacing={3}>
                      <Typography variant="body2">Likelihood: {risk.likelihood.replace("_", " ")}</Typography>
                      <Typography variant="body2">Impact: {risk.impact}</Typography>
                      <Typography variant="body2">Data Sensitivity: {risk.dataSensitivity}</Typography>
                      <Typography variant="body2">Exposure: {risk.exposure}</Typography>
                    </Stack>
                    <Typography variant="body2" color="text.secondary">
                      Owner: {risk.ownerName ?? "Unassigned"} · {risk.findingCount} linked finding(s)
                    </Typography>
                    {canManage && (
                      <Button
                        size="small"
                        variant="outlined"
                        sx={{ alignSelf: "flex-start" }}
                        onClick={() => {
                          setEditingId(risk.id);
                          setDialogOpen(true);
                        }}
                      >
                        Edit
                      </Button>
                    )}
                  </Stack>
                </AccordionDetails>
              </Accordion>
            ))}
            {data.items.length === 0 && <Alert severity="info">No risks recorded yet.</Alert>}
          </Stack>

          <Box sx={{ display: "flex", justifyContent: "center" }}>
            <Pagination count={data.totalPages} page={data.page} onChange={(_, value) => setPage(value)} color="primary" />
          </Box>
        </>
      )}

      {dialogOpen && (!editingId || editingRisk) && (
        <RiskDialog risk={editingId ? editingRisk! : null} onClose={() => { setDialogOpen(false); setEditingId(null); }} onSaved={handleSaved} />
      )}
    </Stack>
  );
}
