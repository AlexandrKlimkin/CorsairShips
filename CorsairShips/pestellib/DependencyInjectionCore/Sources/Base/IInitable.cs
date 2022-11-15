namespace UnityDI.Base
{
    public interface IInitable
    {
        int Priority { get; }
        void OnInitDone();
    }
}