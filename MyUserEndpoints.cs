namespace WebApiPrototype
{
    public static class MyUserEndpoints
    {
        // Extension method called from Program.cs
        public static void AddMyUserEndpoints(this IEndpointRouteBuilder app)
        {
            // these get mapped to the root endpoint (/Register, /Update)
            app.MapGet("Register", () => { return "Registered"; });
            app.MapPut("Update", () => { return "Updated"; });

            // these get mapped under the group name (/api/v17/things)
            var things = app.MapGroup("/api/v17/things");
            things.MapGet("", () => "return all things");
            things.MapGet("/{id}", (int id) => $"return user with id {id}");
            things.MapPost("", (Thing thing) => $"add user with id {thing.ID}");
            things.MapPut("/{id}", (int id) => $"put user with id {id}");
            things.MapDelete("/{id}", (int id) => $"delete user with id {id}");
        }
    }

    public class Thing
    {
        public int ID { get; set; }
    }
}
