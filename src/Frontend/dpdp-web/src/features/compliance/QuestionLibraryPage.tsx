import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
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
import { Link as RouterLink } from "react-router-dom";
import * as complianceApi from "./api";
import { QUESTION_TYPES } from "./types";

const PAGE_SIZE = 25;

export function QuestionLibraryPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [questionType, setQuestionType] = useState("");

  const { data, isPending, isError } = useQuery({
    queryKey: ["compliance", "questions", page, search, questionType],
    queryFn: () =>
      complianceApi.getQuestions({
        page,
        pageSize: PAGE_SIZE,
        search: search || undefined,
        questionType: questionType || undefined,
      }),
  });

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Question Library
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
          label="Question type"
          size="small"
          sx={{ minWidth: 200 }}
          value={questionType}
          onChange={(e) => {
            setQuestionType(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All types</MenuItem>
          {QUESTION_TYPES.map((type) => (
            <MenuItem key={type} value={type}>
              {type}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load questions.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Code</TableCell>
                  <TableCell>Question</TableCell>
                  <TableCell>Control</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell align="right">Evidence</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((q) => (
                  <TableRow key={q.id} hover>
                    <TableCell>{q.code}</TableCell>
                    <TableCell sx={{ maxWidth: 420 }}>{q.text}</TableCell>
                    <TableCell>
                      <RouterLink to={`/compliance/controls/${q.controlId}`}>{q.controlBusinessId}</RouterLink>
                    </TableCell>
                    <TableCell>
                      <Chip label={q.questionType} size="small" variant="outlined" />
                    </TableCell>
                    <TableCell align="right">{q.evidenceRequirements.length}</TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      No questions found.
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
