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
        /// <para>
        /// This is a generic feature property implementation
        /// </para>
        /// <para>
        /// Second docs paragraph. With some <c>Inline code</c>.
        /// </para>
        /// </summary>
        public IGenericFeature<Person> GenericFeature { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// <para>
        /// This is some method. This paragraph references the <paramref name="input"/> parameter and gives a <c>Code example</c>.<br />
        /// It also has a | character, that should be escaped in Markdown tables.
        /// </para>
        /// <para>
        /// <see cref="Person"/> for more details about the person model.
        /// </para>
        /// <para>
        /// <seealso cref="ISampleFeature"/> class.
        /// </para>
        /// <para>
        /// If you don't trust these docs, please search on https://www.google.com by yourself.
        /// </para>
        /// </summary>
        /// <param name="input">The <see cref="Person"/> is passed as input</param>
        public void SomeMethod(Person input)
        {
            // Do something
        }
    }
}
