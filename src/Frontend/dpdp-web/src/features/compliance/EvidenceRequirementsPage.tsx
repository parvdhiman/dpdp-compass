import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
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
import * as complianceApi from "./api";

const PAGE_SIZE = 25;

export function EvidenceRequirementsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");

  const { data, isPending, isError } = useQuery({
    queryKey: ["compliance", "evidence-requirements", page, search],
    queryFn: () => complianceApi.getEvidenceRequirements({ page, pageSize: PAGE_SIZE, search: search || undefined }),
  });

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Evidence Requirements
      </Typography>

      <TextField
        label="Search"
        size="small"
        sx={{ maxWidth: 360 }}
        value={search}
        onChange={(e) => {
          setSearch(e.target.value);
          setPage(1);
        }}
      />

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load evidence requirements.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Question</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell>Acceptable formats</TableCell>
                  <TableCell>Mandatory</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((e) => (
                  <TableRow key={e.id} hover>
                    <TableCell>{e.name}</TableCell>
                    <TableCell>{e.questionCode}</TableCell>
                    <TableCell sx={{ maxWidth: 360 }}>{e.description ?? "—"}</TableCell>
                    <TableCell>{e.acceptableFormats ?? "—"}</TableCell>
                    <TableCell>
                      <Chip label={e.isMandatory ? "Mandatory" : "Optional"} size="small" color={e.isMandatory ? "primary" : "default"} />
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      No evidence requirements found.
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
    </Stack>
  );
}
