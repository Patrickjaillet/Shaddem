# ShaderDemo

**ShaderDemo** is a real-time GLSL shader visualizer for Windows, built with **.NET 8** and **Silk.NET** (OpenGL + windowing) with an **ImGui.NET**-based control panel. It renders a library of fragment shaders with a full post-processing effect stack, audio-reactive modulation, a timeline/automation system, 3D model overlay, particle system, live shader coding, and MP4/GIF video export via `ffmpeg`.

This project is a from-scratch **C# port** of an original Python/Pygame/ModernGL shader demo, rebuilt end-to-end on the .NET/Silk.NET stack.

> Status: early / actively developed (`v0.1.0`). See [`roadmap.md`](roadmap.md) for the detailed, living feature checklist and porting decisions.

## Demo videos

[![ShaderDemo video 1](https://img.youtube.com/vi/YUYbXfiL-mI/maxresdefault.jpg)](https://www.youtube.com/watch?v=YUYbXfiL-mI)
[![ShaderDemo video 2](https://img.youtube.com/vi/MNAm-WXl_5s/maxresdefault.jpg)](https://www.youtube.com/watch?v=MNAm-WXl_5s)

## Features

- **Shader engine** — loads any number of `.glsl` fragment shaders from a folder, hot-reloads them on file change, and reports compile errors live in the UI.
- **Layer stack** — stack multiple shaders with per-layer blend modes (Add, Multiply, Screen, Overlay, Difference, Exclusion, ...) and opacity.
- **Post-processing pipeline** — color grading (brightness/contrast/saturation/hue/posterize/sepia/solarize), geometric distortion (fisheye, wave, mirror, vortex, swirl, pixelate, mosaic), glitch/retro effects (RGB split, chromatic aberration, VHS, scanlines, datamosh), bloom, motion blur, feedback accumulation, and shader-to-shader transitions (wipe/fade/zoom/pixelize).
- **Audio reactivity** — real-time FFT analysis (spectrum + waveform) drives shake, strobe, RGB split, scale, and kick-triggered effects; on-screen spectrum/waveform overlay with trail decay.
- **Timeline & automation** — schedule shader switches, effect changes, and text overlays over time.
- **3D model overlay** — loads Wavefront `.obj` models, with wireframe/solid rendering, transform and lighting controls, composited on top of the shader background.
- **Particle system** — CPU-simulated particle field with kick-driven radial push.
- **Live coding** — edit and hot-compile a shader's `mainImage` directly from the in-app editor, with an insertable snippet library (palette, 2D rotation, 2D noise, raymarching loop).
- **Presets** — save, load, and randomly generate full effect-parameter presets (retro/art/geo/trippy/dark themes).
- **Video export** — record the composited output to MP4 (H.264, with optional NVENC/QSV hardware acceleration) or GIF, with audio muxing and a live progress bar, via `ffmpeg`.
- **Screenshots** — one-key PNG capture of the current frame.
- **Secondary output window** — mirror the composited output to a second, borderless window (e.g. for projection/streaming), sharing the primary GL context.

## Requirements

- **Windows** (the app targets `win-x64` and is published as a self-contained single-file executable)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [ffmpeg](https://ffmpeg.org/download.html) available on `PATH` — only required for video/GIF export; the app runs fine without it otherwise

## Getting started

Clone the repository and restore/build the solution:

```powershell
git clone https://github.com/<your-username>/ShaderDemo.git
cd ShaderDemo
dotnet restore ShaderDemo.sln
dotnet build ShaderDemo.sln
```

Run it directly from source:

```powershell
dotnet run --project src/ShaderDemo.App
```

Or produce a self-contained, single-file `win-x64` build with the provided build script:

```powershell
./build.ps1
```

The published executable is written to `publish/ShaderDemo.exe`, alongside the `shaders/` folder that ships with it.

## Usage

- **Left / Right arrow** — switch to the previous/next loaded shader
- The ImGui control panel (open by default) exposes every effect group, the layer stack, audio settings, the timeline editor, presets, live coding, the 3D model tab, export controls, and the secondary window toggle
- Command-line flags:
  - `--test-full` — runs the built-in headless-style integration smoke test and exits
  - `--test-shader <name>` — loads a single named shader, renders a few frames, saves a screenshot, and exits

Settings, layer stacks, timeline data, and presets are persisted next to the executable as `settings.json`, `layers.json`, `timeline.json`, and `presets.json`.

## Project structure

```
ShaderDemo.sln
src/
  ShaderDemo.App/     Entry point, window bootstrap, wiring
  ShaderDemo.Core/     Rendering engine, GUI panels, audio, timeline, export, settings
shaders/               Bundled GLSL fragment shaders (Shadertoy-style mainImage)
assets/                 Application icon
roadmap.md              Detailed feature/porting roadmap and decisions log
build.ps1               Self-contained win-x64 publish script
```

## Tech stack

- [.NET 8](https://dotnet.microsoft.com/) / C# 12
- [Silk.NET](https://github.com/dotnet/Silk.NET) — windowing, input, and OpenGL bindings
- [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET) — immediate-mode GUI
- [NAudio](https://github.com/naudio/NAudio) — audio decoding and FFT analysis
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp) / [StbImageWriteSharp](https://github.com/StbSharp/StbImageWriteSharp) — texture and screenshot I/O
- [Microsoft.Data.Sqlite](https://learn.microsoft.com/dotnet/standard/data/sqlite/) — local data persistence
- `ffmpeg` (external process) — video/GIF encoding

## Contributing

Contributions, bug reports, and shader submissions are welcome. Please read [`CONTRIBUTING.md`](CONTRIBUTING.md) before opening a pull request.

## Changelog

See [`CHANGELOG.md`](CHANGELOG.md) for release notes.

## License

Distributed under the MIT License. See [`LICENSE`](LICENSE) for the full text.

Copyright (c) 2026 Patrick JAILLET
