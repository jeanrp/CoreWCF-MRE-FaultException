using System;
using System.IO;
using System.ServiceModel;
using FaultExceptionIssueClient.EmailService;

namespace FaultExceptionIssueClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WrappedServiceCall(new EmailServiceClient(), it => it.Send());
        }

        static void WrappedServiceCall<T>(ClientBase<T> service, Action<T> action) where T : class
        {
            var success = false;
            try
            {
                if (service.ClientCredentials == null) return;

                service.ClientCredentials.UserName.UserName = "UserName";
                service.ClientCredentials.UserName.Password = "Password"; 

                action(service as T);

                if (service.State == CommunicationState.Faulted) return;

                service.Close();
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType());
            }
            finally
            {
                if (!success)
                    service.Abort();
            }
        }
    }
}
