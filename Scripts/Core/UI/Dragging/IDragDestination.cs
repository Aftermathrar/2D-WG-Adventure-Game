using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Core.UI.Dragging
{
    /// <summary>
    /// Components that implement this interfaces can act as the destination for
    /// dragging a `DragItem`.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDragDestination<T> where T : class
    {
        /// <summary>
        /// How many of the given item can be accepted.
        /// </summary>
        /// <param name="item">The item type to potentially be added.</param>
        /// <returns>If there is no limit Int.MaxValue should be returned.</returns>
        int MaxAcceptable(T item);

        /// <summary>
        /// Whether the target inventory has a slot the item can be stacked into.
        /// </summary>
        /// <param name="item">The item type to look for a stack.</param>
        /// <returns>If the item is stackable and is in the inventory, returns true.</returns>
        bool HasStack(T item);

        /// <summary>
        /// Update the UI and any data to reflect adding the item to this destination.
        /// </summary>
        /// <param name="item">The item type.</param>
        /// <param name="number">The quantity of items.</param>
        /// <param name="state">The modifier object for the item instance</param>
        void AddItems(T item, int number, object state);

        /// <summary>
        /// Get any modifier fields unique to the item instance.
        /// </summary>
        object GetModifiers();
    }
}