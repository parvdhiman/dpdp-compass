using DPDP.Application.Common.Behaviors;
using FluentValidation;
using Xunit;

namespace DPDP.UnitTests.Application;

public class ValidationBehaviorTests
{
    private sealed record SampleRequest(string Name);

    [Fact]
    public async Task Handle_throws_ValidationException_when_a_validator_fails()
    {
        var validator = new InlineValidator<SampleRequest>();
        validator.RuleFor(r => r.Name).NotEmpty();

        var behavior = new ValidationBehavior<SampleRequest, string>([validator]);

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new SampleRequest(string.Empty),
                (_) => Task.FromResult("handled"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_calls_next_when_validation_passes()
    {
        var validator = new InlineValidator<SampleRequest>();
        validator.RuleFor(r => r.Name).NotEmpty();

        var behavior = new ValidationBehavior<SampleRequest, string>([validator]);

        var result = await behavior.Handle(
            new SampleRequest("valid"),
            (_) => Task.FromResult("handled"),
            CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_calls_next_when_there_are_no_validators()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([]);

        var result = await behavior.Handle(
            new SampleRequest(string.Empty),
            (_) => Task.FromResult("handled"),
            CancellationToken.None);

        Assert.Equal("handled", result);
    }
}
