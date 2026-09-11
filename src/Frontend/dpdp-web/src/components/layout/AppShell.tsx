import type { ReactNode } from "react";
import AppBar from "@mui/material/AppBar";
import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";

/**
 * Pure layout — no auth-awareness, so it stays trivially testable. The
 * authenticated app composes its nav/user-menu separately (see
 * AuthenticatedLayout.tsx) and passes it in as `navItems`.
 */
export function AppShell({ children, navItems }: { children: ReactNode; navItems?: ReactNode }) {
  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      <AppBar position="static" color="primary" enableColorOnDark>
        <Toolbar sx={{ gap: 2 }}>
          <Typography variant="h6" component="div" sx={{ flexShrink: 0 }}>
            DPDP-COMPASS
          </Typography>
          <Box sx={{ flex: 1, display: "flex", gap: 1, alignItems: "center" }}>{navItems}</Box>
        </Toolbar>
      </AppBar>

      <Container component="main" sx={{ py: 4, flex: 1 }}>
        {children}
      </Container>
    </Box>
  );
}
