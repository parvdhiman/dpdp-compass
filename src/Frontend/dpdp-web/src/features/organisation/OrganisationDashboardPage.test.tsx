import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { AuthProvider } from "../auth/AuthProvider";
import * as authApi from "../auth/api";
import { OrganisationDashboardPage } from "./OrganisationDashboardPage";
import * as organisationApi from "./api";

function renderDashboard() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <OrganisationDashboardPage />
      </AuthProvider>
    </QueryClientProvider>,
  );
}

describe("OrganisationDashboardPage", () => {
  it("renders organisation stats once the user session resolves", async () => {
    vi.spyOn(authApi, "refresh").mockResolvedValue({
      accessToken: "access-token",
      accessTokenExpiresAt: new Date().toISOString(),
      refreshToken: "refresh-token",
      refreshTokenExpiresAt: new Date().toISOString(),
      user: {
        id: "u1",
        organisationId: "org1",
        email: "admin@test.local",
        fullName: "Test Admin",
        isSuperAdministrator: false,
        roles: ["Organisation Administrator"],
        permissions: ["organisation.read"],
      },
    });
    localStorage.setItem("dpdp.refreshToken", "refresh-token");

    vi.spyOn(organisationApi, "getOrganisationDashboard").mockResolvedValue({
      organisationId: "org1",
      name: "Drishinfo HQ",
      industry: "Software",
      size: "Medium",
      businessUnitCount: 3,
      departmentCount: 7,
      activeUserCount: 12,
      primaryLocation: {
        id: "loc1",
        label: "Head Office",
        addressLine1: null,
        addressLine2: null,
        city: "Mumbai",
        state: null,
        postalCode: null,
        country: "India",
        isPrimary: true,
      },
      hasDpoConfigured: true,
    });

    renderDashboard();

    await waitFor(() => expect(screen.getByText("Drishinfo HQ")).toBeInTheDocument());
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("7")).toBeInTheDocument();
    expect(screen.getByText("12")).toBeInTheDocument();
    expect(screen.getByText("Head Office")).toBeInTheDocument();

    localStorage.clear();
  });
});
