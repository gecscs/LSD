// <copyright file="ValueBindingHelper.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LSD_Layered_Selection_Display.Extensions
{
    using System;
    using Colossal.UI.Binding;

    /// <summary>
    /// A helper class for managing value bindings with optional update callbacks.
    /// </summary>
    /// <typeparam name="T">The type of the value to bind.</typeparam>
    public class ValueBindingHelper<T>
    {
        private readonly Action<T> _updateCallBack;

        /// <summary>
        /// A helper class for managing value bindings with optional update callbacks.
        /// </summary>
        /// <param name="binding">The value binding to manage.</param>
        public ValueBinding<T> Binding { get; }

        /// <summary>
        /// Gets or sets the value of the binding. Setting the value will update the binding and invoke the optional update callback if provided.
        /// </summary>
        public T Value { get => Binding.value; set => Binding.Update(value); }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBindingHelper{T}"/> class with a specified binding and an optional update callback.
        /// </summary>
        /// <param name="binding">The value binding to manage.</param>
        /// <param name="updateCallBack">An optional callback to invoke when the value is updated.</param>
        public ValueBindingHelper(ValueBinding<T> binding, Action<T> updateCallBack = null)
        {
            Binding = binding;
            _updateCallBack = updateCallBack;
        }

        /// <summary>
        /// Updates the value of the binding and invokes the optional update callback if provided.
        /// </summary>
        /// <param name="value">The new value to set for the binding.</param>
        public void UpdateCallback(T value)
        {
            Binding.Update(value);
            _updateCallBack?.Invoke(value);
        }

        /// <summary>
        /// Defines an implicit conversion from ValueBindingHelper.<T> to T, allowing the helper to be used directly as the underlying value type.
        /// </summary>
        /// <param name="helper">The value binding helper to convert.</param>
        /// <returns>The value of the binding managed by the helper.</returns>
        public static implicit operator T(ValueBindingHelper<T> helper)
        {
            return helper.Binding.value;
        }
    }
}
