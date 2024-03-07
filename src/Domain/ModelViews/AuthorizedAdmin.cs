namespace project.Domain.ModelViews
{
    public record AuthorizedAdmin
    {
        public string Email { get; set; }
        public string Profile { get; set; }
        public string Token { get; set; }
    }
}
