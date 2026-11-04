# Flat Ground preview review

The preview uses a private copy of the vanilla Hoe particle material with a procedural cyan square border, sized to twice LevelRadius. FlatGroundPreviewComponent remains attached only to hex_flat_ground's _GhostOnly child. Existing particles resize when configuration changes, and the opacity pulse updates every frame. Private material and texture resources are released when the ghost is destroyed.

The terrain-following LineRenderers and Custom/GuiScroll shader lookup have been removed at the user's request. There is no overlay shader or terrain raycasting. Grass and uneven terrain can obscure the horizontal particle preview, as before. Terrain operations and paint settings are unchanged; fractional radii can affect vertices beyond the nominal outline because square leveling rounds up.

Focused in-game checks:

1. Select Flat Ground and confirm the cyan square appears without overlay-shader warnings.
2. Change LevelRadius while aiming and confirm the visible square resizes immediately.
3. Confirm the pulse continues without changing configuration.
4. Switch to vanilla tools and holster the Hoe; confirm normal preview visibility and cleanup.

The restored component builds against the local game and Unity assemblies. Output: HexTerrainHacks/bin/PreviewReview/HexTerrainHacks.dll. This build has not been deployed or tested in game. No commit was made. ValheimRIP was not modified.
