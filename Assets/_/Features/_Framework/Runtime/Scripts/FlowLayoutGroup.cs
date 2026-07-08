using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Flow Layout Group")]
public class FlowLayoutGroup : LayoutGroup
{
    public float spacingX = 8f;
    public float spacingY = 8f;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
    }

    public override void CalculateLayoutInputVertical()
    {
        float totalHeight = FormatLayout(true);
        // Indique correctement au système uGUI (et aux parents) la hauteur voulue
        SetLayoutInputForAxis(totalHeight, totalHeight, totalHeight, 1);
    }

    public override void SetLayoutHorizontal()
    {
        FormatLayout(false);
    }

    public override void SetLayoutVertical()
    {
        FormatLayout(false);
    }

    private float FormatLayout(bool forceCalculateHeight)
    {
        // CORRECTION : Si la largeur propre est 0, on prend la largeur du parent pour éviter la boucle infinie
        float width = rectTransform.rect.width;
        if (width <= 0 && rectTransform.parent != null && rectTransform.parent is RectTransform parentRect)
        {
            width = parentRect.rect.width;
        }

        // Si vraiment tout est à 0 (au premier frame), on met une valeur par défaut pour éviter le bug
        if (width <= 0) width = 100f;

        float paddingLeft = padding.left;
        float paddingTop = padding.top;
        
        float currentX = paddingLeft;
        float currentY = paddingTop;
        
        float rowHeight = 0f;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            
            float childWidth = LayoutUtility.GetPreferredSize(child, 0);
            float childHeight = LayoutUtility.GetPreferredSize(child, 1);

            // Gestion du retour à la ligne
            if (currentX + childWidth > width - padding.right && currentX > paddingLeft)
            {
                currentX = paddingLeft;
                currentY += rowHeight + spacingY;
                rowHeight = 0f;
            }

            if (!forceCalculateHeight)
            {
                SetChildAlongAxis(child, 0, currentX, childWidth);
                SetChildAlongAxis(child, 1, currentY, childHeight);
            }

            currentX += childWidth + spacingX;
            rowHeight = Mathf.Max(rowHeight, childHeight);
        }

        return currentY + rowHeight + padding.bottom;
    }
}
