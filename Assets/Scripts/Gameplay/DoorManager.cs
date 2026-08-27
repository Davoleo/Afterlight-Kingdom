using System.Collections.Generic;
using Core;
using Triggers;
using UnityEngine;

namespace Gameplay
{
    public class DoorManager : MonoBehaviour
    {
        public List<GameObject> doors;

        public List<string> openedDoorIds = new();

        public bool RegisterOpenedDoor(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            openedDoorIds ??= new List<string>();

            if (openedDoorIds.Contains(id))
                return false;

            openedDoorIds.Add(id);
            return true;
        }

        public bool IsOpened(string id)
        {
            return openedDoorIds != null && openedDoorIds.Contains(id);
        }

        // Called explicitly by CoreLoader once the level has actually finished loading and
        // become the active scene - same reasoning as CollectiblesManager.RestoreFromSave:
        // the level's Door objects don't exist yet while this GameObject is still booting inside Core.
        public void RestoreFromSave(SaveData save)
        {
            openedDoorIds = save.openedDoorIds != null ? new List<string>(save.openedDoorIds) : new List<string>();

            RefreshDoorReferences();
            RestoreOpenedState();
        }

        private void RefreshDoorReferences()
        {
            doors = new List<GameObject>(GameObject.FindGameObjectsWithTag("Doors"));
        }

        private void RestoreOpenedState()
        {
            foreach (GameObject go in doors)
            {
                DoorTriggerHandler handler = go.GetComponent<DoorTriggerHandler>();
                go.SetActive(!IsOpened(handler.Id));
            }
        }
    }
}
