import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { useParams } from "react-router-dom";
import { useAuth } from "../auth/AuthProvider";
import * as complianceApi from "./api";

export function FrameworkVersionPage() {
  const { id } = useParams<{ id: string }>();
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();

  const { data, isPending, isError } = useQuery({
    queryKey: ["compliance", "framework-version", id],
    queryFn: () => complianceApi.getFrameworkVersion(id!),
    enabled: !!id,
  });

  const activateMutation = useMutation({
    mutationFn: () => complianceApi.activateFrameworkVersion(id!),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["compliance"] }),
  });

  if (isPending) return <CircularProgress />;
  if (isError || !data) return <Alert severity="error">Could not load this framework version.</Alert>;

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start" }}>
        <Stack spacing={0.5}>
          <Typography variant="h4" component="h1">
            {data.frameworkName} — {data.versionLabel}
          </Typography>
          <Stack direction="row" spacing={1}>
            {data.isCurrent && <Chip label="Current version" color="success" size="small" />}
            <Chip label={`Review: ${data.reviewStatus}`} size="small" variant="outlined" />
          </Stack>
        </Stack>
        {hasPermission("controls.manage") && !data.isCurrent && (
          <Button variant="outlined" onClick={() => activateMutation.mutate()} disabled={activateMutation.isPending}>
            Make current version
          </Button>
        )}
      </Stack>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={1}>
          {data.officialCitation && <Typography variant="body2">Official citation: {data.officialCitation}</Typography>}
          <Typography variant="body2">Published: {data.publicationDate ?? "—"}</Typography>
          <Typography variant="body2">
            Effective: {data.effectiveDate ?? "Not yet notified by the Central Government"}
          </Typography>
          {data.sourceUrl && (
            <Typography variant="body2">
              Source: <a href={data.sourceUrl} target="_blank" rel="noreferrer">{data.sourceUrl}</a>
            </Typography>
          )}
          {data.changeSummary && <Typography variant="body2" color="text.secondary">{data.changeSummary}</Typography>}
        </Stack>
      </Paper>

      <Typography variant="h5" component="h2">
        Legal References
      </Typography>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Citation</TableCell>
              <TableCell>Title</TableCell>
              <TableCell>Summary</TableCell>
              <TableCell>Source</TableCell>
              <TableCell>Review status</TableCell>
              <TableCell align="right">Requirements</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.legalReferences.map((ref) => (
              <TableRow key={ref.id}>
                <TableCell>{ref.citation}</TableCell>
                <TableCell>{ref.title}</TableCell>
                <TableCell sx={{ maxWidth: 360 }}>{ref.summaryText ?? "—"}</TableCell>
                <TableCell sx={{ maxWidth: 280 }}>
                  <Typography variant="caption" color="text.secondary">
                    {ref.sourceCitation}
                  </Typography>
                </TableCell>
                <TableCell>{ref.reviewStatus}</TableCell>
                <TableCell align="right">{ref.requirementCount}</TableCell>
              </TableRow>
            ))}
            {data.legalReferences.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  No legal references recorded for this version yet.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Stack>
  );
}
