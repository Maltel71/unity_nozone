using UnityEngine;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Hotbar hotbar;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;

    [Header("Selection")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 1f, 1f);

    private UnityEngine.UI.Image[] _slotBackgrounds;
    private UnityEngine.UI.Image[] _iconImages;
    private TextMeshProUGUI[] _countTexts;
    private int _lastSelected = -1;

    private void Awake()
    {
        if (hotbar == null) { Debug.LogError("HotbarUI: Hotbar reference is missing.", this); return; }
        if (slotPrefab == null) { Debug.LogError("HotbarUI: Slot Prefab is missing.", this); return; }
        if (slotsParent == null) { Debug.LogError("HotbarUI: Slots Parent is missing.", this); return; }

        BuildSlots();
    }

    private void Update()
    {
        if (_slotBackgrounds == null) return;

        int selected = hotbar.SelectedSlot;
        if (selected != _lastSelected)
        {
            RefreshSelection(selected);
            _lastSelected = selected;
        }

        RefreshIcons();
    }

    private void BuildSlots()
    {
        int count = hotbar.SlotCount;
        _slotBackgrounds = new UnityEngine.UI.Image[count];
        _iconImages = new UnityEngine.UI.Image[count];
        _countTexts = new TextMeshProUGUI[count];

        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);
            slot.name = $"Slot_{i}";

            _slotBackgrounds[i] = slot.GetComponent<UnityEngine.UI.Image>();

            Transform iconTransform = slot.transform.Find("Icon");
            _iconImages[i] = iconTransform != null ? iconTransform.GetComponent<UnityEngine.UI.Image>() : null;

            Transform countTransform = slot.transform.Find("Count");
            _countTexts[i] = countTransform != null ? countTransform.GetComponent<TextMeshProUGUI>() : null;

            if (_slotBackgrounds[i] == null)
                Debug.LogWarning("HotbarUI: Slot prefab missing Image component.", this);
            if (_iconImages[i] == null)
                Debug.LogWarning("HotbarUI: Slot prefab missing 'Icon' child with Image.", this);
            if (_countTexts[i] == null)
                Debug.LogWarning("HotbarUI: Slot prefab missing 'Count' child with TextMeshProUGUI.", this);
        }
    }

    private void RefreshSelection(int selected)
    {
        for (int i = 0; i < _slotBackgrounds.Length; i++)
        {
            if (_slotBackgrounds[i] != null)
                _slotBackgrounds[i].color = i == selected ? selectedColor : normalColor;
        }
    }

    private void RefreshIcons()
    {
        for (int i = 0; i < _iconImages.Length; i++)
        {
            ItemData item = hotbar.GetItem(i);
            int stack = hotbar.GetStack(i);

            if (_iconImages[i] != null)
            {
                _iconImages[i].sprite = item?.icon;
                _iconImages[i].enabled = item != null;
            }

            if (_countTexts[i] != null)
            {
                _countTexts[i].text = stack > 1 ? stack.ToString() : "";
                _countTexts[i].enabled = stack > 1;
            }
        }
    }
}