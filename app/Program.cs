using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "Host=db;Port=5432;Database=dimdimdb;Username=dimdim;Password=dimdim123";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Garante que o banco e a tabela existam ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ========== ENDPOINTS CRUD ==========

// CREATE - Criar conta
app.MapPost("/contas", async (Conta conta, AppDbContext db) =>
{
    db.Contas.Add(conta);
    await db.SaveChangesAsync();
    return Results.Created($"/contas/{conta.Id}", conta);
});

// READ ALL - Listar contas
app.MapGet("/contas", async (AppDbContext db) =>
    await db.Contas.ToListAsync());

// READ ONE - Buscar conta por ID
app.MapGet("/contas/{id}", async (int id, AppDbContext db) =>
    await db.Contas.FindAsync(id) is Conta conta
        ? Results.Ok(conta)
        : Results.NotFound());

// UPDATE - Atualizar saldo
app.MapPut("/contas/{id}", async (int id, Conta input, AppDbContext db) =>
{
    var conta = await db.Contas.FindAsync(id);
    if (conta is null) return Results.NotFound();

    conta.Titular = input.Titular;
    conta.Saldo = input.Saldo;
    await db.SaveChangesAsync();
    return Results.Ok(conta);
});

// DELETE - Encerrar conta
app.MapDelete("/contas/{id}", async (int id, AppDbContext db) =>
{
    var conta = await db.Contas.FindAsync(id);
    if (conta is null) return Results.NotFound();

    db.Contas.Remove(conta);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// ========== MODELO ==========
public class Conta
{
    public int Id { get; set; }
    public string Titular { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

// ========== CONTEXTO DO BANCO ==========
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Conta> Contas => Set<Conta>();
}
