// <copyright file="GenericUIWriter.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LayeredSelectionDisplay.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Colossal.UI.Binding;
    using Game.UI;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// A generic UI writer that implements the IWriter interface for writing various types of values to JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value to be written to JSON.</typeparam>
    public class GenericUIWriter<T> : IWriter<T>
    {
        /// <summary>
        /// Writes the specified value of type T to the provided JSON writer. The method handles various types, including primitive types, enums, arrays, and objects, and writes them in a JSON-compatible format.
        /// </summary>
        /// <param name="writer">The JSON writer to write the value to.</param>
        /// <param name="value">The value to be written to JSON.</param>
        public void Write(IJsonWriter writer, T value)
        {
            WriteGeneric(writer, value);
        }

        /// <summary>
        ///    Writes an object of a specified type to the provided JSON writer. The method uses reflection to get the public properties and fields of the object and writes them as JSON properties. It handles various types, including primitive types, enums, arrays, and objects, and writes them in a JSON-compatible format.
        /// </summary>
        /// <param name="writer">The JSON writer to write the object to.</param>
        /// <param name="type">The type of the object to be written.</param>
        /// <param name="obj">The object to be written to JSON.</param>
        private static void WriteObject(IJsonWriter writer, Type type, object obj)
        {
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            writer.TypeBegin(type.FullName);

            foreach (var propertyInfo in properties)
            {
                writer.PropertyName(propertyInfo.Name);
                WriteGeneric(writer, propertyInfo.GetValue(obj));
            }

            foreach (var fieldInfo in fields)
            {
                writer.PropertyName(fieldInfo.Name);
                WriteGeneric(writer, fieldInfo.GetValue(obj));
            }

            writer.TypeEnd();
        }

        /// <summary>
        ///     Writes a generic object to the provided JSON writer. The method checks the type of the object and writes it in a JSON-compatible format, handling various types including primitive types, enums, arrays, and objects. If the object is null, it writes a null value. If the object implements IJsonWritable, it calls its Write method. For other types, it uses reflection to write the object's properties and fields.
        /// </summary>
        /// <param name="writer">The JSON writer to write the object to.</param>
        /// <param name="obj">The object to be written to JSON.</param>
        private static void WriteGeneric(IJsonWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteNull();
                return;
            }

            if (obj is IJsonWritable jsonWritable)
            {
                jsonWritable.Write(writer);
                return;
            }

            if (obj is int @int)
            {
                writer.Write(@int);
                return;
            }

            if (obj is bool @bool)
            {
                writer.Write(@bool);
                return;
            }

            if (obj is uint @uint)
            {
                writer.Write(@uint);
                return;
            }

            if (obj is float @float)
            {
                writer.Write(@float);
                return;
            }

            if (obj is double @double)
            {
                writer.Write(@double);
                return;
            }

            if (obj is string @string)
            {
                writer.Write(@string);
                return;
            }

            if (obj is Enum @enum)
            {
                writer.Write(Convert.ToInt32(@enum));
                return;
            }

            if (obj is Entity entity)
            {
                writer.Write(entity);
                return;
            }

            if (obj is Color color)
            {
                writer.Write(color);
                return;
            }

            if (obj is Array array)
            {
                WriteArray(writer, array);
                return;
            }

            if (obj is IEnumerable objects)
            {
                WriteEnumerable(writer, objects);
                return;
            }

            WriteObject(writer, obj.GetType(), obj);
        }

        private static void WriteArray(IJsonWriter writer, Array array)
        {
            writer.ArrayBegin(array.Length);

            for (var i = 0; i < array.Length; i++)
            {
                WriteGeneric(writer, array.GetValue(i));
            }

            writer.ArrayEnd();
        }

        private static void WriteEnumerable(IJsonWriter writer, object obj)
        {
            var list = new List<object>();

            foreach (var item in obj as IEnumerable)
            {
                list.Add(item);
            }

            writer.ArrayBegin(list.Count);

            foreach (var item in list)
            {
                WriteGeneric(writer, item);
            }

            writer.ArrayEnd();
        }
    }
}
