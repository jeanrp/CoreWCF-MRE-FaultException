using System;

namespace FaultExceptionIssueServer
{
    public class EmailService : IEmailService
    {
        public void Send()
        {
            Console.WriteLine("Sending...");
        }
    }
}
