using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core.UI.Tooltips;
using ButtonGame.UI.Inventories;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ButtonGame.UI.EffectIcon
{
    [RequireComponent(typeof(EffectDisplay))]
    public class EffectTooltipSpawner : TooltipSpawner, IPointerEnterHandler
    {
        EffectDisplay effect = null;
        int fxDescriptionLength;
        List<Text> textLines = new List<Text>();

        private void OnEnable() 
        {
            effect = GetComponent<EffectDisplay>();
        }

        public override bool CanCreateTooltip()
        {
            fxDescriptionLength = effect.GetEffectDescriptionLength();
            return fxDescriptionLength > 0;
        }

        public override void UpdateTooltip()
        {
            var tooltipWindow = GameObject.FindGameObjectWithTag("TooltipWindow");
            if (!tooltipWindow) return;

            var itemTooltip = tooltipWindow.GetComponent<ItemTooltip>();
            Transform parent = itemTooltip.GetTooltipContainer();

            for (int i = 0; i < fxDescriptionLength; i++)
            {
                Text fxText = Instantiate(tooltipPrefab, parent).GetComponent<Text>();
                fxText.text = effect.GetEffectStat(i);
                textLines.Add(fxText);
            }

            itemTooltip.RemoveTooltip();
            itemTooltip.Setup(effect);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (CanCreateTooltip())
            {
                UpdateTooltip();
            }
        }

        protected override void ClearTooltip()
        {
            foreach (Text text in textLines)
            {
                Destroy(text.gameObject);
            }
            textLines.Clear();

            var tooltipWindow = GameObject.FindGameObjectWithTag("TooltipWindow");
            if (!tooltipWindow) return;

            var itemTooltip = tooltipWindow.GetComponent<ItemTooltip>();
            itemTooltip.RemoveTooltip();
        }
    }
}
