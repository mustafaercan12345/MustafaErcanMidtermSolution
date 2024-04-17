using Microsoft.AspNetCore.Mvc;

namespace TuitionPaymentSystem.Models
{
    public abstract class Person 
    {
        public static List<Person> people = new List<Person>
        {
            new Admin {  Name = "admin", Surname = "admin", Email = "admin@admin.com", Password = "admin", PersonId = 1, PersonType = "admin" },
            new Student { Name = "stu1", Surname = "stu1", Email = "stu1@stu.com", Password = "stu1", PersonId = 2, PersonType = "student", Balance = 0, DidPay = false, Tuition = 204000 },
            new Student { Name = "stu2", Surname = "stu2", Email = "stu2@stu.com", Password = "stu2", PersonId = 3, PersonType = "student", Balance = 0, DidPay = false, Tuition = 204000 },
            new Student { Name = "stu3", Surname = "stu3", Email = "stu3@stu.com", Password = "stu3", PersonId = 4, PersonType = "student", Balance = 0, DidPay = false, Tuition = 204000 },
            new Student { Name = "stu4", Surname = "stu4", Email = "stu4@stu.com", Password = "stu4", PersonId = 5, PersonType = "student", Balance = 0, DidPay = false, Tuition = 204000 },
        };

        public int PersonId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PersonType { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
