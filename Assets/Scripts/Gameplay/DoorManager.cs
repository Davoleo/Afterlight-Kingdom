using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Triggers;
using Unity.VisualScripting;
using UnityEngine;

namespace Gameplay
{
    public class DoorManager : MonoBehaviour
    {
        public DoorTriggerHandler[] doors;

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

        public bool IsOpened(string id) => openedDoorIds.Contains(id);

        // private void Update()
        // {
        //     Debug.Log(openedDoorIds.ToSeparatedString(", "));
        //     Debug.Log(doors.Select(door => door.transform.position).ToSeparatedString(", "));
        // }

        // Called explicitly by SceneTransitions once the level has actually finished loading and
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
            doors = FindObjectsByType<DoorTriggerHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        private void RestoreOpenedState()
        {
            foreach (var door in doors)
            {
                door.gameObject.SetActive(!IsOpened(door.Id));
            }
        }
    }
}
