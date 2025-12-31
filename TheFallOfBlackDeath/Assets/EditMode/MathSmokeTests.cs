using NUnit.Framework;
using Assert = NUnit.Framework.Assert; // Esto elimina la ambigüedad

namespace TheFallOfBlackDeath.Tests.EditMode
{
    public class MathSmokeTests
    {
        [Test]
        public void Addition_Works()
        {
            Assert.AreEqual(4, 2 + 2); // Ahora funcionará sin errores
        }
    }
}