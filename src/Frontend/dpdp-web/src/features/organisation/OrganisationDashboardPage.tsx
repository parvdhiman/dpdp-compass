import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as organisationApi from "./api";

function StatCard({ label, value }: { label: string; value: string | number }) {
  return (
    <Card variant="outlined" sx={{ minWidth: 200, flex: "1 1 200px" }}>
      <CardContent>
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="h4">{value}</Typography>
      </CardContent>
    </Card>
  );
}

export function OrganisationDashboardPage() {
  const { user } = useAuth();
  const organisationId = user?.organisationId ?? null;

  const { data, isPending, isError } = useQuery({
    queryKey: ["organisation-dashboard", organisationId],
    queryFn: () => organisationApi.getOrganisationDashboard(organisationId!),
    enabled: !!organisationId,
  });

  if (!organisationId) {
    return (
      <Alert severity="info">
        Super Administrator accounts aren't attached to an organisation — use{" "}
        <code>/api/v1/organisations</code> to browse tenants.
      </Alert>
    );
  }

  if (isPending) return <CircularProgress />;
  if (isError || !data) return <Alert severity="error">Could not load the organisation dashboard.</Alert>;

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        {data.name}
      </Typography>
      <Stack direction="row" spacing={1}>
        {data.industry && <Chip label={data.industry} />}
        {data.size && <Chip label={data.size} variant="outlined" />}
        {!data.hasDpoConfigured && <Chip label="No DPO configured" color="warning" />}
      </Stack>

      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 2 }}>
        <StatCard label="Business Units" value={data.businessUnitCount} />
        <StatCard label="Departments" value={data.departmentCount} />
        <StatCard label="Active Users" value={data.activeUserCount} />
        <StatCard label="Primary Location" value={data.primaryLocation?.label ?? "Not set"} />
      </Box>
    </Stack>
  );
}
