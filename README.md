# BakeSH
Tool to bake directional occlusion to SH in compute shader using Unity and visualize it. This bakes an HDR texture containing L1 spherical harmonics coefficients in each pixel, which can be used to shade a mesh with directional occlusion (check VizSH.shader for an example).

# Example
Comparison with and without baked directional occlusion:

<div>
<img src="https://i.imgur.com/BbLCb3N.png" alt="drawing" width="450"/>
<img src="https://i.imgur.com/0fpeCQG.png" alt="drawing" width="450"/>
</div>
  
# How to use
1. Make a new scene and put the mesh(es) you wish to bake into it as MeshFilter or SkinnedMeshRenderers.
2. Attach BakeSH component to a GameObject with either a MeshFilter or SkinnedMeshRenderer.
3. Configure desired setting.
4. Press the bake button.
5. A texture "bake.exr" will appear in your Assets folder.

The settings look like so:

![](https://i.imgur.com/cKRoHTd.png)

And are explained below:

| Setting name       | Description                                                                                                                             |
|--------------------|-----------------------------------------------------------------------------------------------------------------------------------------|
| Texture Size       | Size of baked texture. Prefer powers of two.                                                                                            |
| Samples Per Pixel  | How many rays to shoot for each pixel of the texture while baking.                                                                      |
| Ray Push Off       | The minimum allowed intersection distance. Use to prevent shadow acne.                                                                  |
| Ray Max Distance   | The maximum allowed intersection distance. Use to prevent unrealistic occlusion.                                                        |
| Occlusion Strength | How dark occluded areas will be.                                                                                                        |
| Grid Density       | The density of the grid used for raytracing. Changing might yield increased performance.                                                |
| Sample Type        | Which sampling method to use while baking. Cosine Weighted will look best in most scenarios. Spherical may be good for specific meshes. |
| Stitch Seams       | Whether to fix ugly seams at edges of UV islands.                                                                                       |
| Draw Grid          | Draw the grid used for raytracing.                                                                                                      |
