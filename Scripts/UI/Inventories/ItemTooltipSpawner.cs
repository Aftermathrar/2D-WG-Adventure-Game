using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ButtonGame.Core.UI.Tooltips;

namespace ButtonGame.UI.Inventories
{
    /// <summary>
    /// To be placed on a UI slot to spawn and show the correct item tooltip.
    /// </summary>
    [RequireComponent(typeof(IItemHolder))]
    public class ItemTooltipSpawner : TooltipSpawner, IPointerEnterHandler
    {
        public override bool CanCreateTooltip()
        {
            var item = GetComponent<IItemHolder>().GetItem();
            if (!item) return false;

            return true;
        }

        public override void UpdateTooltip()
        {
            // var itemTooltip = tooltip.GetComponent<ItemTooltip>();
            var tooltipWindow = GameObject.FindGameObjectWithTag("TooltipWindow");
            if (!tooltipWindow) return;

            var item = GetComponent<IItemHolder>().GetItem();
            var itemTooltip = tooltipWindow.GetComponent<ItemTooltip>();
            itemTooltip.RemoveTooltip();
            itemTooltip.Setup(item);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            // var parentCanvas = GetComponentInParent<Canvas>();

            // foreach (GameObject tooltip in tooltips)
            // {
            //     if (tooltip && !CanCreateTooltip())
            //     {
            //         ClearTooltip();
            //     }

                // if (!tooltip && CanCreateTooltip())
                // {
                //     GameObject tooltipInstance = Instantiate(tooltipPrefab, parentCanvas.transform);
                // }

                if (CanCreateTooltip())
                {
                    UpdateTooltip();
                }
            // }
        }

        protected override void ClearTooltip()
        {
            var tooltipWindow = GameObject.FindGameObjectWithTag("TooltipWindow");
            if (!tooltipWindow) return;

            var itemTooltip = tooltipWindow.GetComponent<ItemTooltip>();
            itemTooltip.RemoveTooltip();
        }
    }
}