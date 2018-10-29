namespace Lab.Common.Infra
{
    public class ErrorPoco
    {
        public string Id { get; set; }
        public System.DateTime ErrorDate { get; set; }
        public string ErrorMessage { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }
        public string UserComment { get; set; }
        public string Email { get; set; }
        public string ValidationErrors { get; set; }
        public string ErrorSource { get; set; }
        public string StackTrace { get; set; }
        public string InnerExceptionSource { get; set; }
        public string InnerExceptionMessage { get; set; }
        public string UserName { get; set; }
        public string AdditionalMessage { get; set; }
        public string QSData { get; set; }
        public string PostData { get; set; }
        public string Referrer { get; set; }
        public string Status { get; set; }
    }

    public class ErrResponsePoco
    {
        public string DbErrorId { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
    }

}
