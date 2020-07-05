using UnityEngine;
using TMPro;
using ButtonGame.Inventories;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ButtonGame.UI.Inventories
{
    /// <summary>
    /// Root of the tooltip prefab to expose properties to other classes.
    /// </summary>
    public class ItemTooltip : MonoBehaviour
    {
        // CONFIG DATA
        [SerializeField] TextMeshProUGUI leftTitleText = null;
        [SerializeField] TextMeshProUGUI rightTitleText = null;
        [SerializeField] GameObject tooltipContainer = null;
        [SerializeField] GameObject tooltipPrefab = null;
        TooltipDescriptionField[] descriptionFields;

        List<GameObject> tooltips = new List<GameObject>();

        // PUBLIC

        public void Setup(ITooltipProvider item)
        {
            leftTitleText.text = item.GetDisplayName();
            rightTitleText.text = item.GetCategoryName();
            descriptionFields = item.GetDescriptionFields();
            foreach (TooltipDescriptionField itemDescription in descriptionFields)
            {
                GameObject tooltipGO = Instantiate(tooltipPrefab, tooltipContainer.transform);
                TooltipIconText tooltip = tooltipGO.GetComponent<TooltipIconText>();
                if(itemDescription.hasIcon)
                {
                    Image icon = tooltip.GetIcon();
                    icon.sprite = itemDescription.iconImage;
                }
                else
                {
                    tooltip.DisableIcon();
                }
                Text textField = tooltip.GetText();
                textField.text = itemDescription.description;
                tooltips.Add(tooltipGO);
            }
        }

        public void RemoveTooltip()
        {
            leftTitleText.text = null;
            rightTitleText.text = null;
            foreach (GameObject obj in tooltips)
            {
                Destroy(obj);
            }
            tooltips.Clear();
        }

        public Transform GetTooltipContainer()
        {
            return tooltipContainer.transform;
        }
    }
}
