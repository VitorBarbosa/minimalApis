using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PeopleDb>(opt => opt.UseInMemoryDatabase("People"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!");
app.MapGet("/hello/{name}", (string name) => $"Hello, {name}!");

app.MapGet("/people", async (PeopleDb db) =>
    await db.People.ToListAsync());

app.MapGet("/people/{id}", async (int id, PeopleDb db) =>
await db.People.FindAsync(id)
    is Person person
        ? Results.Ok(person)
        : Results.NotFound());

app.MapPost("/people", async (Person person, PeopleDb db) =>
{
    db.People.Add(person);
    db.SaveChangesAsync();

    return Results.Created($"/person/{person.Id}", person);
});

app.MapPut("/people/{id}", async (int id, Person inputPerson, PeopleDb db) =>
{
    var person = await db.People.FindAsync(id);

    if (person is null) return Results.NotFound();

    person.LastName = inputPerson.LastName;
    person.FirstName = inputPerson.FirstName;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/people/{id}", async (int id, PeopleDb db) =>
{
    if (await db.People.FindAsync(id) is Person person)
    {
        db.People.Remove(person);
        await db.SaveChangesAsync();
        return Results.Ok(person);
    }

    return Results.NotFound();
});

app.Run();

class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

class PeopleDb: DbContext
{
    public PeopleDb(DbContextOptions<PeopleDb> options) 
        : base(options) { }
    
    public DbSet<Person> People => Set<Person>();
}