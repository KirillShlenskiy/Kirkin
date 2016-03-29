using System;

using Kirkin.ChangeTracking;
using Kirkin.Reflection;

namespace Kirkin.Transactions
{
    /// <summary>
    /// Reflection-based transaction implementation which
    /// creates a snapshot of the object's properties at
    /// initialisation, and rolls back any changes when
    /// disposed, unless preceded by a call to Commit.
    /// This type is not thread-safe.
    /// </summary>
    public sealed class PropertyTrackingTransaction<T> : PropertyTrackingTransaction
        where T : class
    {
        /// <summary>
        /// Property change tracker used for this transaction.
        /// </summary>
        public ChangeTracker<T> ChangeTracker { get; }

        /// <summary>
        /// Creates a new transaction which detects changes in all 
        /// properties of the given object which have a public getter.
        /// </summary>
        public PropertyTrackingTransaction(T trackedObject)
            : this(trackedObject, PropertyList<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new transaction which detects changes in
        /// any of the specified properties of the given object.
        /// </summary>
        public PropertyTrackingTransaction(T trackedObject, PropertyList<T> propertyList)
        {
            if (trackedObject == null) throw new ArgumentNullException(nameof(trackedObject));
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            ChangeTracker = new ChangeTracker<T>(trackedObject, propertyList);
        }

        /// <summary>
        /// Rolls back the changes made since this transaction was initialised.
        /// </summary>
        protected override void Rollback()
        {
            PropertyValueSnapshot<T> snapshot = ChangeTracker.Snapshot;

            snapshot.Apply(snapshot.Target);
        }
    }
}