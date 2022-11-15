#if !UNITY_5_3_OR_NEWER
using System.Threading.Tasks;
#endif

namespace PestelLib.ServerShared
{
    public interface IIapValidator
    {
        IapValidateResult IsValid(IapValidateQuery query, bool acceptOnFail);
#if !UNITY_5_3_OR_NEWER
        Task<IapValidateResult> IsValidAsync(IapValidateQuery query, bool acceptOnFail);
#endif
    }
}
