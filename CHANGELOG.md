# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[0.1.0]: https://github.com/<your-username>/ShaderDemo/releases/tag/v0.1.0
