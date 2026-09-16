using System.Collections.Generic;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>What a leave notification is about, from the recipient's side.</summary>
    public enum VacationNotice
    {
        /// <summary>A request is waiting for the recipient's decision.</summary>
        Submitted,

        /// <summary>Somebody else changed a request the recipient is party to.</summary>
        Changed,

        /// <summary>The employee took their own request back.</summary>
        Withdrawn,

        /// <summary>The recipient's own request was decided.</summary>
        Decided,

        /// <summary>An administrator decided a request the recipient was to decide.</summary>
        DecidedByAdmin
    }

    public class VacationRecipient
    {
        public VacationRecipient(string userId, VacationNotice notice)
        {
            UserId = userId;
            Notice = notice;
        }

        public string UserId { get; }

        public VacationNotice Notice { get; }
    }

    /// <summary>
    /// Who hears about a change to a leave request. Kept apart from the sending
    /// so the rule can be read and tested on its own: nobody is told about
    /// something they did themselves, and a manager hears about a decision only
    /// when it was taken over their head.
    /// </summary>
    public static class VacationNotificationRecipients
    {
        public static IList<VacationRecipient> ForSubmitted(string managerId, string actorId)
        {
            return One(managerId, actorId, VacationNotice.Submitted);
        }

        /// <summary>
        /// The employee's own edit sends the request back for approval, so the
        /// manager hears about it; anybody else's edit is news to the employee.
        /// </summary>
        public static IList<VacationRecipient> ForChanged(string employeeId, string managerId, string actorId)
        {
            return actorId == employeeId
                ? One(managerId, actorId, VacationNotice.Changed)
                : One(employeeId, actorId, VacationNotice.Changed);
        }

        /// <summary>Only the owner can cancel, so this is always the employee's doing.</summary>
        public static IList<VacationRecipient> ForWithdrawn(string managerId, string actorId)
        {
            return One(managerId, actorId, VacationNotice.Withdrawn);
        }

        /// <summary>
        /// The employee always hears the outcome. The manager hears only when
        /// somebody else decided it — their own decision is not news to them.
        /// </summary>
        public static IList<VacationRecipient> ForDecided(string employeeId, string managerId, string actorId)
        {
            var recipients = new List<VacationRecipient>();

            Add(recipients, employeeId, actorId, VacationNotice.Decided);
            Add(recipients, managerId, actorId, VacationNotice.DecidedByAdmin);

            return recipients;
        }

        private static IList<VacationRecipient> One(string userId, string actorId, VacationNotice notice)
        {
            var recipients = new List<VacationRecipient>();
            Add(recipients, userId, actorId, notice);
            return recipients;
        }

        /// <summary>
        /// Drops the actor, an employee with no manager on file, and anyone
        /// already on the list — an employee who manages themselves is told once.
        /// </summary>
        private static void Add(IList<VacationRecipient> recipients, string userId, string actorId, VacationNotice notice)
        {
            if (string.IsNullOrEmpty(userId) || userId == actorId)
            {
                return;
            }

            foreach (var recipient in recipients)
            {
                if (recipient.UserId == userId)
                {
                    return;
                }
            }

            recipients.Add(new VacationRecipient(userId, notice));
        }
    }
}
