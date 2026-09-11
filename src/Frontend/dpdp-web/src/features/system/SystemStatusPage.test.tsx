import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { SystemStatusPage } from "./SystemStatusPage";
import * as api from "./api";

function renderWithQueryClient() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <SystemStatusPage />
    </QueryClientProvider>,
  );
}

describe("SystemStatusPage", () => {
  it("renders system info returned by the API", async () => {
    vi.spyOn(api, "getSystemInfo").mockResolvedValue({
      applicationName: "DPDP-COMPASS",
      version: "0.1.0",
      environment: "Development",
      serverTimeUtc: "2026-09-07T00:00:00Z",
    });

    renderWithQueryClient();

    expect(
      screen.getByRole("heading", { level: 1, name: "DPDP-COMPASS" }),
    ).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("0.1.0")).toBeInTheDocument();
    });
    expect(screen.getByText("Development")).toBeInTheDocument();
    expect(
      screen.getByText((_, element) => element?.textContent === "Application: DPDP-COMPASS"),
    ).toBeInTheDocument();
  });

  it("shows an error message when the API call fails", async () => {
    vi.spyOn(api, "getSystemInfo").mockRejectedValue(new Error("network down"));

    renderWithQueryClient();

    await waitFor(() => {
      expect(screen.getByText(/Could not reach the API/)).toBeInTheDocument();
    });
  });
});
