import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CircularProgress from "@mui/material/CircularProgress";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { getSystemInfo } from "./api";

export function SystemStatusPage() {
  const { data, isPending, isError, error } = useQuery({
    queryKey: ["system-info"],
    queryFn: getSystemInfo,
  });

  return (
    <Stack spacing={3} sx={{ maxWidth: 480 }}>
      <Typography variant="h4" component="h1">
        DPDP-COMPASS
      </Typography>
      <Typography variant="body1" color="text.secondary">
        DPDP Compliance Management &amp; Continuous Assessment Platform —
        Project Foundation.
      </Typography>

      <Card variant="outlined">
        <CardContent>
          <Typography variant="h6" gutterBottom>
            System Status
          </Typography>

          {isPending && <CircularProgress size={24} />}

          {isError && (
            <Alert severity="error">
              Could not reach the API: {error.message}
            </Alert>
          )}

          {data && (
            <Stack spacing={1}>
              <Typography>
                <strong>Application:</strong> {data.applicationName}
              </Typography>
              <Typography>
                <strong>Version:</strong> {data.version}
              </Typography>
              <Typography>
                <strong>Environment:</strong> {data.environment}
              </Typography>
              <Typography>
                <strong>Server time (UTC):</strong>{" "}
                {new Date(data.serverTimeUtc).toLocaleString()}
              </Typography>
            </Stack>
          )}
        </CardContent>
      </Card>
    </Stack>
  );
}
