// <copyright file="SelectedEntity.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

using Colossal.UI.Binding;

namespace LayeredSelectionDisplay.Domain
{
    public struct SelectedEntity : IJsonWritable
    {
        public int Index;
        public int Version;
        public string Name;

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(GetType().FullName);
            writer.PropertyName(nameof(Index));
            writer.Write(Index);
            writer.PropertyName(nameof(Version));
            writer.Write(Version);
            writer.PropertyName(nameof(Name));
            writer.Write(Name);
            writer.TypeEnd();
        }

        public void Read(IJsonReader reader)
        {
            if (reader.ReadProperty(nameof(Index)))
            {
                reader.Read(out Index);
            }

            if (reader.ReadProperty(nameof(Version)))
            {
                reader.Read(out Version);
            }

            if (reader.ReadProperty(nameof(Name)))
            {
                reader.Read(out Name);
            }
        }
    }
}
