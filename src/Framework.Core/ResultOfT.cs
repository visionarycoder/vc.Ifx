namespace VisionaryCoder.Framework;

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? value;

    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing the value of a failed result.</exception>
    public TValue Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("Cannot access the value of a failed result");
            }

            return value!;
        }
    }

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        this.value = value;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public new static Result<TValue> Failure(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    public new static Result<TValue> Failure(string code, string message) =>
        new(default, false, new Error(code, message));

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
