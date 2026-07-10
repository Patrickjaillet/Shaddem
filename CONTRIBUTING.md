# Contributing to ShaderDemo

Thanks for your interest in contributing! This document covers the basics for reporting issues, proposing changes, and submitting pull requests.

## Getting set up

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and (optionally, for video export) [ffmpeg](https://ffmpeg.org/download.html) on `PATH`.
2. Fork and clone the repository.
3. Restore and build:
   ```powershell
   dotnet restore ShaderDemo.sln
   dotnet build ShaderDemo.sln
   ```
4. Run from source:
   ```powershell
   dotnet run --project src/ShaderDemo.App
   ```

## Project conventions

These rules are enforced throughout the existing codebase and should be followed in any contribution:

- **Target framework**: .NET 8, C# 12.
- **No inline comments** in source code (no `//`, no `/* */`, no XML doc comments), with a single exception: every `.cs` file starts with one header line, `// Copyright (c) 2026 Patrick JAILLET`. Design rationale and decisions belong in [`roadmap.md`](roadmap.md), not in the code.
- **English only** for all UI text, log messages, file names, shader names, and exception messages.
- **Naming**: PascalCase for types/methods, camelCase for locals/fields (`_field` for private fields).
- **Nullable reference types** and **implicit usings** are enabled; keep new code warning-clean under these settings.
- Keep `roadmap.md` up to date: check boxes, add rows, and log notable decisions in its decisions-log section as part of the same change.

## Submitting shaders

New `.glsl` fragment shaders can be dropped into the `shaders/` folder. Shaders follow a Shadertoy-style `mainImage(out vec4 fragColor, in vec2 fragCoord)` entry point; see existing files in `shaders/` for examples and available uniforms (`iTime`, `iResolution`, `iMouse`, `iChannel0`, and the `EffectParams`-driven `custom*` uniforms).

## Pull requests

1. Create a feature branch from `main`.
2. Keep changes focused; unrelated fixes should be separate PRs.
3. Make sure the solution builds cleanly (`dotnet build ShaderDemo.sln`) before submitting.
4. Describe what changed and why in the PR description, and update `roadmap.md`/`CHANGELOG.md` if the change is user-facing.
5. Open the PR against `main` and fill in the template if one is provided.

## Reporting bugs

Please include:
- Steps to reproduce
- Expected vs. actual behavior
- Your OS/GPU, .NET SDK version, and whether you're running from source or a published build
- Any relevant log output or shader compiler errors from the in-app error panel

## Code of conduct

Be respectful and constructive. Harassment or abusive behavior toward other contributors will not be tolerated.
