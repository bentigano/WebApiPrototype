namespace WebApiPrototype
{
    public static class MyUserEndpoints
    {
        // Extension method called from Program.cs
        public static void AddMyUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("Register", () => { return "Registered"; });
            app.MapPut("Update", () => { return "Updated"; });
        }
    }
}
