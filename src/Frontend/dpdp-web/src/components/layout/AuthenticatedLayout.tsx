import type { ReactNode } from "react";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { useAuth } from "../../features/auth/AuthProvider";
import { AppShell } from "./AppShell";

export function AuthenticatedLayout({ children }: { children: ReactNode }) {
  const { user, hasPermission, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  const navItems = (
    <>
      <Button component={RouterLink} to="/" color="inherit">
        Dashboard
      </Button>
      {hasPermission("organisation.read") && (
        <Button component={RouterLink} to="/organisation" color="inherit">
          Organisation
        </Button>
      )}
      {hasPermission("businessunits.read") && (
        <Button component={RouterLink} to="/business-units" color="inherit">
          Business Units
        </Button>
      )}
      {hasPermission("departments.read") && (
        <Button component={RouterLink} to="/departments" color="inherit">
          Departments
        </Button>
      )}
      {hasPermission("users.read") && (
        <Button component={RouterLink} to="/users" color="inherit">
          Users
        </Button>
      )}
      {hasPermission("assessments.read") && (
        <Button component={RouterLink} to="/assessments" color="inherit">
          Assessments
        </Button>
      )}
      {hasPermission("controls.read") && (
        <Button component={RouterLink} to="/compliance/frameworks" color="inherit">
          Frameworks
        </Button>
      )}
      {hasPermission("controls.read") && (
        <Button component={RouterLink} to="/compliance/controls" color="inherit">
          Controls
        </Button>
      )}
      {hasPermission("controls.read") && (
        <Button component={RouterLink} to="/compliance/questions" color="inherit">
          Questions
        </Button>
      )}
      {hasPermission("controls.read") && (
        <Button component={RouterLink} to="/compliance/evidence-requirements" color="inherit">
          Evidence Requirements
        </Button>
      )}
      {hasPermission("evidence.read") && (
        <Button component={RouterLink} to="/evidence" color="inherit">
          Evidence
        </Button>
      )}
      {hasPermission("findings.read") && (
        <Button component={RouterLink} to="/findings" color="inherit">
          Findings
        </Button>
      )}
      {hasPermission("risks.read") && (
        <Button component={RouterLink} to="/risks" color="inherit">
          Risks
        </Button>
      )}
      {hasPermission("remediation.read") && (
        <Button component={RouterLink} to="/remediation-tasks" color="inherit">
          Remediation
        </Button>
      )}
      {hasPermission("remediation.read") && (
        <Button component={RouterLink} to="/remediation-tasks/overdue" color="inherit">
          Overdue
        </Button>
      )}
      {hasPermission("datasources.read") && (
        <Button component={RouterLink} to="/data-discovery" color="inherit">
          Data Discovery
        </Button>
      )}
      {hasPermission("datainventory.read") && (
        <Button component={RouterLink} to="/data-inventory" color="inherit">
          Data Inventory
        </Button>
      )}
      {hasPermission("privacynotices.read") && (
        <Button component={RouterLink} to="/consent-privacy" color="inherit">
          Consent &amp; Privacy
        </Button>
      )}
      {hasPermission("roles.read") && (
        <Button component={RouterLink} to="/roles" color="inherit">
          Roles
        </Button>
      )}
      {hasPermission("roles.read") && (
        <Button component={RouterLink} to="/permissions" color="inherit">
          Permissions
        </Button>
      )}

      <Stack direction="row" spacing={1} sx={{ alignItems: "center", ml: "auto" }}>
        <Button component={RouterLink} to="/profile" color="inherit">
          {user?.fullName}
        </Button>
        <Typography variant="caption" sx={{ opacity: 0.8 }}>
          {user?.isSuperAdministrator ? "Super Administrator" : user?.roles.join(", ")}
        </Typography>
        <Button color="inherit" variant="outlined" onClick={handleLogout}>
          Logout
        </Button>
      </Stack>
    </>
  );

  return <AppShell navItems={navItems}>{children}</AppShell>;
}
