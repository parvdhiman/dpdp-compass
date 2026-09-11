import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import FormControlLabel from "@mui/material/FormControlLabel";
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
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as api from "./api";
import { CLASSIFICATION_CATEGORIES } from "./types";

const PAGE_SIZE = 20;

function ClassificationCell({ elementId, category, confidence, source, isHumanCorrected, canReview }: {
  elementId: string;
  category: string | null;
  confidence: number | null;
  source: string | null;
  isHumanCorrected: boolean;
  canReview: boolean;
}) {
  const queryClient = useQueryClient();
  const mutation = useMutation({
    mutationFn: (value: string) => api.updateDataElementClassification(elementId, value || null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["data-elements"] });
      queryClient.invalidateQueries({ queryKey: ["classification-summary"] });
    },
  });

  if (!canReview) {
    return category ? (
      <Tooltip title={`${source === "HUMAN" ? "Human-corrected" : "System-suggested"} — ${confidence ?? 0}% confidence`}>
        <Chip size="small" label={category.replace("_", " ")} color={isHumanCorrected ? "success" : "default"} variant={isHumanCorrected ? "filled" : "outlined"} />
      </Tooltip>
    ) : (
      <Typography variant="body2" color="text.secondary">
        Not classified
      </Typography>
    );
  }

  return (
    <Stack spacing={0.5}>
      <TextField
        select
        size="small"
        value={category ?? ""}
        onChange={(e) => mutation.mutate(e.target.value)}
        disabled={mutation.isPending}
        sx={{ minWidth: 200 }}
      >
        <MenuItem value="">Not personal data</MenuItem>
        {CLASSIFICATION_CATEGORIES.map((c) => (
          <MenuItem key={c} value={c}>
            {c.replace("_", " ")}
          </MenuItem>
        ))}
      </TextField>
      {category && (
        <Typography variant="caption" color="text.secondary">
          {confidence ?? 0}% confidence · {isHumanCorrected ? "human-corrected" : "system-suggested"}
        </Typography>
      )}
    </Stack>
  );
}

export function DataElementsTab({ dataAssetId }: { dataAssetId?: string }) {
  const { hasPermission } = useAuth();
  const [page, setPage] = useState(1);
  const [category, setCategory] = useState("");
  const [unclassifiedOnly, setUnclassifiedOnly] = useState(false);
  const [lowConfidenceOnly, setLowConfidenceOnly] = useState(false);

  const canReview = hasPermission("classification.review");

  const { data, isPending, isError } = useQuery({
    queryKey: ["data-elements", page, category, unclassifiedOnly, lowConfidenceOnly, dataAssetId],
    queryFn: () =>
      api.getDataElements({
        page,
        pageSize: PAGE_SIZE,
        category: category || undefined,
        unclassifiedOnly: unclassifiedOnly || undefined,
        lowConfidenceOnly: lowConfidenceOnly || undefined,
        dataAssetId,
      }),
  });

  return (
    <Stack spacing={2}>
      <Alert severity="info" sx={{ py: 0.5 }}>
        Classifications are system-generated suggestions, not legal determinations — review and correct them as needed.
      </Alert>

      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap", alignItems: "center" }}>
        <TextField
          select
          label="Category"
          size="small"
          sx={{ minWidth: 200 }}
          value={category}
          onChange={(e) => {
            setCategory(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All categories</MenuItem>
          {CLASSIFICATION_CATEGORIES.map((c) => (
            <MenuItem key={c} value={c}>
              {c.replace("_", " ")}
            </MenuItem>
          ))}
        </TextField>
        <FormControlLabel
          control={<Switch size="small" checked={unclassifiedOnly} onChange={(e) => { setUnclassifiedOnly(e.target.checked); setPage(1); }} />}
          label="Unclassified only"
        />
        <FormControlLabel
          control={<Switch size="small" checked={lowConfidenceOnly} onChange={(e) => { setLowConfidenceOnly(e.target.checked); setPage(1); }} />}
          label="Low confidence only"
        />
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load data elements.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Asset</TableCell>
                  <TableCell>Column</TableCell>
                  <TableCell>Data Type</TableCell>
                  <TableCell>Nullable</TableCell>
                  <TableCell>Sample</TableCell>
                  <TableCell>Classification</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((e) => (
                  <TableRow key={e.id} hover>
                    <TableCell>{e.assetName}</TableCell>
                    <TableCell>{e.columnName}</TableCell>
                    <TableCell>{e.dataType}</TableCell>
                    <TableCell>{e.isNullable ? "Yes" : "No"}</TableCell>
                    <TableCell sx={{ fontFamily: "monospace" }}>{e.sampleMaskedValue ?? "—"}</TableCell>
                    <TableCell>
                      <ClassificationCell
                        elementId={e.id}
                        category={e.classificationCategory}
                        confidence={e.classificationConfidence}
                        source={e.classificationSource}
                        isHumanCorrected={e.isHumanCorrected}
                        canReview={canReview}
                      />
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      No data elements found.
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
