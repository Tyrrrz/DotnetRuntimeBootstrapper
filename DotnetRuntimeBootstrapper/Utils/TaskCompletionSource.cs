using System;

namespace DotnetRuntimeBootstrapper.Utils
{
    internal class TaskCompletionOutcome
    {
        public Exception Exception { get; }

        public bool IsSuccessful => Exception == null;

        public bool IsFailed => !IsSuccessful;

        public TaskCompletionOutcome(Exception exception) => Exception = exception;
    }

    internal class TaskCompletionSource
    {
        private readonly object _lock = new object();
        private Action<TaskCompletionOutcome> _onCompleted;

        public TaskCompletionOutcome Outcome { get; private set; }

        public bool IsCompleted => Outcome != null;

        public void OnCompleted(Action<TaskCompletionOutcome> handleOutcome)
        {
            lock (_lock)
            {
                if (IsCompleted)
                {
                    handleOutcome(Outcome);
                }
                else
                {
                    _onCompleted += handleOutcome;
                }
            }
        }

        public void SetOutcome(TaskCompletionOutcome outcome)
        {
            lock (_lock)
            {
                if (IsCompleted)
                    return;

                Outcome = outcome;
                _onCompleted?.Invoke(outcome);
            }
        }

        public void SetSuccessful() => SetOutcome(new TaskCompletionOutcome(null));

        public void SetFailed(Exception exception) => SetOutcome(new TaskCompletionOutcome(exception));
    }
}