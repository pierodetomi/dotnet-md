namespace PieroDeTomi.DotNetMd.SampleLib
{
    /// <summary>
    /// This is a generic feature
    /// </summary>
    /// <typeparam name="T">Type of generic param</typeparam>
    public interface IGenericFeature<T> where T : class, new()
    {
        /// <summary>
        /// This is a generic feature property
        /// </summary>
        IGenericFeature<T> GenericFeature { get; set; }

        /// <summary>
        /// This is some method
        /// </summary>
        /// <param name="input">This is the input parameter for this method</param>
        void SomeMethod(T input);
    }
}
