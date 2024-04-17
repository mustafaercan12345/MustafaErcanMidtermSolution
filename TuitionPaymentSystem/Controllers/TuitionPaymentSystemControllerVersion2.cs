using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Transactions;
using TuitionPaymentSystem.Models;

namespace TuitionPaymentSystem.Controllers
{
    [Route("api/v2/Tuition")]
    public class TuitionPaymentSystemControllerVersion2 : ControllerBase
    {

        /// <summary>
        /// For getting tuition details of the user.
        /// </summary>
        public class GetTuitionResV2
        {
            public int StudentId { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public double Tuition { get; set; }
            public double Balance { get; set; }
            public bool IsPaid { get; set; }
            
            /// <summary>
            /// This is not exists in the version 1
            /// </summary>
            public string Email { get; set; }
        }

        /// <summary>
        /// Get tuition and balance and the payment status of student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public GetTuitionResV2 GetTuition([FromQuery] int personId)
        {
            var student = (Student)Person.people.FirstOrDefault(person => person.PersonId == personId && person.PersonType == "student");

            if (student == null)
            {
                throw new BadHttpRequestException("No such person");
            }

            return new GetTuitionResV2
            {
                StudentId = student.PersonId,
                Balance = student.Balance,
                IsPaid = student.DidPay,
                Tuition = student.Tuition,
                Name = student.Name,
                Surname = student.Surname,
                Email = student.Email,
            };
        }

        /// <summary>
        /// Request model for payment request 
        /// </summary>
        public class PaymentReqV2
        {
            public int StudentId { get; set; }
            public double PaymentAmount { get; set; }
        }

        public class PaymentResV2
        {
            public string Status { get; set; }
        }

        /// <summary>
        /// Pay tuition for the student
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("pay")]
        public PaymentResV2 Pay([FromBody] PaymentReqV2 request)
        {
            var student = (Student)Person.people.FirstOrDefault(person => person.PersonId == request.StudentId && person.PersonType == "student");

            if (student == null)
            {
                throw new BadHttpRequestException(JsonConvert.SerializeObject(new PaymentResV2 { Status = "error" }));
            }

            student.Balance += request.PaymentAmount;

            student.DidPay = student.Tuition > student.Balance;

            return new PaymentResV2 { Status = "success" };
        }

        public class AddTuitionReqV2
        {
            public int StudentId { get; set; }
            public double ExtraTuition { get; set; }
        }

        public class AddTuitionResV2
        {
            public string TransactionStatus { get; set; }
        }

        /// <summary>
        /// Add extra tuition to student 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("addTuition")]
        [Authorize]
        public AddTuitionResV2 AddTuition([FromBody] AddTuitionReqV2 request)
        {
            var student = (Student)Person.people.FirstOrDefault(person => person.PersonId == request.StudentId && person.PersonType == "student");

            if (student == null)
            {
                throw new BadHttpRequestException(JsonConvert.SerializeObject(new AddTuitionResV2 { TransactionStatus = "error" }));
            }

            student.Tuition += request.ExtraTuition;

            student.DidPay = student.Tuition > student.Balance;

            return new AddTuitionResV2 { TransactionStatus = "success" };
        }

        public class GetMultipleStudentsResV2
        {
            public List<GetTuitionResV2> Students { get; set; }
        }

        /// <summary>
        /// Get students who didn't pay
        /// </summary>
        /// <param name="page"></param>
        /// <param name="studentCountForAPage"></param>
        /// <returns></returns>
        [HttpGet("studentsWhoDidntPay")]
        [Authorize]
        public GetMultipleStudentsResV2 GetStudentsWhoDidntPayYet([FromQuery] int page, [FromQuery] int studentCountForAPage)
        {
            return new GetMultipleStudentsResV2
            {
                Students = Person.people.Where(person => person.PersonType == "student").Select(person => (Student)person)
                .Skip(studentCountForAPage * (page - 1)).Take(studentCountForAPage).Select(student => new GetTuitionResV2
                {
                    Balance = student.Balance,
                    IsPaid = student.DidPay,
                    Name = student.Name,
                    StudentId = student.PersonId,
                    Surname = student.Surname,
                    Tuition = student.Tuition,
                    Email = student.Email,  
                }).ToList()
            };
        }

    }
}
