﻿// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FreeRangeCamera
{
    using System.Reflection;
    using Colossal.Logging;
    using Game;
    using Game.Modding;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class Mod : IMod
    {
        /// <summary>
        /// The mod's default name.
        /// </summary>
        public const string ModName = "Free Range Camera";

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        public static Mod Instance { get; private set; }

        /// <summary>
        /// Gets the mod's active log.
        /// </summary>
        internal ILog Log { get; private set; }

        /// <summary>
        /// Called by the game when the mod is loaded.
        /// </summary>
        /// <param name="updateSystem">Game update system.</param>
        public void OnLoad(UpdateSystem updateSystem)
        {
            // Set instance reference.
            Instance = this;

            // Initialize logger.
            Log = LogManager.GetLogger(ModName);
#if DEBUG
            Log.Info("setting logging level to Debug");
            Log.effectivenessLevel = Level.Debug;
#endif

            Log.Info($"loading {ModName} version {Assembly.GetExecutingAssembly().GetName().Version}");

            // Apply harmony patches.
            new Patcher("algernon-FreeRangeCamera", Log);

            // Don't do anything if Harmony patches weren't applied.
            if (Patcher.Instance is null || !Patcher.Instance.PatchesApplied)
            {
                Log.Critical("Harmony patches not applied; aborting system activation");
                return;
            }

            // Ensure system is created - we don't need to activate the update.
            updateSystem.World.GetOrCreateSystemManaged<FreeRangeCameraSystem>();
        }

        /// <summary>
        /// Called by the game when the mod is disposed of.
        /// </summary>
        public void OnDispose()
        {
            Log.Info("disposing");
            Instance = null;

            // Revert harmony patches.
            Patcher.Instance?.UnPatchAll();
        }
    }
}
