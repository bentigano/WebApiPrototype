namespace WebApiPrototype
{
    public static class MyUserEndpoints
    {
        // Extension method called from Program.cs
        public static void AddMyUserEndpoints(this IEndpointRouteBuilder app)
        {
            // these get mapped to the root endpoint (/Register, /Update)
            app.MapGet("Register", () => { return "Registered"; }).WithDescription("Registers a new object in the system.");
            app.MapPut("Update", () => { return "Updated"; }).WithDescription("Updates an existing object in the system.");

            // these get mapped under the group name (/api/v17/things)
            // this a rudimentary example of versioning via URL segments (v17) since you could have another group for v18
            // however the swagger documentation includes them all grouped together
            var things = app.MapGroup("/api/v17/things").WithDescription("Group Endpoint Description"); // used on endpoints that don't have their own description
            things.MapGet("", () => "return all things").WithDescription("Returns everything.");
            things.MapGet("/{id}", (int id) => $"return user with id {id}").WithDescription("Returns an object with a specific ID");
            things.MapPost("", (Thing thing) => $"add user with id {thing.ID}").WithDescription("Creates a new object.");
            things.MapPut("/{id}", (int id) => $"put user with id {id}").WithDescription("Updates an existing object.");
            things.MapDelete("/{id}", (int id) => $"delete user with id {id}"); // will use the group description since we didn't provide one
        }
    }

    public class Thing
    {
        public int ID { get; set; }
    }
}
