# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-07-10

### Added

- Image, text, and 3D-model layers as first-class, reorderable/blendable entries in the layer stack (the "megademo" compositing feature), alongside shader layers — with fit-mode/position/scale/rotation controls and a dedicated `LayersPanel` editor for each.
- Timeline-driven sequencing for Image, Model3D, and Music clips (previously legal to place on a track but silently inert during playback), with fade in/out and, for Music, seek-accurate playback start.
- Per-layer keyframing: a `LayerAutomation` timeline clip animates a named layer's opacity/enabled state, reusing the existing effect-parameter interpolation/easing system.
- A built-in "Image Layer Showcase" demo template proving image-layer compositing end-to-end through the template system.
- Texture memory budget: image layers loaded as layers are downscaled on load to a configurable maximum dimension (default 2048px) to bound VRAM use, with a live control in `LayersPanel`.
- Multi-layer performance benchmark harness measuring real CPU/GPU frame cost and VRAM as simultaneous layer count scales.

## [0.1.0] - 2026

### Added

- Initial C# port of the original Python/Pygame/ModernGL shader demo to .NET 8 + Silk.NET + ImGui.NET.
- Core rendering engine: GL window/context bootstrap, framebuffers, fullscreen quad, shader program compilation, layer/blend-mode model, and full render pipeline.
- Shader system: fragment shader wrapper with the full post-processing uniform set, folder-based shader loading with hot reload, shader compile error reporting, and an HLSL-to-GLSL compatibility converter.
- Post-processing effect parity: color grading, geometric distortion, glitch/retro effects, bloom, motion blur, feedback and datamosh accumulation, and shader-to-shader transitions.
- Audio-reactive modulation and a spectrum/waveform overlay driven by real-time FFT analysis.
- Timeline and automation system for scripted shader/effect/text-overlay changes over time.
- 3D model overlay (Wavefront `.obj` loading, wireframe/solid rendering, transform and lighting controls).
- CPU-simulated particle system with kick-driven behavior.
- Live shader coding panel with a snippet library.
- Preset system (save/load/randomize) for effect parameters.
- Video/GIF export via `ffmpeg`, including audio muxing, export progress reporting, and optional hardware-accelerated encoding (NVENC/QSV).
- PNG screenshot capture.
- Secondary output window sharing the primary GL context.
- ImGui-based control panel covering effects, layers, audio, timeline, presets, live coding, 3D model, export, and window management.
- Local settings/layers/timeline/presets persistence to JSON next to the executable.
- `build.ps1` self-contained `win-x64` publish script.

[1.0.0]: https://github.com/Patrickjaillet/Shaddem/releases/tag/v1.0.0
[0.1.0]: https://github.com/Patrickjaillet/Shaddem/releases/tag/v0.1.0
