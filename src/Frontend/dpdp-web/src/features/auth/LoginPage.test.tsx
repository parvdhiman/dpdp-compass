import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../lib/apiClient";
import { AuthProvider } from "./AuthProvider";
import { LoginPage } from "./LoginPage";
import * as authApi from "./api";

describe("LoginPage", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it("logs in and navigates away from /login on success", async () => {
    vi.spyOn(authApi, "login").mockResolvedValue({
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
        permissions: ["users.read"],
      },
    });

    render(
      <AuthProvider>
        <MemoryRouter initialEntries={["/login"]}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/" element={<div>Home Page</div>} />
          </Routes>
        </MemoryRouter>
      </AuthProvider>,
    );

    await userEvent.type(screen.getByLabelText("Email"), "admin@test.local");
    await userEvent.type(screen.getByLabelText("Password"), "Xk9!Zephyr*Batt3ry");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    await waitFor(() => expect(screen.getByText("Home Page")).toBeInTheDocument());
    expect(authApi.login).toHaveBeenCalledWith("admin@test.local", "Xk9!Zephyr*Batt3ry");
  });

  it("shows the server error message on failed login", async () => {
    vi.spyOn(authApi, "login").mockRejectedValue(new ApiError("Invalid email or password.", 401, null));

    render(
      <AuthProvider>
        <MemoryRouter initialEntries={["/login"]}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
          </Routes>
        </MemoryRouter>
      </AuthProvider>,
    );

    await userEvent.type(screen.getByLabelText("Email"), "admin@test.local");
    await userEvent.type(screen.getByLabelText("Password"), "wrong-password");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByText("Invalid email or password.")).toBeInTheDocument();
  });
});
