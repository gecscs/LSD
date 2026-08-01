// <copyright file="SelectedEntities.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>

using Colossal.UI.Binding;
using System.Collections.Generic;
using Unity.Entities;

namespace LSD_Layered_Selection_Display.Domain
{
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

        public void AddSelectedEntity(Entity e)
        {
            SelectedEntity selectedEntity = new SelectedEntity
            {
                Index = e.Index,
                Version = e.Version
            };
            Entities.Add(selectedEntity);
        }

        public void Clear()
        {
            Entities.Clear();
        }
    }
}
