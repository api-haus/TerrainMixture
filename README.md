Terrain Mixture
===============

> Extension for [Mixture](alelievr/Mixture) enabling Procedural Terrain Generation.

> Status: Proof Of Concept / WIP

## Project Description.

With inclusion of Terrain Topology Nodes, Mixture becomes a powerful tool for Procedural Terrain Generation.

Unlike other Editor-Centered solutions, Mixture batches entire graph as single Compute Buffer and executes on GPU in
background.

Mixture allows using both Fragment and Compute Shaders for Node implementations.

This package aims to provide a Procedural Terrain Generation framework utilizing Mixture as Node-Based Procedural
Engine.

Texture Outputs, such as Terrain Heightmap, Splat Maps, Detail Density Maps are simple to generate using Mixture's
powerful arsenal.

Additionally, Compute Buffer support allows for placing Tree Instances.

## Features.

* [x] Output various Terrain data from Static Mixture Graphs:
    * [x] Terrain Height
    * [x] Splat Maps
    * [x] Detail Output via Details Output Node
    * [ ] Tree Instances via Trees Output Node
* [x] Background execution with configurable amount of Runtime Skipped Frames.
* [ ] Runtime Tile-based "endless" setup.
* [x] In-Editor setup with realtime previews (using Editor Coroutines).
* [ ] Biome Blending.
* [ ] Stamps.
