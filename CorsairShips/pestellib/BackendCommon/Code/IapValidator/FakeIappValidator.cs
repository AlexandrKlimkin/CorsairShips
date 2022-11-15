using System.Threading.Tasks;
using PestelLib.ServerShared;

namespace BackendCommon.Code.IapValidator
{
    public class FakeIappValidator : IIapValidator
    {
        public IapValidateResult IsValid(IapValidateQuery query, bool acceptOnFail)
        {
            return new IapValidateResult() { IsValid = true, IsTest = true };
        }

        public Task<IapValidateResult> IsValidAsync(IapValidateQuery query, bool acceptOnFail)
        {
            return Task.FromResult(new IapValidateResult() { IsValid = true, IsTest = true});
        }
    }
}