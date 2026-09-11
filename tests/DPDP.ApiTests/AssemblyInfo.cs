using Xunit;

// WebApplicationFactory<Program> relies on a process-wide static hook to
// intercept host startup. Running multiple factories concurrently across
// test classes in this assembly is flaky (intermittent
// "entry point exited without ever building an IHost" failures) — disable
// cross-class parallelization for this assembly only.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
