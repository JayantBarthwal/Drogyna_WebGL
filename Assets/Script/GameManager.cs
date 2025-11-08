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

    [Space]
    [SerializeField] private GameObject StartGameBtn;

    [Space]
    [SerializeField] private RectTransform watermarkLeft;
    [SerializeField] private RectTransform watermarkRight;

    [Space]
    [SerializeField] private Color btnPressedColor;


    Sequence infoWheelSlowSpin;

    float coverOgScale;


    [Header("Audio ")]
    [SerializeField] private AudioSource source;

    [SerializeField] private AudioClip introVO;
    [SerializeField] private AudioClip wooshSFX;
    [SerializeField] private AudioClip startBtnClick;
    [SerializeField] private AudioClip wheelIntro;
    [SerializeField] private AudioClip wheelSpinSFX;
    [SerializeField] private AudioClip btnClickSFX;


    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem flyingParticlesPrefab;
    [SerializeField] private ParticleSystem healBigPrefab;


    private void Awake()
    {
        Application.targetFrameRate = 60;
        // Initialize UI states
        homeScreen.SetActive(true);
        gameScreen.SetActive(false);
        introLogo.SetActive(true);
        introLogoTarget.SetActive(false);
        StartGameBtn.transform.localScale = Vector3.zero;

        watermarkLeft.DOAnchorPosX(-600, 0f);
        watermarkRight.DOAnchorPosX(600, 0f);
        introLogo.transform.DOScale(Vector3.zero, 0f);


        coverOgScale = wheelCover.transform.localScale.x;
        wheelCover.transform.localScale = Vector3.zero;


    }

    void Start()
    {
        StartCoroutine(IntroSequence());

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
    #region Intro Sequence

    private IEnumerator IntroSequence()
    {
        yield return new WaitForSeconds(0.5f);
        //StartCoroutine(WatermarkSequence());
        watermarkLeft.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
        PlayOneShot(wooshSFX);

        yield return new WaitForSeconds(0.5f);

        watermarkRight.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
        PlayOneShot(wooshSFX);

        yield return new WaitForSeconds(0.5f);

        introLogo.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        PlayOneShot(wooshSFX);

        yield return new WaitForSeconds(.5f);

        PlayOneShot(introVO);

        yield return new WaitForSeconds(introVO.length + 0.5f);
        StartGameBtn.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        PlayOneShot(wooshSFX);

    }
    // IEnumerator WatermarkSequence()
    // {
    //     watermarkLeft.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
    //     PlayOneShot(wooshSFX);

    //     yield return new WaitForSeconds(0.25f);

    //     watermarkRight.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack);
    //     PlayOneShot(wooshSFX);

    //     yield return new WaitForSeconds(0.25f);

    //     introLogo.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    //     PlayOneShot(wooshSFX);

    // }
    #endregion


    void StartGame()
    {
        StopFlyingParticles();
        PlayOneShot(startBtnClick);
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

        PlayOneShot(wheelIntro);
        wheelObj.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Game has begun, you can add additional logic here
            StartSlowRotation();
            MoveWheelToCentre();
        });
    }

    void MoveWheelToCentre()
    {
        PlayOneShot(wooshSFX);

        wheelObj.transform.SetParent(wheelParent);
        wheelObj.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Additional logic after wheel is centered can be added here
            Vector3 wheelWorldPos = wheelObj.transform.position;
            wheelWorldPos.z = 0f;
            healBigPrefab.transform.position = wheelWorldPos;
        });

        buttonsHolder.DOAnchorPos(startPos, .5f).SetEase(Ease.OutBack);
    }

    bool wheelCoverIsOpen = false;
    void OptionsClicked(int index)
    {
        Debug.Log("Option " + index + " clicked.");
        healBigPrefab.Play();
        PlayOneShot(btnClickSFX);
        // Handle option button clicks here
        ChangeOptionsColor(index);
        optionsBtn[index].transform.DOPunchScale(Vector3.one * .05f, .2f, 5, 1f);

        if (!wheelCoverIsOpen)
        {
            wheelCoverIsOpen = true;
            PlayOneShot(wooshSFX);

            wheelCover.transform.DOScale(Vector3.one * coverOgScale, .5f).SetEase(Ease.InOutQuart).OnComplete(() =>
            {
                StopSlowRotation();
                RotateWheel(index);
            });
        }
        else
        {
            RotateWheel(index);


        }
    }
    void ChangeOptionsColor(int index)
    {
        for (int i = 0; i < optionsBtn.Length; i++)
        {
            Color c = (i == index) ? btnPressedColor : Color.white;
            optionsBtn[i].GetComponent<Image>().color = c;
        }
    }

    private void RotateWheel(int index)
    {
        PlayOneShot(wheelSpinSFX);
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
        StartSlowRotation();
        PlayOneShot(wooshSFX);

        wheelCover.transform.DOScale(Vector3.zero, .5f).SetEase(Ease.InOutQuart).OnComplete(() =>
               {
                   wheelCoverIsOpen = false;
               });
    }


    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }


    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null && source != null)
        {
            source.PlayOneShot(clip);
        }
    }

    private void StartSlowRotation()
    {
        infoWheelSlowSpin = DOTween.Sequence();
        infoWheelSlowSpin.Append(infoWheel.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), 60f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1));
    }
    private void StopSlowRotation()
    {
        if (infoWheelSlowSpin != null)
        {
            infoWheelSlowSpin.Kill();
            infoWheel.transform.localEulerAngles = Vector3.zero;
        }

    }

    void StopFlyingParticles()
    {
        ParticleSystem ps = flyingParticlesPrefab;
        var main = ps.emission;
        main.enabled = false;
    }
}
