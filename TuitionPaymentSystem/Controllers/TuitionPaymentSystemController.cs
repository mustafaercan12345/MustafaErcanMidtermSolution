using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Transactions;
using TuitionPaymentSystem.Models;

namespace TuitionPaymentSystem.Controllers
{
    [Route("api/v1/Tuition")]
    public class TuitionPaymentSystemController : ControllerBase
    {

        /// <summary>
        /// For getting tuition details of the user.
        /// </summary>
        public class GetTuitionRes
        {
            public int StudentId { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public double Tuition { get; set; }
            public double Balance { get; set; }
            public bool IsPaid { get; set; }
        }

        /// <summary>
        /// Get tuition and balance and the payment status of student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public GetTuitionRes GetTuition([FromQuery] int personId)
        {
            var student = (Student)Person.people.FirstOrDefault(person => person.PersonId == personId && person.PersonType == "student");

            if (student == null)
            {
                throw new BadHttpRequestException("No such person");
            }

            return new GetTuitionRes
            {
                StudentId = student.PersonId,
                Balance = student.Balance,
                IsPaid = student.DidPay,
                Tuition = student.Tuition,
                Name = student.Name,
                Surname = student.Surname,
            };
        }

        /// <summary>
        /// Request model for payment request 
        /// </summary>
        public class PaymentReq
        {
            public int StudentId { get; set; }
            public double PaymentAmount { get; set; }
        }

        public class PaymentRes
        {
            public string Status { get; set; }
        }

        /// <summary>
        /// Pay tuition for the student
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("pay")]
        public PaymentRes Pay([FromBody] PaymentReq request)
        {
            var student = (Student)Person.people.FirstOrDefault(person => person.PersonId == request.StudentId && person.PersonType == "student");

            if (student == null)
            {
                throw new BadHttpRequestException(JsonConvert.SerializeObject(new PaymentRes { Status = "error" }));
            }

            student.Balance += request.PaymentAmount;

            student.DidPay = student.Tuition <= student.Balance;

            return new PaymentRes { Status = "success" };
        }

        public class AddTuitionReq
        {
            public int StudentId { get; set; }
            public double ExtraTuition { get; set; }
        }

        public class AddTuitionRes
        {
            public string TransactionStatus { get; set; }
        }

        /// <summary>
        /// Add extra tuition to student 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("addTuition")]
        public AddTuitionRes AddTuition([FromBody] AddTuitionReq request)
        {
            var student = (Student)Person.people.FirstOrDefault(person => person.PersonId == request.StudentId && person.PersonType == "student");

            if (student == null)
            {
                throw new BadHttpRequestException(JsonConvert.SerializeObject(new AddTuitionRes { TransactionStatus = "error" }));
            }

            student.Tuition += request.ExtraTuition;

            student.DidPay = student.Tuition <= student.Balance;

            return new AddTuitionRes { TransactionStatus = "success" };
        }

        public class GetMultipleStudentsRes
        {
            public List<GetTuitionRes> Students { get; set; }
        }

        /// <summary>
        /// Get students who didn't pay
        /// </summary>
        /// <param name="page"></param>
        /// <param name="studentCountForAPage"></param>
        /// <returns></returns>
        [HttpGet("studentsWhoDidntPay")]
        public GetMultipleStudentsRes GetStudentsWhoDidntPayYet([FromQuery] int page, [FromQuery] int studentCountForAPage)
        {
            return new GetMultipleStudentsRes
            {
                Students = Person.people.Where(person => person.PersonType == "student").Select(person => (Student)person)
                .Where(student => student.DidPay == false)
                .Skip(studentCountForAPage * (page - 1)).Take(studentCountForAPage).Select(student => new GetTuitionRes
                {
                    Balance = student.Balance,
                    IsPaid = student.DidPay,
                    Name = student.Name,
                    StudentId = student.PersonId,
                    Surname = student.Surname,
                    Tuition = student.Tuition,
                }).ToList()
            };
        }

    }
}
