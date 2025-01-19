using FMInatorul.Data;
using FMInatorul.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public static class SeedData
{
    public static void Initialize(IServiceProvider
        serviceProvider)
    {
        using (var context = new ApplicationDbContext(
                   serviceProvider.GetRequiredService
                       <DbContextOptions<ApplicationDbContext>>()))
        {
            if (context.Roles.Any()) return; // baza de date contine deja roluri
            // CREAREA ROLURILOR IN BD
            // daca nu contine roluri, acestea se vor crea
            context.Roles.AddRange(
                new IdentityRole
                { Id = "2c5e174e-3b0e-446f-86af483d56fd7210", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                new IdentityRole
                {
                    Id = "2c5e174e-3b0e-446f-86af483d56fd7211",
                    Name = "Profesor",
                    NormalizedName = "Profesor".ToUpper()
                },
                new IdentityRole
                {
                    Id = "2c5e174e-3b0e-446f-86af483d56fd7212",
                    Name = "Student",
                    NormalizedName = "Student".ToUpper()
                }
            );

            var hasher = new PasswordHasher<ApplicationUser>();
            // CREAREA USERILOR IN BD
            // Se creeaza cate un user pentru fiecare rol
            context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                    // primary key
                    UserName = "admin@test.com",
                    FirstName = "Admin",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,
                        "Admin1!")
                },
                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                    // primary key
                    UserName = "profesor@unibuc.ro",
                    FirstName = "Profesor",
                    LastName = "Profesor",
                    EmailConfirmed = true,
                    NormalizedEmail = "PROFESOR@TEST.COM",
                    Email = "profesor@test.com",
                    NormalizedUserName = "PROFESOR@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Profesor1!")
                },
                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                    // primary key
                    UserName = "student@s.unibuc.ro",
                    FirstName = "Student",
                    LastName = "Student",
                    EmailConfirmed = true,
                    NormalizedEmail = "STUDENT@TEST.COM",
                    Email = "student@test.com",
                    NormalizedUserName = "STUDENT@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Student1!")
                }
            );
            // ASOCIEREA USER-ROLE
            context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af483d56fd7212",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                }
            );

            //Seeding Facultati
            if(!context.Facultati.Any())
            {
                context.Facultati.AddRange(
                    new Facultate
                    {
                        nume="?"
                    },
                    new Facultate
                    {
                        nume="FMI"
                    },
                    new Facultate
                    {
                        nume="FLLS"
                    },
                    new Facultate
                    {
                        nume="ACS"
                    }
                    );
                
                context.SaveChanges();
            }
            //Seeding Materii
            if (!context.Materii.Any())
            {
                context.Materii.AddRange(
                    new Materie
                    {
                        nume = "Structuri algebrice in informatica",
                        anStudiu = 1,
                        semestru = 1,
                        descriere = "Studenţii vor dobândi cunoştinţe teoretice avansate privind structurile algebrice şi abilitatea de a face calcule, raţionamente şi aplicaţii folosind noţiunile studiate.",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Arhitectura sistemelor de calcul",
                        anStudiu = 1,
                        semestru = 1,
                        descriere = "Însuşirea cunoştinţelor fundamentale privind organizarea şi funcţionarea calculatoarelor. Însuşirea cunoştinţelor privind aritmetica şi logica calculatoarelor, circuitele logice, arhitectura x86, programarea în limbaj de asamblare",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Programarea algoritmilor",
                        anStudiu = 1,
                        semestru = 1,
                        descriere = "Obținerea de cunoștințe de bază legate de programarea în limbajul Python. Cunoașterea şi folosirea tehnicilor de programare Greedy, Divide et Impera, Backtracking și Programare dinamică. Alegerea unui model potrivit pentru rezolvarea problemelor nou întâlnite, prin sinteza noțiunilor asimilate",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Logica Matematica si Computationala",
                        anStudiu = 1,
                        semestru = 1,
                        descriere = "Exersarea unor tehnici fundamentale de raţionament matematic şi a redactării demonstraţiilor formalizate. Însuşirea unei baze de cunoştinţe de teoria mulţimilor, structuri algebrice ordonate şi logică formală necesare pentru cursurile din semestrele următoare.",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Calcul diferential si integral",
                        anStudiu = 1,
                        semestru = 1,
                        descriere = "Capacitatea de analizare si sintetizare a notiunilor de Analiză Matematică. Capacitatea de soluţionare a problemelor interdisciplinare Dezvoltarea gândirii critice",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Gandire critica si etica academica",
                        anStudiu = 1,
                        semestru = 1,
                        descriere = "blah blah blah",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Tehnici Web",
                        anStudiu = 1,
                        semestru = 2,
                        descriere = "Obiectivul cursului este prezentarea tehnicilor și tehnologiilor pentru dezvoltarea de aplicații web pe partea de client și familiarizarea cu utilizarea serverelor web și a limbajului JavaScript pe partea de server (Node.js).",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Geometrie si Algebra Liniara",
                        anStudiu = 1,
                        semestru = 2,
                        descriere = "Invatarea unor notiuni si teoreme importante de geometrie euclidiana, a unor teoreme de clasificare si a unor metode specifice de determinare de invarianti metrici",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Structuri de date",
                        anStudiu = 1,
                        semestru = 2,
                        descriere = "Studentii isi vor dezvolta capacitatea de a intelege si de a implementa algoritmi si structuri de date precum şi capacitatea de a analiza si rezolva probleme.",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Baze de date",
                        anStudiu = 1,
                        semestru = 2,
                        descriere = "Familiarizarea studenților cu bazele de date relaționale, cu limbajul de interogare standard al acestora, cunoașterea direcțiilor recente în domeniul bazelor de date și aplicarea abilităților dobândite pentru proiectarea bazelor de date.",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Limbaje formale si automate",
                        anStudiu = 1,
                        semestru = 2,
                        descriere = "blah blah blah",
                        FacultateID = 2
                    },
                    new Materie
                    {
                        nume = "Programare oritentata pe obiecte",
                        anStudiu = 1,
                        semestru = 2,
                        descriere = "Materia pune accent pe dezvoltarea abilităților de a gândi și a structura programele într-un mod modular și reutilizabil, utilizând conceptele POO pentru a crea soluții eficiente și robuste la problemele de programare.",
                        FacultateID = 2
                    }
                );
            }
            context.SaveChanges();
        }
    }
}