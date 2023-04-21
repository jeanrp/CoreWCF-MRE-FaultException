using System.Threading.Tasks;
using CoreWCF.IdentityModel.Selectors;
using CoreWCF.Security;

namespace FaultExceptionIssueServer
{
    internal class CustomUserNameValidator : UserNamePasswordValidator
    {
        public override ValueTask ValidateAsync(string userName, string password)
        { 
            throw new MessageSecurityException("Unknown Username or Incorrect Password"); 
        }
    }
}
