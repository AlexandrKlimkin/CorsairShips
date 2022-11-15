namespace ServiceCommon
{
    public interface PestelCrewService
    {
        /// <summary>
        /// Start and init service
        /// </summary>
        void Start();
        /// <summary>
        /// Stop service gracefully
        /// </summary>
        void Stop();
        /// <summary>
        /// Stop immediately
        /// </summary>
        void Terminate();
    }
}
