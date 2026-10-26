// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core;

namespace Stride.Assets.Presentation.AssetEditors.GameEditor
{
    /// <summary>
    /// Enumerates the different type of transformation that can be performed on an object.
    /// </summary>
    public enum Transformation
    {
        /// <summary>
        /// A simple translation
        /// </summary>
        [Display("Use translation gizmo")]
        Translation,

        /// <summary>
        /// A simple rotation
        /// </summary>
        [Display("Use rotation gizmo")]
        Rotation,

        /// <summary>
        /// A simple scale
        /// </summary>
        [Display("Use scale gizmo")]
        Scale
    }
}
