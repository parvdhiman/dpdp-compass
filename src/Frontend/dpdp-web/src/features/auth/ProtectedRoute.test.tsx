import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it } from "vitest";
import { AuthProvider } from "./AuthProvider";
import { ProtectedRoute } from "./ProtectedRoute";

describe("ProtectedRoute", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("redirects to /login when no session exists", async () => {
    render(
      <AuthProvider>
        <MemoryRouter initialEntries={["/secret"]}>
          <Routes>
            <Route path="/login" element={<div>Login Page</div>} />
            <Route
              path="/secret"
              element={
                <ProtectedRoute>
                  <div>Secret content</div>
                </ProtectedRoute>
              }
            />
          </Routes>
        </MemoryRouter>
      </AuthProvider>,
    );

    expect(await screen.findByText("Login Page")).toBeInTheDocument();
    expect(screen.queryByText("Secret content")).not.toBeInTheDocument();
  });
});
