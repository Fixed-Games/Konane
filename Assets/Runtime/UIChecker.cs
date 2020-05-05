using UnityEngine;
using UnityEngine.UI;

public class UIChecker : MonoBehaviour
{
    public ITriggable context = null;

    [SerializeField] private Button button = null;
    [SerializeField] private Image buttonFrame = null;
    [SerializeField] private Image icon = null;
    [SerializeField] private Image iconFrame = null;

    private bool iconNotified = false;
    private bool iconOneShotNotified = false;

    public void Check()
    {
        button.interactable = true;
        buttonFrame.color = Color.white;
        icon.enabled = true;
    }

    public void CheckOff()
    {
        button.interactable = false;
        buttonFrame.color = Color.clear;
        icon.enabled = true;
    }

    public void Clear()
    {
        iconFrame.color = Color.clear;
        iconNotified = false;
        iconOneShotNotified = false;
    }

    public void Init(Color color)
    {
        button.interactable = true;
        icon.color = color;
        icon.enabled = true;
        iconFrame.color = Color.clear;
    }

    public void Repaint()
    {
        if (iconNotified)
        {
            Color color = iconFrame.color;
            color.a = 1f;
            iconFrame.color = color;
            iconOneShotNotified = true;
        }
    }

    public void Repaint(float deltaTime)
    {
        if (iconOneShotNotified)
        {
            Color color = iconFrame.color;
            if (color.a > 0f)
            {
                color.a = Mathf.MoveTowards(color.a, 0f, deltaTime);
                iconFrame.color = color;
            }
            else
            {
                iconOneShotNotified = false;
            }
        }
    }

    public void SetInfo(Color color)
    {
        iconFrame.color = color;
        iconNotified = true;
        iconOneShotNotified = false;
    }

    public void SetInfoError()
    {
        iconFrame.color = Color.red;
        iconNotified = false;
        iconOneShotNotified = true;
    }

    public void SetInfoWarning()
    {
        iconFrame.color = Color.yellow;
        iconNotified = false;
        iconOneShotNotified = true;
    }

    public void Sleep()
    {
        button.interactable = false;
        buttonFrame.color = Color.clear;
        icon.enabled = false;
    }

    public void SleepWithCheck()
    {
        button.interactable = true;
        buttonFrame.color = Color.clear;
        icon.enabled = false;
    }

    private void Awake()
    {
        button.onClick.AddListener(OnButton);
    }

    private void OnButton()
    {
        context?.Trigger(GetHashCode());
    }

    private void OnDestroy()
    {
        context = null;
    }
}
