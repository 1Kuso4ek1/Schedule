using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace TodoApi;

internal static class ScheduleApi
{
    public static RouteGroupBuilder MapSchedule(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/schedule");

        group.WithTags("Schedule");

        // Add security requirements, all incoming requests to this API *must*
        // be authenticated with a valid user.
        group.RequireAuthorization(pb => pb.RequireCurrentUser());

        // Rate limit all of the APIs
        group.RequirePerUserRateLimit();

        // Validate the parameters
        group.WithParameterValidation(typeof(ScheduleItem));

        group.MapGet("/", async (ScheduleDbContext db, CurrentUser owner) =>
        {
            return await db.Schedule.Where(todo => todo.OwnerId == owner.Id).Select(t => t.AsScheduleItem()).AsNoTracking().ToListAsync();
        });

        group.MapGet("/{id}", async Task<Results<Ok<ScheduleItem>, NotFound>> (ScheduleDbContext db, int id, CurrentUser owner) =>
        {
            return await db.Schedule.FindAsync(id) switch
            {
                Schedule todo when todo.OwnerId == owner.Id || owner.IsAdmin => TypedResults.Ok(todo.AsScheduleItem()),
                _ => TypedResults.NotFound()
            };
        });

        group.MapPost("/", async Task<Created<ScheduleItem>> (ScheduleDbContext db, ScheduleItem newSchedule, CurrentUser owner) =>
        {
            var schedule = new Schedule
            {
                Group = newSchedule.Group,
                OwnerId = owner.Id
            };

            db.Schedule.Add(schedule);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/schedule/{schedule.Id}", schedule.AsScheduleItem());
        });

        group.MapPut("/{id}", async Task<Results<Ok, NotFound, BadRequest>> (ScheduleDbContext db, int id, ScheduleItem schedule, CurrentUser owner) =>
        {
            if (id != schedule.Id)
            {
                return TypedResults.BadRequest();
            }

            var rowsAffected = await db.Schedule.Where(t => t.Id == id && (t.OwnerId == owner.Id || owner.IsAdmin))
                                             .ExecuteUpdateAsync(updates =>
                                                updates.SetProperty(t => t.Days, schedule.Days)
                                                       .SetProperty(t => t.Group, schedule.Group));

            return rowsAffected == 0 ? TypedResults.NotFound() : TypedResults.Ok();
        });

        group.MapDelete("/{id}", async Task<Results<NotFound, Ok>> (ScheduleDbContext db, int id, CurrentUser owner) =>
        {
            var rowsAffected = await db.Schedule.Where(t => t.Id == id && (t.OwnerId == owner.Id || owner.IsAdmin))
                                             .ExecuteDeleteAsync();

            return rowsAffected == 0 ? TypedResults.NotFound() : TypedResults.Ok();
        });

        return group;
    }
}
