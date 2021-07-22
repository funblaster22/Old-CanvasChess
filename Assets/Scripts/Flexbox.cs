namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Horizontal Layout Group", 150)]
    /// <summary>
    /// Layout class for arranging child elements side by side while maintaining aspect ratio and within container.
    /// </summary>
    public class Flexbox : HorizontalOrVerticalLayoutGroup
    {
        protected Flexbox() { }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal() {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical() {
            CalcAlongAxis(1, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal() {
            SetChildrenAlongAxis(0, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical() {
            SetChildrenAlongAxis(1, false);
        }

        protected new void SetChildrenAlongAxis(int axis, bool isVertical) {
            float pos = 0;
            float childWidth = rectTransform.rect.width / rectChildren.Count;
            float childHeight = Mathf.Min(childWidth, rectTransform.rect.height);

            for (int i = 0; i < rectChildren.Count; i++) {
                RectTransform child = rectChildren[i];
                if (axis == 0)  // Set horizontal (left/right) pos & width
                    SetChildAlongAxis(child, axis, pos, childWidth);
                else  // Set vertical (up/down) pos & height
                    SetChildAlongAxis(child, axis, rectTransform.rect.height - childHeight, childHeight);
                pos += childWidth;
            }
        }
    }
}
