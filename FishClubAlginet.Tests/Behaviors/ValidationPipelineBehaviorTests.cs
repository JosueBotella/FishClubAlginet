using FluentAssertions;
using FluentValidation;
using MediatR;
using FishClubAlginet.Application.Behaviors;
// Moq no se usa en este archivo: IValidator<T> no es mockeable cuando T es un tipo privado anidado.

namespace FishClubAlginet.Tests.Behaviors;

public class ValidationPipelineBehaviorTests
{
    // ── minimal fixtures ──────────────────────────────────────────────────────

    private record TestCommand(string Name) : IRequest<ErrorOr<string>>;

    private class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode("Test.Name.Required")
                .WithMessage("Name is required.");
        }
    }

    /// <summary>
    /// Segundo validator real usado en el test de agregación de errores.
    /// MinimumLength(5) falla en string.Empty (longitud 0 < 5), igual que NotEmpty,
    /// garantizando que ambos validators disparen con el mismo input vacío.
    /// MaximumLength no sirve aquí: solo falla cuando el string supera el límite, no cuando es vacío.
    /// Evita Moq sobre tipos privados anidados (Castle DynamicProxy requiere accesibilidad pública).
    /// </summary>
    private class TestCommandMinLengthValidator : AbstractValidator<TestCommand>
    {
        public TestCommandMinLengthValidator()
        {
            RuleFor(x => x.Name)
                .MinimumLength(5)
                .WithErrorCode("Test.Name.TooShort")
                .WithMessage("Name must be at least 5 characters.");
        }
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static ValidationPipelineBehavior<TestCommand, ErrorOr<string>> BuildBehavior(
        params IValidator<TestCommand>[] validators)
        => new(validators);

    private static RequestHandlerDelegate<ErrorOr<string>> NextReturning(string value)
        => _ => Task.FromResult<ErrorOr<string>>(value);

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoValidators_ShouldCallNextAndReturnItsResult()
    {
        // Arrange
        var behavior = BuildBehavior(); // zero validators
        var command  = new TestCommand("anything");

        // Act
        var result = await behavior.Handle(command, NextReturning("ok"), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallNextAndReturnItsResult()
    {
        // Arrange
        var behavior = BuildBehavior(new TestCommandValidator());
        var command  = new TestCommand("Josué");

        // Act
        var result = await behavior.Handle(command, NextReturning("success"), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("success");
    }

    [Fact]
    public async Task Handle_InvalidCommand_ShouldReturnValidationErrorsWithoutCallingNext()
    {
        // Arrange
        var behavior      = BuildBehavior(new TestCommandValidator());
        var command       = new TestCommand(string.Empty); // fails NotEmpty
        var nextWasCalled = false;

        RequestHandlerDelegate<ErrorOr<string>> next = _ =>
        {
            nextWasCalled = true;
            return Task.FromResult<ErrorOr<string>>("should not reach here");
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Test.Name.Required");
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MultipleValidators_ShouldAggregateAllErrors()
    {
        // Arrange: two real validators with non-overlapping rules over the same command.
        // Moq no puede generar proxy de IValidator<T> cuando T es un tipo privado anidado
        // (Castle DynamicProxy requiere accesibilidad pública). Se usan validators reales.
        var validator1 = new TestCommandValidator();       // falla NotEmpty    → Test.Name.Required
        var validator2 = new TestCommandMinLengthValidator(); // falla MinLength(5) → Test.Name.TooShort
        // Ambas fallan con string.Empty: NotEmpty (longitud 0) y MinLength(5) (0 < 5).

        var behavior = BuildBehavior(validator1, validator2);
        var command  = new TestCommand(string.Empty);

        // Act
        var result = await behavior.Handle(command, NextReturning("x"), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCountGreaterThan(1);
        result.Errors.Should().Contain(e => e.Code == "Test.Name.Required");
        result.Errors.Should().Contain(e => e.Code == "Test.Name.TooShort");
    }

    [Fact]
    public async Task Handle_CancellationTokenPropagated_WhenNextIsCalled()
    {
        // Arrange
        var behavior = BuildBehavior(new TestCommandValidator());
        var command  = new TestCommand("valid");
        var cts      = new CancellationTokenSource();
        CancellationToken capturedToken = default;

        RequestHandlerDelegate<ErrorOr<string>> next = ct =>
        {
            capturedToken = ct;
            return Task.FromResult<ErrorOr<string>>("ok");
        };

        // Act
        await behavior.Handle(command, next, cts.Token);

        // Assert
        capturedToken.Should().Be(cts.Token);
    }
}
