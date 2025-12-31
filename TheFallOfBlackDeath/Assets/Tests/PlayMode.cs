using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TheFallOfBlackDeath.Tests.PlayMode
{
    public class CoroutineSmokeTests
    {
        [UnityTest]
        public IEnumerator WaitForFrames_Works()
        {
            yield return null; // 1 frame
            yield return null; // 2 frames
            
            // Usa la ruta completa para evitar errores de ambigüedad
            NUnit.Framework.Assert.Pass(); 
        }
    }
}