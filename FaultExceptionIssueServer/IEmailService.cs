using CoreWCF;

namespace FaultExceptionIssueServer
{
    [ServiceContract]
    internal interface IEmailService
    {
        [OperationContract] 
        void Send();
    }
}
