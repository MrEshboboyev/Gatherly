namespace Gatherly.Domain.Shared;

public interface IValidationResult
{
    public static readonly Error ValidationError = new(
        "ValidationError",
        "A vaidation problem occurred!");

    Error[] Errors { get; }
}
