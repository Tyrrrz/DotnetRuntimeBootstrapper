using System;

namespace DotnetRuntimeBootstrapper.Utils
{
    // Basic implementation of a promise
    internal partial class Task
    {
        internal TaskCompletionSource Source { get; }

        public Task(TaskCompletionSource source) => Source = source;

        public Task Then(Action next)
        {
            return Create(composedSource =>
            {
                Source.OnCompleted(outcome =>
                {
                    try
                    {
                        if (outcome.IsFailed)
                        {
                            composedSource.SetOutcome(outcome);
                        }
                        else
                        {
                            next();
                            composedSource.SetSuccessful();
                        }
                    }
                    catch (Exception ex)
                    {
                        composedSource.SetFailed(ex);
                    }
                });
            });
        }

        public Task Then(Func<Task> getNext)
        {
            return Create(composedSource =>
            {
                Source.OnCompleted(outcome =>
                {
                    try
                    {
                        if (outcome.IsFailed)
                        {
                            composedSource.SetOutcome(outcome);
                        }
                        else
                        {
                            var next = getNext();
                            next.Source.OnCompleted(composedSource.SetOutcome);
                        }
                    }
                    catch (Exception ex)
                    {
                        composedSource.SetFailed(ex);
                    }
                });
            });
        }

        public Task Catch(Action<Exception> handleException)
        {
            return Create(composedSource =>
            {
                Source.OnCompleted(outcome =>
                {
                    try
                    {
                        if (outcome.IsFailed)
                        {
                            handleException(outcome.Exception);
                            composedSource.SetSuccessful();
                        }
                        else
                        {
                            composedSource.SetOutcome(outcome);
                        }
                    }
                    catch (Exception ex)
                    {
                        composedSource.SetFailed(ex);
                    }
                });
            });
        }
    }

    internal partial class Task
    {
        public static Task Create(Action<TaskCompletionSource> register)
        {
            var source = new TaskCompletionSource();
            register(source);
            return new Task(source);
        }

        public static Task Successful { get; } = Create(source => source.SetSuccessful());
    }
}