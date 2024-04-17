namespace TuitionPaymentSystem.Models
{
    public class Student : Person
    {
        public double Tuition { get; set; }
        public double Balance { get; set; }
        public bool DidPay { get; set; }
    }
}
