namespace Aspects.UnitTest.Stubs
{
    /// <summary>
    /// A test dummy interface useful when testing things that require implementation like aspects
    /// </summary>
    public interface IDummyInterface
    {
        /// <summary>
        /// Gets the dummy data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        string GetDummyData(string data);
    }
}
