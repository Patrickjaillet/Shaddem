// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void SaveThenLoad_RoundTripsValues()
    {
        string path = Path.Combine(Path.GetTempPath(), $"settings_test_{Guid.NewGuid():N}.json");
        try
        {
            var original = new AppSettings
            {
                WindowWidth = 1600,
                WindowHeight = 900,
                ShaderSwitchInterval = 45,
                MusicFile = "audio/mytrack.mp3",
                MusicVolume = 0.42f,
                AudioReactive = false,
                AutoSaveSettings = false,
            };
            original.Effects.Intensity = 2.5f;
            original.Effects.Bloom = 1.5f;

            SettingsService.Save(original, path);
            AppSettings loaded = SettingsService.Load(path);

            Assert.Equal(1600, loaded.WindowWidth);
            Assert.Equal(900, loaded.WindowHeight);
            Assert.Equal(45, loaded.ShaderSwitchInterval);
            Assert.Equal("audio/mytrack.mp3", loaded.MusicFile);
            Assert.Equal(0.42f, loaded.MusicVolume);
            Assert.False(loaded.AudioReactive);
            Assert.False(loaded.AutoSaveSettings);
            Assert.Equal(2.5f, loaded.Effects.Intensity);
            Assert.Equal(1.5f, loaded.Effects.Bloom);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        AppSettings loaded = SettingsService.Load(Path.Combine(Path.GetTempPath(), $"does_not_exist_{Guid.NewGuid():N}.json"));

        Assert.Equal(new AppSettings().WindowWidth, loaded.WindowWidth);
    }

    [Fact]
    public void Load_ClampsOutOfRangeValues()
    {
        string path = Path.Combine(Path.GetTempPath(), $"settings_invalid_{Guid.NewGuid():N}.json");
        try
        {
            var invalid = new AppSettings
            {
                WindowWidth = 50,
                WindowHeight = 100000,
                MusicVolume = 5.0f,
            };
            invalid.Effects.Bloom = 999.0f;
            invalid.Effects.Gamma = -50.0f;

            SettingsService.Save(invalid, path);
            AppSettings loaded = SettingsService.Load(path);

            Assert.InRange(loaded.WindowWidth, 320, 7680);
            Assert.InRange(loaded.WindowHeight, 240, 4320);
            Assert.InRange(loaded.MusicVolume, 0.0f, 1.0f);
            Assert.InRange(loaded.Effects.Bloom, 0.0f, 5.0f);
            Assert.InRange(loaded.Effects.Gamma, 0.1f, 3.0f);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
