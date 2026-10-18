## BulletPhysics

This folder houses bullet, the physics engine used by `Stride.Physics` assembly.

* `libbulletc.*` is the bullet c++ library, we should have one lib compiled for each platform and architecture.

* `BulletSharp.NetStandard.dll` is the C# wrapper around that C++ library, there's one DLL for any platform.

### Sources

The sources, build scripts and instructions are hosted over at https://github.com/stride3d/BulletSharpPInvoke

At the time of writing this (26/June/19), the files in this folder were built under commit [2a79f9af9c3fcc61aaad69a9a24e6b1f7246f758](https://github.com/stride3d/BulletSharpPInvoke/commit/2a79f9af9c3fcc61aaad69a9a24e6b1f7246f758), changes were merged with stride through [PR#289](https://github.com/stride3d/stride/pull/289).
