import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { AppShell } from "./AppShell";

describe("AppShell", () => {
  it("renders the app title and its children", () => {
    render(
      <AppShell>
        <p>page content</p>
      </AppShell>,
    );

    expect(screen.getByText("DPDP-COMPASS")).toBeInTheDocument();
    expect(screen.getByText("page content")).toBeInTheDocument();
  });
});
