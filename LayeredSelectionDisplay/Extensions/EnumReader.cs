// <copyright file="EnumReader.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LayeredSelectionDisplay.Extensions
{
    using Colossal.UI.Binding;

    /// <summary>
    /// A reader for enum values that reads an integer from JSON and casts it to the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type to read.</typeparam>
    public class EnumReader<T> : IReader<T>
    {
        /// <summary>
        /// Reads an integer value from the provided JSON reader and casts it to the specified enum type T. The method outputs the casted enum value through the out parameter.
        /// </summary>
        /// <param name="reader">The JSON reader to read the integer value from.</param>
        /// <param name="value">The output parameter that will hold the casted enum value.</param>
        public void Read(IJsonReader reader, out T value)
        {
            reader.Read(out int value2);
            value = (T)(object)value2;
        }
    }
}
