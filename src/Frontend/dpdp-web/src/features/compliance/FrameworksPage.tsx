import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
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
import { Link as RouterLink } from "react-router-dom";
import * as complianceApi from "./api";

export function FrameworksPage() {
  const { data, isPending, isError } = useQuery({
    queryKey: ["compliance", "frameworks"],
    queryFn: complianceApi.getFrameworks,
  });

  return (
    <Stack spacing={3}>
      <Stack spacing={0.5}>
        <Typography variant="h4" component="h1">
          Compliance Frameworks
        </Typography>
        <Typography variant="body2" color="text.secondary">
          The legal frameworks this platform assesses against, each with one or more versions. See{" "}
          <code>docs/COMPLIANCE_CONTENT_GOVERNANCE.md</code> for how this content is sourced and reviewed.
        </Typography>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load frameworks.</Alert>}

      {data?.map((framework) => (
        <Paper key={framework.id} variant="outlined" sx={{ p: 2 }}>
          <Stack spacing={1}>
            <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
              <Typography variant="h6">{framework.name}</Typography>
              <Chip label={framework.code} size="small" />
              <Chip label={framework.jurisdiction} size="small" variant="outlined" />
            </Stack>
            <Typography variant="body2" color="text.secondary">
              Issuing authority: {framework.issuingAuthority}
            </Typography>
            {framework.description && <Typography variant="body2">{framework.description}</Typography>}

            <TableContainer sx={{ mt: 1 }}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Version</TableCell>
                    <TableCell>Current</TableCell>
                    <TableCell>Review status</TableCell>
                    <TableCell>Published</TableCell>
                    <TableCell>Effective</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {framework.versions.map((version) => (
                    <TableRow key={version.id} hover>
                      <TableCell>
                        <RouterLink to={`/compliance/framework-versions/${version.id}`}>{version.versionLabel}</RouterLink>
                      </TableCell>
                      <TableCell>
                        {version.isCurrent ? <Chip label="Current" color="success" size="small" /> : "—"}
                      </TableCell>
                      <TableCell>{version.reviewStatus}</TableCell>
                      <TableCell>{version.publicationDate ?? "—"}</TableCell>
                      <TableCell>{version.effectiveDate ?? "Not yet notified"}</TableCell>
                    </TableRow>
                  ))}
                  {framework.versions.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} align="center">
                        No versions yet.
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Stack>
        </Paper>
      ))}
    </Stack>
  );
}
