﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract("PrefabAsset")]
    [AssetDescription(FileExtension, AllowArchetype = false)]
    [AssetContentType(typeof(Prefab))]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "1.7.0-beta01", typeof(SpriteComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta01", "1.7.0-beta02", typeof(UIComponentRenamingResolutionUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta02", "1.7.0-beta03", typeof(ParticleColorAnimationUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta03", "1.7.0-beta04", typeof(EntityDesignUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta04", "1.9.0-beta01", typeof(CharacterSlopeUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta01", "1.9.0-beta02", typeof(IdentifiableComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta02", "1.9.0-beta03", typeof(BasePartsRemovalComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta03", "1.9.0-beta04", typeof(MaterialFromModelComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta04", "1.9.0-beta05", typeof(ParticleTrailEdgeUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.0-beta05", "1.10.0-beta01", typeof(MoveRenderGroupInsideComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.10.0-beta01", "1.10.0-beta02", typeof(FixPartReferenceUpgrader))]
    [Display(1950, "Prefab")]
    public class PrefabAsset : EntityHierarchyAssetBase
    {
        private const string CurrentVersion = "1.10.0-beta02";

        /// <summary>
        /// The default file extension used by the <see cref="PrefabAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefab";

        /// <summary>
        /// Creates a instance of this prefab that can be added to another <see cref="EntityHierarchyAssetBase"/>.
        /// </summary>
        /// <param name="targetLocation">The location of the target container asset.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> containing the cloned entities of </returns>
        [NotNull]
        public AssetCompositeHierarchyData<EntityDesign, Entity> CreatePrefabInstance([NotNull] string targetLocation)
        {
            Guid unused;
            return CreatePrefabInstance(targetLocation, out unused);
        }

        /// <summary>
        /// Creates a instance of this prefab that can be added to another <see cref="EntityHierarchyAssetBase"/>.
        /// </summary>
        /// <param name="targetLocation">The location of the target container asset.</param>
        /// <param name="instanceId">The identifier of the created instance.</param>
        /// <returns>An <see cref="AssetCompositeHierarchyData{EntityDesign, Entity}"/> containing the cloned entities of </returns>
        [NotNull]
        public AssetCompositeHierarchyData<EntityDesign, Entity> CreatePrefabInstance([NotNull] string targetLocation, out Guid instanceId)
        {
            Dictionary<Guid, Guid> idRemapping;
            var instance = (PrefabAsset)CreateDerivedAsset(targetLocation, out idRemapping);
            instanceId = instance.Hierarchy.Parts.FirstOrDefault()?.Base?.InstanceId ?? Guid.NewGuid();
            return instance.Hierarchy;
        }
    }
}
