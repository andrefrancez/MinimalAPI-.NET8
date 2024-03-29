﻿using project.Domain.Enums;

namespace project.Domain.ModelViews
{
    public record AdminModelViews
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
    }
}
