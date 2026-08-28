using System.Collections.Generic;
using System.Linq;
using Triggers;
using UnityEngine;

namespace Gameplay
{
    public class Level2GateController : MonoBehaviour
    {
        private List<LeverInteractionHandler> levers;

        private void Start()
        {
            levers = FindObjectsByType<LeverInteractionHandler>(FindObjectsSortMode.None).ToList();
        }

        private void Update()
        {
            bool gateOpenReqs = levers.All(lever => lever.WasActivated());
            Debug.Log( "active levers: "+ levers.Count(lever => lever.WasActivated()) + "/" + levers.Count);
            if (gateOpenReqs) gameObject.SetActive(false);
        }
    }
}