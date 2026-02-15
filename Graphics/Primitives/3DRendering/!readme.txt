Hi, if you are a Calamity addon or other mod dev, do not steal this directory.
I can almost guarantee that you will benefit from a simpler, less feature complete mesh renderer or simple shader.

This system is designed for easy debugging, completeness, correctness, ease of use, and arbitrary reuse. 
It is not designed for ease of understanding or compactness. It is unreasonable to expect that you will be able to maintain it as a beginner.
You have been warned.

contact 'undeathlyghost' on discord for more information

notes: 
all non-strip meshgen generates correct UVs but incorrect 'distorted' uvs may be desired sometimes for effects
i use 1e-6 as my primary epsilon value in this system, and i suggest you do the same for consistency
it is entirely possible to reuse meshes and you are encouraged to do so rather than reconstructing one if your mesh has no reason to be modified again
pooled meshes are preferred to avoid garbage collection for non-persistent/constantly changing meshes