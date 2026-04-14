using NUnit.Framework;
using Assert = NUnit.Framework.Assert; // Esto elimina la ambigÃ¼edad

namespace TheFallOfBlackDeath.Tests.EditMode
{
    /// <summary>
    /// Provides automated verification for math smoke tests.
    /// </summary>
    public class MathSmokeTests
    {
        [Test]
        /// <summary>
        /// Adds the ition works.
        /// </summary>
        public void Addition_Works()
        {
            Assert.AreEqual(4, 2 + 2); // Ahora funcionarÃ¡ sin errores
        }
    }
}
