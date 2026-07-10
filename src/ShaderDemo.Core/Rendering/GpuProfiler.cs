// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class GpuProfiler : IDisposable
{
    private sealed class Section : IDisposable
    {
        private readonly GL _gl;
        private readonly uint[] _queries = new uint[2];
        private readonly bool[] _pending = new bool[2];
        private int _writeIndex;

        public double LastElapsedMilliseconds { get; private set; }

        public Section(GL gl)
        {
            _gl = gl;
            _queries[0] = gl.GenQuery();
            _queries[1] = gl.GenQuery();
        }

        public void Begin()
        {
            _gl.BeginQuery(QueryTarget.TimeElapsed, _queries[_writeIndex]);
        }

        public void End()
        {
            _gl.EndQuery(QueryTarget.TimeElapsed);
            _pending[_writeIndex] = true;

            int readIndex = 1 - _writeIndex;
            if (_pending[readIndex])
            {
                _gl.GetQueryObject(_queries[readIndex], QueryObjectParameterName.ResultAvailable, out uint available);
                if (available != 0)
                {
                    _gl.GetQueryObject(_queries[readIndex], QueryObjectParameterName.Result, out uint nanoseconds);
                    LastElapsedMilliseconds = nanoseconds / 1_000_000.0;
                    _pending[readIndex] = false;
                }
            }

            _writeIndex = readIndex;
        }

        public void Dispose()
        {
            _gl.DeleteQuery(_queries[0]);
            _gl.DeleteQuery(_queries[1]);
        }
    }

    private readonly GL _gl;
    private readonly Dictionary<string, Section> _sections = new();

    public bool Enabled { get; set; }

    public GpuProfiler(GL gl)
    {
        _gl = gl;
    }

    public void Begin(string name)
    {
        if (!Enabled) return;
        GetOrCreateSection(name).Begin();
    }

    public void End(string name)
    {
        if (!Enabled) return;
        GetOrCreateSection(name).End();
    }

    public IReadOnlyDictionary<string, double> GetTimingsMilliseconds()
    {
        var result = new Dictionary<string, double>();
        foreach ((string name, Section section) in _sections)
        {
            result[name] = section.LastElapsedMilliseconds;
        }

        return result;
    }

    private Section GetOrCreateSection(string name)
    {
        if (!_sections.TryGetValue(name, out Section? section))
        {
            section = new Section(_gl);
            _sections[name] = section;
        }

        return section;
    }

    public void Dispose()
    {
        foreach (Section section in _sections.Values) section.Dispose();
        _sections.Clear();
    }
}
