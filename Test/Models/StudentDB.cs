using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Test.Models
{
    class StudentContext : DbContext
    {
        public DbSet<Student> Students { get; set; }

        static StudentContext()
        {
            Database.SetInitializer(new DatabaseInitializer());
        }
    }

    class DatabaseInitializer : CreateDatabaseIfNotExists<StudentContext>
    {
        protected override void Seed(StudentContext context)
        {
            context.Students.AddRange(new Student[]
            {
                new Student {Surname = "Иванов", Name = "Иван", BirthYear =  2000},
                new Student {Surname = "Петров", Name = "Пётр", BirthYear =  1970},
                new Student {Surname = "Иващенко", Name = "Игорь", BirthYear =  1998},
                new Student {Surname = "Латышев", Name = "Андрей", BirthYear =  1999}
            });
            base.Seed(context);
        }
    }
}
