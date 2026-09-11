import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CircularProgress from "@mui/material/CircularProgress";
import LinearProgress from "@mui/material/LinearProgress";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import * as api from "./api";

export function ClassificationTab() {
  const { data, isPending, isError } = useQuery({
    queryKey: ["classification-summary"],
    queryFn: api.getClassificationSummary,
  });

  if (isPending) return <CircularProgress />;
  if (isError || !data) return <Alert severity="error">Could not load the classification summary.</Alert>;

  const maxCount = Math.max(1, ...data.categoryCounts.map((c) => c.count));

  return (
    <Stack spacing={3}>
      <Alert severity="info">
        These are informational categorizations to support your own DPDP assessment — not automated legal determinations.
        Correct any suggestion that looks wrong from the Data Elements tab.
      </Alert>

      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
        <Card variant="outlined" sx={{ minWidth: 180 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Total Elements
            </Typography>
            <Typography variant="h4">{data.totalElements}</Typography>
          </CardContent>
        </Card>
        <Card variant="outlined" sx={{ minWidth: 180 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Unclassified
            </Typography>
            <Typography variant="h4">{data.unclassifiedCount}</Typography>
          </CardContent>
        </Card>
        <Card variant="outlined" sx={{ minWidth: 180 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Human-Corrected
            </Typography>
            <Typography variant="h4">{data.humanCorrectedCount}</Typography>
          </CardContent>
        </Card>
      </Stack>

      <Typography variant="h6" component="h3">
        By Category
      </Typography>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Category</TableCell>
              <TableCell>Count</TableCell>
              <TableCell sx={{ width: "40%" }}>Share</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.categoryCounts.map((c) => (
              <TableRow key={c.category}>
                <TableCell>{c.category.replace("_", " ")}</TableCell>
                <TableCell>{c.count}</TableCell>
                <TableCell>
                  <LinearProgress variant="determinate" value={(c.count / maxCount) * 100} sx={{ height: 8, borderRadius: 1 }} />
                </TableCell>
              </TableRow>
            ))}
            {data.categoryCounts.length === 0 && (
              <TableRow>
                <TableCell colSpan={3} align="center">
                  No classified elements yet.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Stack>
  );
}
