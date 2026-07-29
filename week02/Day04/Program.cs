using Day04.Domain;

namespace Day04
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // تفعيل خدمات السواجر فقط (وحذف سطر builder.Services.AddOpenApi())
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 1. تفعيل السواجر أولاً في بيئة التطوير
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My First API V1");
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            var minimalBooks = new List<Book>
                {
                    new Book { Id = 101, Title = "Clean Code", Author = "Robert C. Martin", Price = 45.00m },
                    new Book { Id = 102, Title = "C# in Depth", Author = "Jon Skeet", Price = 50.00m },
                    new Book { Id = 103, Title = "Design Patterns", Author = "Erich Gamma", Price = 55.50m }
                };

         
            app.MapGet("/api/minimal/books", () => Results.Ok(minimalBooks));

            app.MapGet("/api/minimal/books/{id}", (int id) =>
            {
                var book = minimalBooks.FirstOrDefault(b => b.Id == id);

                return book is not null
                    ? Results.Ok(book)
                    : Results.NotFound($"Book with ID {id} was not found.");
            });
            app.Run();
        }
    }
}