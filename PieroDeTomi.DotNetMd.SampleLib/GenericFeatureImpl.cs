using PieroDeTomi.DotNetMd.SampleLib.Models;

namespace PieroDeTomi.DotNetMd.SampleLib
{
    /// <summary>
    /// This is a generic feature implementation
    /// </summary>
    /// <remarks>This is not a normal implementation</remarks>
    public class GenericFeatureImpl : IGenericFeature<Person>
    {
        /// <summary>
        /// This is a generic feature property implementation
        /// </summary>
        public IGenericFeature<Person> GenericFeature { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// This is some method
        /// </summary>
        /// <param name="input">The <see cref="Person"/> is passed as input</param>
        public void SomeMethod(Person input)
        {
            // Do something
        }
    }
}
