using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button[] optionsBtn;
    [SerializeField] private float[] optionAngles;


    [Header("UI Panels")]
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject gameScreen;

    [Space(10)]
    [SerializeField] private GameObject introLogo;
    [SerializeField] private GameObject introLogoTarget;

    [Space(10)]
    [SerializeField] private GameObject wheelObj;
    private Transform wheelParent;

    [Space(10)]
    [SerializeField] private RectTransform buttonsHolder;
    [SerializeField] private float startOffsetX = 200f;
    Vector2 startPos;

    [Space]
    [SerializeField] private GameObject infoWheel;
    [SerializeField] private GameObject wheelCover;


    float coverOgScale;





    private void Awake()
    {
        Application.targetFrameRate = 60;
        // Initialize UI states
        homeScreen.SetActive(true);
        gameScreen.SetActive(false);
        introLogo.SetActive(true);
        introLogoTarget.SetActive(false);

        coverOgScale = wheelCover.transform.localScale.x;
        wheelCover.transform.localScale = Vector3.zero;


    }


    private void OnEnable()
    {
        startButton.onClick.AddListener(StartGame);

        for (int i = 0; i < optionsBtn.Length; i++)
        {
            int index = i; // Capture the current index
            optionsBtn[i].onClick.AddListener(() => OptionsClicked(index));
        }
    }
    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartGame);
        for (int i = 0; i < optionsBtn.Length; i++)
        {
            int index = i; // Capture the current index
            optionsBtn[i].onClick.RemoveListener(() => OptionsClicked(index));
        }
    }

#if UNITY_EDITOR

    // void Update()
    // {
    //     // R to reset the game (for testing purposes)
    //     if (Input.GetKeyDown(KeyCode.R))
    //     {
    //         UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    //     }
    // }

#endif


    void StartGame()
    {
        homeScreen.SetActive(false);

        // Animate the intro logo to the target position and scale
        introLogo.transform.DOMove(introLogoTarget.transform.position, 1f).SetEase(Ease.InOutSine);
        introLogo.transform.DOScale(introLogoTarget.transform.localScale, 1f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // After animation completes, switch UI panels
            introLogo.SetActive(false);
            introLogoTarget.SetActive(true);
            StartCoroutine(GameScene_Begin());
        });

    }

    IEnumerator GameScene_Begin()
    {
        wheelParent = wheelObj.transform.parent;
        wheelObj.transform.SetParent(gameScreen.transform);

        wheelObj.transform.localScale = Vector3.zero;
        wheelObj.transform.localPosition = Vector3.zero;

        startPos = buttonsHolder.anchoredPosition;
        buttonsHolder.DOAnchorPos(startPos + new Vector2(-startOffsetX, 0f), 0f);

        yield return new WaitForSeconds(0.1f);
        gameScreen.SetActive(true);

        wheelObj.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Game has begun, you can add additional logic here
            MoveWheelToCentre();
        });
    }

    void MoveWheelToCentre()
    {
        wheelObj.transform.SetParent(wheelParent);
        wheelObj.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack);

        buttonsHolder.DOAnchorPos(startPos, .5f).SetEase(Ease.OutBack);
    }

    bool wheelCoverIsOpen = false;
    void OptionsClicked(int index)
    {
        Debug.Log("Option " + index + " clicked.");
        // Handle option button clicks here

        if (!wheelCoverIsOpen)
        {
            wheelCoverIsOpen = true;

            wheelCover.transform.DOScale(Vector3.one * coverOgScale, .5f).SetEase(Ease.InOutQuart).OnComplete(() =>
            {
                RotateWheel(index);
            });
        }
        else
        {
            RotateWheel(index);


        }
    }

    private void RotateWheel(int index)
    {
        float targetAngle = optionAngles[index];
        float currentZ = infoWheel.transform.localEulerAngles.z;

        // Normalize angles to 0-360
        currentZ = (currentZ + 360f) % 360f;
        targetAngle = (targetAngle + 360f) % 360f;

        // Calculate delta to reach target
        float delta = Mathf.DeltaAngle(currentZ, targetAngle);

        // Add one full rotation in the same direction
        if (delta >= 0)
            delta += 360f;
        else
            delta -= 360f;

        // Compute final rotation
        float finalZ = currentZ + delta;

        // Kill previous tweens
        infoWheel.transform.DOKill();

        // Animate rotation
        infoWheel.transform
            .DOLocalRotate(new Vector3(0f, 0f, finalZ), 0.8f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutBack);
    }

    public void HideCover()
    {
        wheelCover.transform.DOScale(Vector3.zero, .5f).SetEase(Ease.InOutQuart).OnComplete(() =>
               {
                   wheelCoverIsOpen = false;
               });
    }




}
