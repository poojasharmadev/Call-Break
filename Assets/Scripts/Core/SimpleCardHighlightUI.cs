using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class SimpleCardHighlightUI : MonoBehaviour
    {
        RectTransform borderRoot;
        readonly Image[] borderEdges = new Image[4];

        public void Configure(Color borderColor, float borderThickness, float borderPadding)
        {
            EnsureBorderExists();

            borderRoot.offsetMin = new Vector2(-borderPadding, -borderPadding);
            borderRoot.offsetMax = new Vector2(borderPadding, borderPadding);

            ApplyEdgeLayout(borderThickness);

            for (int i = 0; i < borderEdges.Length; i++)
            {
                borderEdges[i].color = borderColor;
            }
        }

        public void SetVisible(bool visible)
        {
            EnsureBorderExists();
            borderRoot.gameObject.SetActive(visible);
        }

        void EnsureBorderExists()
        {
            if (borderRoot != null)
            {
                return;
            }

            GameObject borderObject = new GameObject("PlayableHighlight", typeof(RectTransform));
            borderObject.transform.SetParent(transform, false);

            borderRoot = borderObject.GetComponent<RectTransform>();
            borderRoot.anchorMin = Vector2.zero;
            borderRoot.anchorMax = Vector2.one;
            borderRoot.offsetMin = Vector2.zero;
            borderRoot.offsetMax = Vector2.zero;
            borderRoot.localScale = Vector3.one;

            CreateEdge("TopEdge", 0);
            CreateEdge("RightEdge", 1);
            CreateEdge("BottomEdge", 2);
            CreateEdge("LeftEdge", 3);

            borderRoot.gameObject.SetActive(false);
        }

        void CreateEdge(string edgeName, int edgeIndex)
        {
            GameObject edgeObject = new GameObject(edgeName, typeof(RectTransform), typeof(Image));
            edgeObject.transform.SetParent(borderRoot, false);

            Image edgeImage = edgeObject.GetComponent<Image>();
            edgeImage.raycastTarget = false;
            borderEdges[edgeIndex] = edgeImage;
        }

        void ApplyEdgeLayout(float thickness)
        {
            SetHorizontalEdge((RectTransform)borderEdges[0].transform, thickness, true);
            SetVerticalEdge((RectTransform)borderEdges[1].transform, thickness, false);
            SetHorizontalEdge((RectTransform)borderEdges[2].transform, thickness, false);
            SetVerticalEdge((RectTransform)borderEdges[3].transform, thickness, true);
        }

        void SetHorizontalEdge(RectTransform edge, float thickness, bool top)
        {
            edge.anchorMin = new Vector2(0f, top ? 1f : 0f);
            edge.anchorMax = new Vector2(1f, top ? 1f : 0f);
            edge.pivot = new Vector2(0.5f, 0.5f);
            edge.offsetMin = new Vector2(0f, top ? -thickness : 0f);
            edge.offsetMax = new Vector2(0f, top ? 0f : thickness);
        }

        void SetVerticalEdge(RectTransform edge, float thickness, bool left)
        {
            edge.anchorMin = new Vector2(left ? 0f : 1f, 0f);
            edge.anchorMax = new Vector2(left ? 0f : 1f, 1f);
            edge.pivot = new Vector2(0.5f, 0.5f);
            edge.offsetMin = new Vector2(left ? 0f : -thickness, 0f);
            edge.offsetMax = new Vector2(left ? thickness : 0f, 0f);
        }
    }
}
