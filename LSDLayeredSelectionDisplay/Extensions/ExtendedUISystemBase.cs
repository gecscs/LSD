// <copyright file="ExtendedUISystemBase.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LSD_Layered_Selection_Display.Extensions
{
    using System;
    using Colossal.UI.Binding;
    using Game.UI;

    /// <summary>
    /// An abstract base class for creating extended UI systems with value and trigger bindings.
    /// </summary>
    public abstract partial class ExtendedUISystemBase : UISystemBase
    {
        /// <summary>
        /// Creates a value binding helper for a specified key and initial value.
        /// </summary>
        /// <typeparam name="T">The type of the value to bind.</typeparam>
        /// <param name="key">The key for the binding.</param>
        /// <param name="initialValue">The initial value for the binding.</param>
        /// <returns>A value binding helper for the specified key and initial value.</returns>
        public ValueBindingHelper<T> CreateBinding<T>(string key, T initialValue)
        {
            var helper = new ValueBindingHelper<T>(new (LSDLayeredSelectionDisplayMod.Id, key, initialValue, new GenericUIWriter<T>()));

            AddBinding(helper.Binding);

            return helper;
        }

        /// <summary>
        /// Creates a value binding helper for a specified key, setter key, initial value, and an optional update callback.
        /// </summary>
        /// <typeparam name="T">The type of the value to bind.</typeparam>
        /// <param name="key">The key for the binding.</param>
        /// <param name="setterKey">The key for the setter binding.</param>
        /// <param name="initialValue">The initial value for the binding.</param>
        /// <param name="updateCallBack">An optional callback to invoke when the value is updated.</param>
        /// <returns>A value binding helper for the specified key, setter key, initial value, and update callback.</returns>
        public ValueBindingHelper<T> CreateBinding<T>(string key, string setterKey, T initialValue, Action<T> updateCallBack = null)
        {
            var helper = new ValueBindingHelper<T>(new (LSDLayeredSelectionDisplayMod.Id, key, initialValue, new GenericUIWriter<T>()), updateCallBack);
            var trigger = new TriggerBinding<T>(LSDLayeredSelectionDisplayMod.Id, setterKey, helper.UpdateCallback, initialValue is Enum ? new EnumReader<T>() : null);

            AddBinding(helper.Binding);
            AddBinding(trigger);

            return helper;
        }

        /// <summary>
        /// Creates a getter value binding for a specified key and getter function.
        /// </summary>
        /// <typeparam name="T">The type of the value to bind.</typeparam>
        /// <param name="key">The key for the binding.</param>
        /// <param name="getterFunc">The function to get the value.</param>
        /// <returns>A getter value binding for the specified key and getter function.</returns>
        public GetterValueBinding<T> CreateBinding<T>(string key, Func<T> getterFunc)
        {
            var binding = new GetterValueBinding<T>(LSDLayeredSelectionDisplayMod.Id, key, getterFunc, new GenericUIWriter<T>());

            AddBinding(binding);

            return binding;
        }

        /// <summary>
        /// Creates a trigger binding for a specified key and action.
        /// </summary>
        /// <param name="key">The key for the binding.</param>
        /// <param name="action">The action to invoke when the trigger is activated.</param>
        /// <returns>A trigger binding for the specified key and action.</returns>
        public TriggerBinding CreateTrigger(string key, Action action)
        {
            var binding = new TriggerBinding(LSDLayeredSelectionDisplayMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }

        /// <summary>
        /// Creates a trigger binding for a specified key and action with one parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the parameter for the action.</typeparam>
        /// <param name="key">The key for the binding.</param>
        /// <param name="action">The action to invoke when the trigger is activated.</param>
        /// <returns>A trigger binding for the specified key and action with one parameter.</returns>
        public TriggerBinding<T1> CreateTrigger<T1>(string key, Action<T1> action)
        {
            var binding = new TriggerBinding<T1>(LSDLayeredSelectionDisplayMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }

    /// <summary>
    /// Creates a trigger binding for a specified key and action with two parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter for the action.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the action.</typeparam>
    /// <param name="key">The key for the binding.</param>
    /// <param name="action">The action to invoke when the trigger is activated.</param>
    /// <returns>A trigger binding for the specified key and action with two parameters.</returns>
        public TriggerBinding<T1, T2> CreateTrigger<T1, T2>(string key, Action<T1, T2> action)
        {
            var binding = new TriggerBinding<T1, T2>(LSDLayeredSelectionDisplayMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }

        /// <summary>
        /// Creates a trigger binding for a specified key and action with three parameters.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the third parameter for the action.</typeparam>
        /// <param name="key">The key for the binding.</param>
        /// <param name="action">The action to invoke when the trigger is activated.</param>
        /// <returns>A trigger binding for the specified key and action with three parameters.</returns>
        public TriggerBinding<T1, T2, T3> CreateTrigger<T1, T2, T3>(string key, Action<T1, T2, T3> action)
        {
            var binding = new TriggerBinding<T1, T2, T3>(LSDLayeredSelectionDisplayMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }

        /// <summary>
        /// Creates a trigger binding for a specified key and action with four parameters.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter for the action.</typeparam>
        /// <param name="key">The key for the binding.</param>
        /// <param name="action">The action to invoke when the trigger is activated.</param>
        /// <returns>A trigger binding for the specified key and action with four parameters.</returns>
        public TriggerBinding<T1, T2, T3, T4> CreateTrigger<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> action)
        {
            var binding = new TriggerBinding<T1, T2, T3, T4>(LSDLayeredSelectionDisplayMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }
    }
}
