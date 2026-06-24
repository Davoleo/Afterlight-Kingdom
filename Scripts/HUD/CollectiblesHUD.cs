using System;
using Gameplay;
using TMPro;
using UnityEngine;

namespace HUD
{
    public class CollectiblesHUD : MonoBehaviour
    {
        private CollectibleType type;
        private CollectiblesManager manager;

        private TextMeshProUGUI textComp;
        
        private void Start()
        {
            manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CollectiblesManager>();

            
            var typeString = gameObject.name;
            Enum.TryParse(typeString, out type);

            var countObj = transform.GetChild(1);
            textComp = countObj.GetComponent<TextMeshProUGUI>();


        }

        private void Update()
        {
            int count = manager.GetCount(type);
            textComp.text = count.ToString();
        }
    }
}
