
using API_Pets.Data;
using API_Pets.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace API_Pets
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            //1. Get the connection string from appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            //2. Add DbContext to the services container
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapPost("/seedpets", async (ApplicationDbContext db) =>
            {
                //check if the database is already seeded
                if (await db.Users.AnyAsync() || await db.Pets.AnyAsync())
                {
                    return Results.BadRequest("The database already seeded");
                }

                var passwordHasher = new PasswordHasher<User>();
                //Create a user to own the pets
                var user = new User
                {
                    Username = "petowner",
                    Email = "owner@example.com",
                    Role = "User"
                };

                user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");

                //Create some pets and assign them to the user
                var pets = new List<Pets>
                {
                    new Pets{ Name = "Buddy", Breed = "Golden Retriever" },
                    new Pets{ Name = "Lucy", Breed = "Siamese Cat" },
                    new Pets{ Name = "Rocky", Breed = "Boxer", ImageUrl= "https://picsum.photos/200/301" }
                };

                //Add the user and pets to the database
                db.Users.Add(user);
                db.Pets.AddRange(pets); //because multiple pets
                await db.SaveChangesAsync();

                return Results.Ok("Database seeded successfully with 1 user and 3 pets.");
            });

            //POST / register - User Registration
            app.MapPost("/register", async (UserRegisterDto newUser, ApplicationDbContext db) =>
            { 
                if (await db.Users.AnyAsync(u => u.Username == newUser.Username || u.Email == newUser.Email))
                {
                    return Results.BadRequest("Username or Email already exists.");
                }

                //1. Create an instance of the password hasher.
                var passwordHasher = new PasswordHasher<User>();
                var user = new User
                {
                    Username = newUser.Username,
                    Email = newUser.Email,
                    Role = newUser.role
                };
                //2. Hash the password using the new hasher
                user.PasswordHash = passwordHasher.HashPassword(user, newUser.Password);

                db.Users.Add(user);
                await db.SaveChangesAsync();

                return Results.Created($"/users/{user.Username}", new {user.Username, user.Email, user.Role});
            })
            .WithOpenApi();


            //POST / login - User Login
            app.MapPost("/login", async (UserLoginDto loginData, ApplicationDbContext db) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginData.Username);
                if (user == null)
                {
                    return Results.BadRequest("Invalid username or password.");
                }

                //1. Create an instance of the password hasher.
                var passwordHasher = new PasswordHasher<User>();
                //2. Verify the password 
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginData.Password);
                //3. Check the result of the verification
                if (result == PasswordVerificationResult.Failed)
                {
                    return Results.BadRequest("Invalid username or password.");
                }
                //Login successful
                return Results.Ok(new { Message = "Login successful!", user.Username, user.Email, user.Role });
            })
            .WithOpenApi();


            //GET all pets
            app.MapGet("/pets", async (ApplicationDbContext db) =>
                await db.Pets.ToListAsync());

            //GET a single pet by ID
            app.MapGet("/pets/{id}", async (int id, ApplicationDbContext db) =>
            {
                var pet = await db.Pets.FindAsync(id);
                if(pet is not null)
                {
                    return Results.Ok(pet);
                }
                else
                {
                    return Results.NotFound();
                }
            });


            //POST (Create) a new pet
            app.MapPost("/pets", async (PetCreateDto newPet, ApplicationDbContext db) =>
            {
                var pet = new Pets
                {
                    Name = newPet.Name,
                    Breed = newPet.Breed,
                    ImageUrl = newPet.ImageUrl
                };

                db.Pets.Add(pet);
                await db.SaveChangesAsync();

                return Results.Created($"/pets/{pet.Id}", pet);
            });

            //PUT (Update) an existing pet
            app.MapPut("/pets/{id}", async (int id, PetUpdateDto updatedPet, ApplicationDbContext db) =>
            {
                var pet = await db.Pets.FindAsync(id);

                if(pet is null)
                {
                    return Results.NotFound();
                }

                pet.Name = updatedPet.Name;
                pet.Breed = updatedPet.Breed;
                pet.ImageUrl = updatedPet.ImageUrl;

                await db.SaveChangesAsync();
                return Results.NoContent();
            });



            //DELETE a pet 
            app.MapDelete("/pets/{id}", async (int id, ApplicationDbContext db) =>
            {
                var pet = await db.Pets.FindAsync(id);

                if(pet is null)
                {
                    return Results.NotFound();
                }

                db.Pets.Remove(pet);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            app.Run();


          
    }
        //Hides sensitive info from attackers and users by not showing the models directly - prevents sql injection attacks
        //Data Transfer Objects (DTOs)
        public record UserRegisterDto(string Username, string Email, string Password, string role);
        public record UserLoginDto(string Username, string Password);

        //Pet Data Transfer Object
        public record PetCreateDto(string Name, string Breed, string ImageUrl);
        public record PetUpdateDto(string Name, string Breed, string ImageUrl);
    }
}
