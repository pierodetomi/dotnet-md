namespace PieroDeTomi.DotNetMd.SampleLib
{
    /// <summary>
    /// This is just a sample feature
    /// </summary>
    public interface ISampleFeature
    {
        /// <summary>
        /// This is the first method
        /// </summary>
        /// <param name="id">The id of something to find</param>
        /// <returns>The result of the method</returns>
        string FirstMethod(int id);

        /// <summary>
        /// This is a generic method
        /// </summary>
        /// <typeparam name="T">The type of the generic method</typeparam>
        /// <param name="id">The id of something to look up for</param>
        /// <returns>The result of the computation</returns>
        T FirstMethod<T>(int id);

        /// <summary>
        /// This is the second method
        /// </summary>
        /// <typeparam name="T">A T generic param</typeparam>
        /// <typeparam name="U">A V generic param</typeparam>
        /// <typeparam name="V">A V generic param</typeparam>
        /// <param name="uParam">The first input parameter of the method</param>
        /// <param name="vParam">The second input parameter</param>
        /// <returns>A generic T value</returns>
        T SecondMethod<T, U, V>(U uParam, V vParam);
    }
}
