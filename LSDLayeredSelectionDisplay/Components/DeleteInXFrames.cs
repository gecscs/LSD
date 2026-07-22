// <copyright file="DeleteInXFrames.cs" company="0belix's Mods. MIT License">
// Copyright (c) 0belix's Mods. MIT License. All rights reserved.
// </copyright>
namespace LSD_Layered_Selection_Display.Components
{
    using Unity.Entities;

    /// <summary>
    /// A component used to delete an entity on the next frame.
    /// </summary>
    public struct DeleteInXFrames : IComponentData, IQueryTypeParameter
    {
        /// <summary>
        /// A count for frames remaining until deletion.
        /// </summary>
        public int m_FramesRemaining;
    }
}