using UnityEngine;
using UnityEngine.EventSystems;

namespace ButtonGame.Core.UI.Tooltips
{
    /// <summary>
    /// Abstract base class that handles the spawning of a tooltip prefab at the
    /// correct position on screen relative to a cursor.
    /// 
    /// Override the abstract functions to create a tooltip spawner for your own
    /// data.
    /// </summary>
    public abstract class TooltipSpawner : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // CONFIG DATA
        [Tooltip("The prefab of the tooltip to spawn.")]
        [SerializeField] protected GameObject tooltipPrefab = null;

        // PRIVATE STATE
        protected GameObject[] tooltips = null;

        /// <summary>
        /// Called when it is time to update the information on the tooltip
        /// prefab.
        /// </summary>
        /// <param name="tooltip">
        /// The spawned tooltip prefab for updating.
        /// </param>
        public abstract void UpdateTooltip();
        
        /// <summary>
        /// Return true when the tooltip spawner should be allowed to create a tooltip.
        /// </summary>
        public abstract bool CanCreateTooltip();

        // PRIVATE

        private void OnDestroy()
        {
            ClearTooltip();
        }

        private void OnDisable()
        {
            ClearTooltip();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            var parentCanvas = GetComponentInParent<Canvas>();

            foreach (GameObject tooltip in tooltips)
            {
                if (tooltip && !CanCreateTooltip())
                {
                    ClearTooltip();
                }

                if (!tooltip && CanCreateTooltip())
                {
                    GameObject tooltipInstance = Instantiate(tooltipPrefab, parentCanvas.transform);
                }

                if (tooltip)
                {
                    UpdateTooltip();
                    PositionTooltip();
                }
            }
        }

        private void PositionTooltip()
        {
            // Required to ensure corners are updated by positioning elements.
            Canvas.ForceUpdateCanvases();

            foreach (GameObject tooltip in tooltips)
            {
                var tooltipCorners = new Vector3[4];
                tooltip.GetComponent<RectTransform>().GetWorldCorners(tooltipCorners);
                var slotCorners = new Vector3[4];
                GetComponent<RectTransform>().GetWorldCorners(slotCorners);

                bool below = transform.position.y > Screen.height / 2;
                bool right = transform.position.x < Screen.width / 2;

                int slotCorner = GetCornerIndex(below, right);
                int tooltipCorner = GetCornerIndex(!below, !right);

                tooltip.transform.position = slotCorners[slotCorner] - tooltipCorners[tooltipCorner] + tooltip.transform.position;
            }
        }

        private int GetCornerIndex(bool below, bool right)
        {
            if (below && !right) return 0;
            else if (!below && !right) return 1;
            else if (!below && right) return 2;
            else return 3;

        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            ClearTooltip();
        }

        protected virtual void ClearTooltip()
        {
            if(tooltips.Length > 0)
            {
                foreach (GameObject tooltip in tooltips)
                {
                    Destroy(tooltip.gameObject);
                }
            }
        }
    }
}