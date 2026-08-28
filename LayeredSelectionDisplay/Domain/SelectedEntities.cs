// <copyright file="SelectedEntities.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

namespace LayeredSelectionDisplay.Domain
{
    using System.Collections.Generic;
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Unity.Entities;

    public struct SelectedEntities : IJsonWritable
    {
        public List<SelectedEntity> Entities;

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(GetType().FullName);
            writer.PropertyName(nameof(Entities));
            writer.ArrayBegin(Entities.Count);

            foreach (SelectedEntity e in Entities)
            {
                e.Write(writer);
            }

            writer.ArrayEnd();
            writer.TypeEnd();
        }

        public void AddSelectedEntity(Entity e, string name)
        {
            SelectedEntity selectedEntity = new SelectedEntity
            {
                Index = e.Index,
                Version = e.Version,
                Name = name,
            };
            Entities.Add(selectedEntity);
        }

        public void Clear()
        {
            Entities.Clear();
        }
    }
}
