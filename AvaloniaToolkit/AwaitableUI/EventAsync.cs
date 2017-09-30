using System;
using System.Threading.Tasks;

namespace AvaloniaToolkit.AwaitableUI
{
    public static class EventAsync
    {
        /// <summary>
        /// Creates a <see cref="System.Threading.Tasks.Task"/>
        /// that waits for an event to occur.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// await EventAsync.FromEvent(
        ///     eh => storyboard.Completed += eh,
        ///     eh => storyboard.Completed -= eh,
        ///     storyboard.Begin);
        /// ]]>
        /// </example>
        /// <param name="addEventHandler">
        /// The action that subscribes to the event.
        /// </param>
        /// <param name="removeEventHandler">
        /// The action that unsubscribes from the event when it first occurs.
        /// </param>
        /// <param name="beginAction">
        /// The action to call after subscribing to the event.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/> that
        /// completes when the event registered in
        /// <paramref name="addEventHandler"/> occurs.
        /// </returns>
        public static Task<object> FromEvent<T>(
            Action<EventHandler<T>> addEventHandler,
            Action<EventHandler<T>> removeEventHandler,
            Action beginAction = null)
        {
            return new EventHandlerTaskSource<T>(
                addEventHandler,
                removeEventHandler,
                beginAction).Task;
        }

        private sealed class EventHandlerTaskSource<TEventArgs>
        {
            private readonly TaskCompletionSource<object> tcs;
            private readonly Action<EventHandler<TEventArgs>> removeEventHandler;

            public EventHandlerTaskSource(
                Action<EventHandler<TEventArgs>> addEventHandler,
                Action<EventHandler<TEventArgs>> removeEventHandler,
                Action beginAction = null)
            {
                if (addEventHandler == null)
                {
                    throw new ArgumentNullException(nameof(addEventHandler));
                }

                if (removeEventHandler == null)
                {
                    throw new ArgumentNullException(nameof(removeEventHandler));
                }

                this.tcs = new TaskCompletionSource<object>();
                this.removeEventHandler = removeEventHandler;
                addEventHandler.Invoke(EventCompleted);

                if (beginAction != null)
                {
                    beginAction.Invoke();
                }
            }

            /// <summary>
            /// Returns a task that waits for the event to occur.
            /// </summary>
            public Task<object> Task
            {
                get { return tcs.Task; }
            }

            private void EventCompleted(object sender, TEventArgs args)
            {
                this.removeEventHandler.Invoke(EventCompleted);
                this.tcs.SetResult(args);
            }
        }

    }
}