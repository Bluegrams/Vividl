using System;
using System.Collections.Generic;
using System.Linq;
using Enterwell.Clients.Wpf.Notifications;

namespace Vividl.Services
{
    /// <summary>
    /// NotificationMessageManager with max number of notifications.
    /// Copied and adapted from NotificationMessageManager class.
    /// </summary>
    /// <seealso cref="INotificationMessageManager" />
    public class CappedNotificationMessageManager : INotificationMessageManager
    {
        private readonly List<INotificationMessage> queuedMessages = new List<INotificationMessage>();

        public event NotificationMessageManagerEventHandler OnMessageQueued;

        public event NotificationMessageManagerEventHandler OnMessageDismissed;

        public INotificationMessageFactory Factory { get; set; } = new NotificationMessageFactory();

        public int MaxNotifications { get; }

        public CappedNotificationMessageManager(int maxNotifications)
        {
            MaxNotifications = maxNotifications;
        }

        /// <summary>
        /// Queues the specified message.
        /// This will ignore the <c>null</c> message or already queued notification message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Queue(INotificationMessage message)
        {
            if (message == null || this.queuedMessages.Contains(message))
                return;

            this.queuedMessages.Add(message);

            this.TriggerMessageQueued(message);

            if (this.queuedMessages.Count > MaxNotifications)
            {
                this.Dismiss(this.queuedMessages.First());
            }
        }

        /// <summary>
        /// Triggers the message queued event.
        /// </summary>
        /// <param name="message">The message.</param>
        private void TriggerMessageQueued(INotificationMessage message)
        {
            this.OnMessageQueued?.Invoke(this, new NotificationMessageManagerEventArgs(message));
        }

        /// <summary>
        /// Dismisses the specified message.
        /// This will ignore the <c>null</c> or not queued notification message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Dismiss(INotificationMessage message)
        {
            if (message == null || !this.queuedMessages.Contains(message))
                return;

            this.queuedMessages.Remove(message);

            this.TriggerMessageDismissed(message);
        }

        /// <summary>
        /// Triggers the message dismissed event.
        /// </summary>
        /// <param name="message">The message.</param>
        private void TriggerMessageDismissed(INotificationMessage message)
        {
            this.OnMessageDismissed?.Invoke(this, new NotificationMessageManagerEventArgs(message));
        }
    }
}
