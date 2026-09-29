using System.Collections;
using NUnit.Framework;
using SwipeClean.Core;
using SwipeClean.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace SwipeClean.Tests.PlayMode
{
    public sealed class PrototypeSmokeTests
    {
        [UnityTest]
        public IEnumerator RuntimeBootstrap_CreatesServicesAndPrototypeUi()
        {
            if (GameBootstrap.Current == null)
            {
                new GameObject("Test AppRoot").AddComponent<GameBootstrap>();
            }

            if (Object.FindFirstObjectByType<PrototypeFlowController>() == null)
            {
                new GameObject("Test Prototype UI").AddComponent<PrototypeFlowController>();
            }

            yield return null;

            Assert.That(GameBootstrap.Current, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PrototypeFlowController>(), Is.Not.Null);
        }
    }
}

