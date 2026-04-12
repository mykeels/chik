using Bogus;
using Newtonsoft.Json;

namespace Chik.Exams.Data;

public static class ClassSeeder
{
    private static readonly Faker Faker = new("en");

    public static async Task Seed(IServiceProvider services)
    {
        var adminAuth = User.Admin;
        var classService = services.GetRequiredService<IClassService>();
        await classService.Create(adminAuth, new Class.Create("Preparatory"));
        await classService.Create(adminAuth, new Class.Create("Kindergarten 1"));
        await classService.Create(adminAuth, new Class.Create("Kindergarten 2"));
        await classService.Create(adminAuth, new Class.Create("Nursery"));
        await classService.Create(adminAuth, new Class.Create("Basic 1"));
        await classService.Create(adminAuth, new Class.Create("Basic 2"));
        await classService.Create(adminAuth, new Class.Create("Basic 3"));
        await classService.Create(adminAuth, new Class.Create("Basic 4"));
        await classService.Create(adminAuth, new Class.Create("Basic 5"));
        await classService.Create(adminAuth, new Class.Create("JSS 1"));
        await classService.Create(adminAuth, new Class.Create("JSS 2"));
        await classService.Create(adminAuth, new Class.Create("JSS 3"));
        await classService.Create(adminAuth, new Class.Create("SSS 1"));
        await classService.Create(adminAuth, new Class.Create("SSS 2"));
        await classService.Create(adminAuth, new Class.Create("SSS 3"));
    }
}
