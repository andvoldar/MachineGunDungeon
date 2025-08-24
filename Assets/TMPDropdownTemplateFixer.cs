// Assets/_Scripts/UI/TMPDropdownTemplateFixer.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class TMPDropdownTemplateFixer : MonoBehaviour
{
    [Tooltip("Altura visible del panel al desplegar (px).")]
    public float templateHeight = 480f;

    [Tooltip("Altura preferida por ítem (px).")]
    public float itemPreferredHeight = 40f;

    private TMP_Dropdown dd;

    void Awake()
    {
        dd = GetComponent<TMP_Dropdown>();
        if (!dd || !dd.template)
        {
            Debug.LogWarning("TMPDropdownTemplateFixer: TMP_Dropdown o Template no asignado.", this);
            return;
        }

        // 1) Template size
        var tplRT = dd.template.GetComponent<RectTransform>();
        if (tplRT)
        {
            tplRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, templateHeight);
            tplRT.pivot = new Vector2(0.5f, 1f);
            tplRT.anchorMin = new Vector2(0, 1);
            tplRT.anchorMax = new Vector2(1, 1);
            tplRT.anchoredPosition = Vector2.zero;
        }

        // 2) Viewport: RectMask2D + Image
        var viewport = dd.template.Find("Viewport") as RectTransform;
        if (viewport)
        {
            if (!viewport.GetComponent<RectMask2D>()) viewport.gameObject.AddComponent<RectMask2D>();
            if (!viewport.GetComponent<Image>()) viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            viewport.anchorMin = new Vector2(0, 0);
            viewport.anchorMax = new Vector2(1, 1);
            viewport.pivot = new Vector2(0.5f, 1f);
            viewport.anchoredPosition = Vector2.zero;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
        }

        // 3) Content: VLG + CSF
        var content = dd.template.Find("Viewport/Content") as RectTransform;
        if (content)
        {
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            if (!vlg) vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 8;

            var fitter = content.GetComponent<ContentSizeFitter>();
            if (!fitter) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.offsetMin = new Vector2(0, 0);
            content.offsetMax = new Vector2(0, 0);
        }

        // 4) Item: LayoutElement para fijar altura por fila
        var item = dd.template.Find("Viewport/Content/Item") as RectTransform;
        if (item)
        {
            var le = item.GetComponent<LayoutElement>();
            if (!le) le = item.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = itemPreferredHeight;
        }

        // 5) ScrollRect wiring
        var sr = dd.template.GetComponent<ScrollRect>();
        if (sr && viewport)
        {
            sr.content = content;
            sr.viewport = viewport;
            // Scrollbar vertical si existe
            var sb = dd.template.Find("Scrollbar") as RectTransform;
            if (sb) sr.verticalScrollbar = sb.GetComponent<Scrollbar>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
        }
    }
}
