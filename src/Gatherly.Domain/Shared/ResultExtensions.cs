namespace Gatherly.Domain.Shared;

/// <summary>
/// Extensions for the Result pattern to enable fluent, LINQ-style operations.
/// </summary>
public static class ResultExtensions
{
    #region Bind / Map Operations

    /// <summary>
    /// Maps a Result to a new Result of a different type.
    /// </summary>
    public static Result<TResult> Map<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> mapper)
    {
        return result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Asynchronously maps a Result to a new Result of a different type.
    /// </summary>
    public static async Task<Result<TResult>> MapAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<TResult>> mapper)
    {
        return result.IsSuccess
            ? Result.Success(await mapper(result.Value))
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Asynchronously maps a Task<Result> to a new Result of a different type.
    /// </summary>
    public static async Task<Result<TResult>> MapAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TResult> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Asynchronously maps a Task<Result> to a new Result of a different type using an async mapper.
    /// </summary>
    public static async Task<Result<TResult>> MapAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<TResult>> mapper)
    {
        var result = await resultTask;
        return await result.MapAsync(mapper);
    }

    /// <summary>
    /// Binds a Result to another Result-returning function.
    /// </summary>
    public static Result<TResult> Bind<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Result<TResult>> binder)
    {
        return result.IsSuccess
            ? binder(result.Value)
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Asynchronously binds a Result to another Result-returning function.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<Result<TResult>>> binder)
    {
        return result.IsSuccess
            ? await binder(result.Value)
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Asynchronously binds a Task<Result> to a Result-returning function.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Result<TResult>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    /// <summary>
    /// Asynchronously binds a Task<Result> to an async Result-returning function.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<Result<TResult>>> binder)
    {
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

    #endregion

    #region LINQ Query Syntax Support

    /// <summary>
    /// LINQ query syntax support for Select operations.
    /// </summary>
    public static Result<TResult> Select<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> selector)
    {
        return result.Map(selector);
    }

    /// <summary>
    /// LINQ query syntax support for Select operations on Task<Result>.
    /// </summary>
    public static Task<Result<TResult>> Select<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TResult> selector)
    {
        return resultTask.MapAsync(selector);
    }

    /// <summary>
    /// LINQ query syntax support for SelectMany (monadic bind) operations.
    /// </summary>
    public static Result<TResult> SelectMany<TValue, TIntermediate, TResult>(
        this Result<TValue> result,
        Func<TValue, Result<TIntermediate>> intermediateSelector,
        Func<TValue, TIntermediate, TResult> resultSelector)
    {
        return result.IsSuccess
            ? intermediateSelector(result.Value).IsSuccess
                ? Result.Success(resultSelector(result.Value, intermediateSelector(result.Value).Value))
                : Result.Failure<TResult>(intermediateSelector(result.Value).Error)
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// LINQ query syntax support for SelectMany with Task<Result>.
    /// </summary>
    public static async Task<Result<TResult>> SelectMany<TValue, TIntermediate, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Result<TIntermediate>> intermediateSelector,
        Func<TValue, TIntermediate, TResult> resultSelector)
    {
        var result = await resultTask;
        return result.SelectMany(intermediateSelector, resultSelector);
    }

    /// <summary>
    /// LINQ query syntax support for SelectMany with Result and async selector.
    /// </summary>
    public static async Task<Result<TResult>> SelectMany<TValue, TIntermediate, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<Result<TIntermediate>>> intermediateSelector,
        Func<TValue, TIntermediate, TResult> resultSelector)
    {
        if (!result.IsSuccess)
            return Result.Failure<TResult>(result.Error);

        var intermediateResult = await intermediateSelector(result.Value);
        if (!intermediateResult.IsSuccess)
            return Result.Failure<TResult>(intermediateResult.Error);

        return Result.Success(resultSelector(result.Value, intermediateResult.Value));
    }

    /// <summary>
    /// LINQ query syntax support for SelectMany with Task<Result> and async selector.
    /// </summary>
    public static async Task<Result<TResult>> SelectMany<TValue, TIntermediate, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<Result<TIntermediate>>> intermediateSelector,
        Func<TValue, TIntermediate, TResult> resultSelector)
    {
        var result = await resultTask;
        return await result.SelectMany(intermediateSelector, resultSelector);
    }

    /// <summary>
    /// Direct binding for Result<T> to Result<TResult>.
    /// </summary>
    public static Result<TResult> SelectMany<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Result<TResult>> selector)
    {
        return result.Bind(selector);
    }

    /// <summary>
    /// Direct binding for Result<T> to Task<Result<TResult>>.
    /// </summary>
    public static Task<Result<TResult>> SelectMany<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<Result<TResult>>> selector)
    {
        return result.BindAsync(selector);
    }

    /// <summary>
    /// Direct binding for Task<Result<T>> to Result<TResult>.
    /// </summary>
    public static Task<Result<TResult>> SelectMany<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Result<TResult>> selector)
    {
        return resultTask.BindAsync(selector);
    }

    /// <summary>
    /// Direct binding for Task<Result<T>> to Task<Result<TResult>>.
    /// </summary>
    public static Task<Result<TResult>> SelectMany<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<Result<TResult>>> selector)
    {
        return resultTask.BindAsync(selector);
    }

    #endregion

    #region Matching and Pattern Matching

    /// <summary>
    /// Matches the result to different functions depending on success or failure.
    /// </summary>
    public static TResult Match<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Matches the result to different async functions depending on success or failure.
    /// </summary>
    public static Task<TResult> MatchAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Matches the result to different functions depending on success or failure, with async failure handler.
    /// </summary>
    public static Task<TResult> MatchAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> onSuccess,
        Func<Error, Task<TResult>> onFailure)
    {
        return result.IsSuccess
            ? Task.FromResult(onSuccess(result.Value))
            : onFailure(result.Error);
    }

    /// <summary>
    /// Matches the result to different functions depending on success or failure, with async success handler.
    /// </summary>
    public static Task<TResult> MatchAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<TResult>> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : Task.FromResult(onFailure(result.Error));
    }

    /// <summary>
    /// Matches the result to different functions depending on success or failure.
    /// </summary>
    public static async Task<TResult> MatchAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Matches the result to different async functions depending on success or failure.
    /// </summary>
    public static async Task<TResult> MatchAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure)
    {
        var result = await resultTask;
        return await result.MatchAsync(onSuccess, onFailure);
    }

    #endregion

    #region Ensure (Validation) Operations

    /// <summary>
    /// Ensures that a predicate is satisfied, otherwise returns a failure.
    /// </summary>
    public static Result<TValue> Ensure<TValue>(
        this Result<TValue> result,
        Func<TValue, bool> predicate,
        Error error)
    {
        if (!result.IsSuccess)
            return result;

        return predicate(result.Value)
            ? result
            : Result.Failure<TValue>(error);
    }

    /// <summary>
    /// Ensures that an async predicate is satisfied, otherwise returns a failure.
    /// </summary>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, Task<bool>> predicate,
        Error error)
    {
        if (!result.IsSuccess)
            return result;

        return await predicate(result.Value)
            ? result
            : Result.Failure<TValue>(error);
    }

    /// <summary>
    /// Ensures that a predicate is satisfied for a Task<Result>, otherwise returns a failure.
    /// </summary>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, bool> predicate,
        Error error)
    {
        var result = await resultTask;
        return result.Ensure(predicate, error);
    }

    /// <summary>
    /// Ensures that an async predicate is satisfied for a Task<Result>, otherwise returns a failure.
    /// </summary>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<bool>> predicate,
        Error error)
    {
        var result = await resultTask;
        return await result.EnsureAsync(predicate, error);
    }

    #endregion

    #region Tapping Operations (Side Effects)

    /// <summary>
    /// Executes an action on the value if the result is successful, and returns the original result.
    /// </summary>
    public static Result<TValue> Tap<TValue>(
        this Result<TValue> result,
        Action<TValue> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    /// <summary>
    /// Executes an async action on the value if the result is successful, and returns the original result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }

    /// <summary>
    /// Executes an action on the value if the result is successful, and returns the original result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Action<TValue> action)
    {
        var result = await resultTask;
        return result.Tap(action);
    }

    /// <summary>
    /// Executes an async action on the value if the result is successful, and returns the original result.
    /// </summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task> action)
    {
        var result = await resultTask;
        return await result.TapAsync(action);
    }

    /// <summary>
    /// Executes an action on the error if the result is a failure, and returns the original result.
    /// </summary>
    public static Result<TValue> TapError<TValue>(
        this Result<TValue> result,
        Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    /// <summary>
    /// Executes an async action on the error if the result is a failure, and returns the original result.
    /// </summary>
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(
        this Result<TValue> result,
        Func<Error, Task> action)
    {
        if (result.IsFailure)
            await action(result.Error);

        return result;
    }

    /// <summary>
    /// Executes an action on the error if the result is a failure, and returns the original result.
    /// </summary>
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Action<Error> action)
    {
        var result = await resultTask;
        return result.TapError(action);
    }

    /// <summary>
    /// Executes an async action on the error if the result is a failure, and returns the original result.
    /// </summary>
    public static async Task<Result<TValue>> TapErrorAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<Error, Task> action)
    {
        var result = await resultTask;
        return await result.TapErrorAsync(action);
    }

    #endregion

    #region Conversion Operations

    /// <summary>
    /// Converts a Result to a Result of a different type.
    /// </summary>
    public static Result<TResult> To<TValue, TResult>(this Result<TValue> result, TResult value)
    {
        return result.IsSuccess
            ? Result.Success(value)
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Converts a Result to a Result of a different type using a converter function.
    /// </summary>
    public static Result<TResult> To<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> converter)
    {
        return result.Map(converter);
    }

    /// <summary>
    /// Converts a Task<Result> to a Task<Result> of a different type.
    /// </summary>
    public static async Task<Result<TResult>> ToAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        TResult value)
    {
        var result = await resultTask;
        return result.To(value);
    }

    /// <summary>
    /// Converts a Task<Result> to a Task<Result> of a different type using a converter function.
    /// </summary>
    public static async Task<Result<TResult>> ToAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TResult> converter)
    {
        var result = await resultTask;
        return result.To(converter);
    }

    #endregion

    #region Combining Results

    /// <summary>
    /// Combines two Results and applies a function to their values if both are successful.
    /// </summary>
    public static Result<TResult> Combine<T1, T2, TResult>(
        this Result<T1> result1,
        Result<T2> result2,
        Func<T1, T2, TResult> resultSelector)
    {
        return result1.IsSuccess && result2.IsSuccess
            ? Result.Success(resultSelector(result1.Value, result2.Value))
            : result1.IsFailure
                ? Result.Failure<TResult>(result1.Error)
                : Result.Failure<TResult>(result2.Error);
    }

    /// <summary>
    /// Combines three Results and applies a function to their values if all are successful.
    /// </summary>
    public static Result<TResult> Combine<T1, T2, T3, TResult>(
        this Result<T1> result1,
        Result<T2> result2,
        Result<T3> result3,
        Func<T1, T2, T3, TResult> resultSelector)
    {
        if (result1.IsFailure) return Result.Failure<TResult>(result1.Error);
        if (result2.IsFailure) return Result.Failure<TResult>(result2.Error);
        if (result3.IsFailure) return Result.Failure<TResult>(result3.Error);

        return Result.Success(resultSelector(result1.Value, result2.Value, result3.Value));
    }

    /// <summary>
    /// Combines four Results and applies a function to their values if all are successful.
    /// </summary>
    public static Result<TResult> Combine<T1, T2, T3, T4, TResult>(
        this Result<T1> result1,
        Result<T2> result2,
        Result<T3> result3,
        Result<T4> result4,
        Func<T1, T2, T3, T4, TResult> resultSelector)
    {
        if (result1.IsFailure) return Result.Failure<TResult>(result1.Error);
        if (result2.IsFailure) return Result.Failure<TResult>(result2.Error);
        if (result3.IsFailure) return Result.Failure<TResult>(result3.Error);
        if (result4.IsFailure) return Result.Failure<TResult>(result4.Error);

        return Result.Success(resultSelector(result1.Value, result2.Value, result3.Value, result4.Value));
    }

    /// <summary>
    /// Asynchronously combines two Results and applies a function to their values if both are successful.
    /// </summary>
    public static async Task<Result<TResult>> CombineAsync<T1, T2, TResult>(
        this Task<Result<T1>> resultTask1,
        Task<Result<T2>> resultTask2,
        Func<T1, T2, TResult> resultSelector)
    {
        var result1 = await resultTask1;
        var result2 = await resultTask2;

        return result1.Combine(result2, resultSelector);
    }

    /// <summary>
    /// Asynchronously combines three Results and applies a function to their values if all are successful.
    /// </summary>
    public static async Task<Result<TResult>> CombineAsync<T1, T2, T3, TResult>(
        this Task<Result<T1>> resultTask1,
        Task<Result<T2>> resultTask2,
        Task<Result<T3>> resultTask3,
        Func<T1, T2, T3, TResult> resultSelector)
    {
        var result1 = await resultTask1;
        var result2 = await resultTask2;
        var result3 = await resultTask3;

        return result1.Combine(result2, result3, resultSelector);
    }

    #endregion

    #region Recovery Operations

    /// <summary>
    /// Recovers from a failure by providing a fallback value.
    /// </summary>
    public static Result<TValue> Recover<TValue>(
        this Result<TValue> result,
        TValue fallbackValue)
    {
        return result.IsSuccess
            ? result
            : Result.Success(fallbackValue);
    }

    /// <summary>
    /// Recovers from a failure by providing a fallback function.
    /// </summary>
    public static Result<TValue> Recover<TValue>(
        this Result<TValue> result,
        Func<Error, TValue> fallbackFunc)
    {
        return result.IsSuccess
            ? result
            : Result.Success(fallbackFunc(result.Error));
    }

    /// <summary>
    /// Recovers from a failure by providing a fallback Result.
    /// </summary>
    public static Result<TValue> Recover<TValue>(
        this Result<TValue> result,
        Func<Error, Result<TValue>> fallbackFunc)
    {
        return result.IsSuccess
            ? result
            : fallbackFunc(result.Error);
    }

    /// <summary>
    /// Asynchronously recovers from a failure by providing a fallback function.
    /// </summary>
    public static async Task<Result<TValue>> RecoverAsync<TValue>(
        this Result<TValue> result,
        Func<Error, Task<TValue>> fallbackFunc)
    {
        return result.IsSuccess
            ? result
            : Result.Success(await fallbackFunc(result.Error));
    }

    /// <summary>
    /// Asynchronously recovers from a failure by providing a fallback Result.
    /// </summary>
    public static async Task<Result<TValue>> RecoverAsync<TValue>(
        this Result<TValue> result,
        Func<Error, Task<Result<TValue>>> fallbackFunc)
    {
        return result.IsSuccess
            ? result
            : await fallbackFunc(result.Error);
    }

    /// <summary>
    /// Asynchronously recovers from a failure by providing a fallback value.
    /// </summary>
    public static async Task<Result<TValue>> RecoverAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        TValue fallbackValue)
    {
        var result = await resultTask;
        return result.Recover(fallbackValue);
    }

    /// <summary>
    /// Asynchronously recovers from a failure by providing a fallback function.
    /// </summary>
    public static async Task<Result<TValue>> RecoverAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<Error, TValue> fallbackFunc)
    {
        var result = await resultTask;
        return result.Recover(fallbackFunc);
    }

    /// <summary>
    /// Asynchronously recovers from a failure by providing a fallback Result.
    /// </summary>
    public static async Task<Result<TValue>> RecoverAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<Error, Result<TValue>> fallbackFunc)
    {
        var result = await resultTask;
        return result.Recover(fallbackFunc);
    }

    /// <summary>
    /// Asynchronously recovers from a failure by providing an async fallback function.
    /// </summary>
    public static async Task<Result<TValue>> RecoverAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<Error, Task<TValue>> fallbackFunc)
    {
        var result = await resultTask;
        return await result.RecoverAsync(fallbackFunc);
    }

    /// <summary>
    /// Asynchronously recovers from a failure by providing an async fallback Result.
    /// </summary>
    public static async Task<Result<TValue>> RecoverAsync<TValue>(
        this Task<Result<TValue>> resultTask,
        Func<Error, Task<Result<TValue>>> fallbackFunc)
    {
        var result = await resultTask;
        return await result.RecoverAsync(fallbackFunc);
    }

    #endregion
}
