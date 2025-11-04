using System;

namespace RECO.Application.DTOs
{
    public class VideoDto
    {
        public string Key { get; set; } = string.Empty; // YouTube key for embedding
        public string Site { get; set; } = string.Empty; // e.g. "YouTube"
        public string Type { get; set; } = string.Empty; // e.g. "Trailer"
        public string? Name { get; set; }
    }
}
