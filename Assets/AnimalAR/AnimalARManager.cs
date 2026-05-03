using UnityEngine;

public class AnimalARManager : MonoBehaviour
{
    public static AnimalARManager Instance;

    public enum GameState
    {
        WaitingForAnimal,
        WaitingForHome,
        EnvironmentDecision,
        PathBuilding,
        MovingHome,
        CareChallenge,
        Interaction
    }

    public AnimalData[] animals;
    public UIManager uiManager;
    public PlacementController placementController;

    public GameState CurrentState { get; private set; } = GameState.WaitingForAnimal;
    public AnimalData CurrentAnimalData { get; private set; }
    public string CurrentAnimalId { get; private set; }
    public string CurrentHomeId { get; private set; }

    private GameObject currentCardAnimalInstance;
    private Transform currentHomeTransform;
    private bool animalScannedThisRound;
    private int careScore;
    private bool gaveFish;
    private bool addedIce;
    private bool arcticDiscovered;
    private bool desertDiscovered;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();

        if (placementController == null)
            placementController = FindAnyObjectByType<PlacementController>();

        WireButtons();
        ResetSession();
    }

    public void OnAnimalTargetFound(string animalId, Transform animalTargetTransform)
    {
        if (animalScannedThisRound) return;

        AnimalData data = GetAnimalData(animalId);

        if (data == null)
        {
            Debug.LogWarning("No AnimalData found for animalId: " + animalId);
            return;
        }

        CurrentAnimalData = data;
        CurrentAnimalId = data.animalId;
        CurrentHomeId = null;
        currentHomeTransform = null;
        animalScannedThisRound = true;

        if (currentCardAnimalInstance != null)
            Destroy(currentCardAnimalInstance);

        if (data.cardAnimalPrefab != null)
        {
            currentCardAnimalInstance = Instantiate(
                data.cardAnimalPrefab,
                animalTargetTransform.position,
                animalTargetTransform.rotation,
                animalTargetTransform
            );

            currentCardAnimalInstance.transform.localPosition = Vector3.zero;
            currentCardAnimalInstance.transform.localRotation = Quaternion.identity;
            SetAnimalTapEnabled(false);
        }
        else
        {
            Debug.LogError("cardAnimalPrefab is NULL for " + data.displayName);
        }

        if (uiManager != null)
        {
            uiManager.SetInstruction("Step 2: Scan the penguin's home.");
            uiManager.SetFeedback("");
            uiManager.SetInteractionHint("Tap points to create a path.");
            uiManager.ShowInteractionHint(false);
            uiManager.SetPathButtonsVisible(false);
            uiManager.SetDecisionButtonsVisible(false);
            uiManager.SetCareButtonsVisible(false);
            uiManager.ShowScanner(false);
            uiManager.ShowCarePanel(false);
            uiManager.SetProgressStep(1);
        }

        SetState(GameState.WaitingForHome);
    }

    public void OnHomeTargetFound(string homeId)
    {
        ShowHome(homeId);

        if (homeId == "arctic")
            arcticDiscovered = true;
        else if (homeId == "desert")
            desertDiscovered = true;

        if (CurrentAnimalData == null)
        {
            UpdateDiscoveryFeedback();
            return;
        }

        if (CurrentState == GameState.WaitingForHome)
        {
            if (uiManager != null)
            {
                uiManager.SetInstruction("Step 3: Tap the habitat you want to choose.");
                uiManager.SetFeedback("Found " + GetHomeDisplayName(homeId) + ". Tap a habitat to select it.");
                uiManager.SetInteractionHint("Look at the AR habitat, then tap Arctic or Desert.");
                uiManager.ShowInteractionHint(true);
                uiManager.SetPathButtonsVisible(false);
                uiManager.SetDecisionButtonsVisible(false);
                uiManager.SetCareButtonsVisible(false);
                uiManager.ShowScanner(false);
                uiManager.SetProgressStep(2);
            }
        }
        else if (CurrentState != GameState.EnvironmentDecision)
        {
            UpdateDiscoveryFeedback();
        }
    }

    public void SelectHome(string homeId)
    {
        if (CurrentAnimalData == null)
        {
            if (uiManager != null)
                uiManager.SetFeedback("Scan the penguin first, then choose a home.");

            return;
        }

        if (CurrentState != GameState.WaitingForHome && CurrentState != GameState.EnvironmentDecision)
            return;

        bool isCorrectHome = homeId == CurrentAnimalData.correctHomeId;
        Transform selectedHomeTransform = ShowHome(homeId);

        CurrentHomeId = homeId;
        currentHomeTransform = isCorrectHome ? selectedHomeTransform : null;

        if (uiManager != null)
        {
            uiManager.SetInstruction("Selected: " + GetHomeDisplayName(homeId) + ". Is this a safe home?");
            uiManager.SetInteractionHint("Look at the scanner clues, then choose Safe Home or Not Safe.");
            uiManager.ShowInteractionHint(true);
            uiManager.SetPathButtonsVisible(false);
            uiManager.SetDecisionButtonsVisible(true);
            uiManager.SetCareButtonsVisible(false);
            uiManager.ShowEnvironmentScanner(homeId, isCorrectHome);
            uiManager.SetProgressStep(2);
        }

        SetState(GameState.EnvironmentDecision);
    }

    public void ChooseSafeHome()
    {
        if (CurrentState != GameState.EnvironmentDecision) return;
        if (CurrentAnimalData == null) return;

        if (CurrentHomeId == CurrentAnimalData.correctHomeId)
        {
            StartRescuePath();
        }
        else
        {
            if (uiManager != null)
            {
                uiManager.SetFeedback(CurrentAnimalData.wrongHomeMessage);
                uiManager.SetInstruction("Try another habitat card.");
                uiManager.SetDecisionButtonsVisible(false);
                uiManager.ShowScanner(false);
                uiManager.ShowInteractionHint(false);
                uiManager.SetProgressStep(1);
            }

            if (currentCardAnimalInstance != null)
            {
                AnimalReaction reaction = currentCardAnimalInstance.GetComponentInChildren<AnimalReaction>();

                if (reaction != null)
                    reaction.PlayWrongReaction();
            }

            CurrentHomeId = null;
            currentHomeTransform = null;
            SetState(GameState.WaitingForHome);
        }
    }

    public void ChooseNotSafe()
    {
        if (CurrentState != GameState.EnvironmentDecision) return;
        if (CurrentAnimalData == null) return;

        if (CurrentHomeId == CurrentAnimalData.correctHomeId)
        {
            if (uiManager != null)
            {
                uiManager.SetFeedback("Look again! The arctic has cold, fish, and ice for the penguin.");
            }

            return;
        }

        if (uiManager != null)
        {
            uiManager.SetInstruction("Good rescue thinking! Find the icy home.");
            uiManager.SetFeedback("That habitat is not safe for a penguin.");
            uiManager.SetDecisionButtonsVisible(false);
            uiManager.ShowScanner(false);
            uiManager.ShowInteractionHint(false);
            uiManager.SetProgressStep(1);
        }

        CurrentHomeId = null;
        currentHomeTransform = null;
        SetState(GameState.WaitingForHome);
    }

    public void GoHome()
    {
        if (CurrentState != GameState.PathBuilding) return;
        if (placementController == null || currentCardAnimalInstance == null) return;

        AnimalMoveToHome mover = currentCardAnimalInstance.GetComponentInChildren<AnimalMoveToHome>();
        if (mover == null) return;

        if (uiManager != null)
        {
            uiManager.SetFeedback("");
            uiManager.ShowInteractionHint(false);
            uiManager.SetPathButtonsVisible(false);
        }

        currentCardAnimalInstance.transform.SetParent(null, true);
        SetState(GameState.MovingHome);
        placementController.SendAnimalHome(mover);
    }

    public void ClearPath()
    {
        if (placementController != null)
            placementController.ClearPath();
    }

    public void OnAnimalArrivedHome()
    {
        if (uiManager != null)
        {
            uiManager.SetInstruction("Step 5: Help the penguin settle in.");
            uiManager.SetFeedback("Choose two things the penguin needs.");
            uiManager.SetInteractionHint("");
            uiManager.ShowInteractionHint(false);
            uiManager.SetPathButtonsVisible(false);
            uiManager.ShowScanner(false);
            uiManager.ShowCareChallenge(careScore);
            uiManager.SetProgressStep(4);
        }

        SetAnimalTapEnabled(false);
        SetState(GameState.CareChallenge);
    }

    public void ChooseCareItem(string itemId)
    {
        if (CurrentState != GameState.CareChallenge) return;

        if (itemId == "fish")
        {
            if (!gaveFish)
            {
                gaveFish = true;
                careScore++;
                PlayHappyReaction();
                SetCareFeedback("Great! Penguins eat fish.");
            }
            else
            {
                SetCareFeedback("You already gave the penguin fish. Pick one more helper item.");
            }
        }
        else if (itemId == "ice")
        {
            if (!addedIce)
            {
                addedIce = true;
                careScore++;
                PlayHappyReaction();
                SetCareFeedback("Nice! Ice gives the penguin a cold place to rest.");
            }
            else
            {
                SetCareFeedback("You already added ice. Pick another helpful item.");
            }
        }
        else if (itemId == "sun")
        {
            SetCareFeedback("Too warm! Penguins need cold places.");
            PlayWrongReaction();
        }
        else if (itemId == "cactus")
        {
            SetCareFeedback("A cactus belongs in the desert, not the arctic.");
            PlayWrongReaction();
        }

        if (uiManager != null)
            uiManager.UpdateCareMeter(careScore);

        if (careScore >= 2)
            CompleteMission();
    }

    public void ResetSession()
    {
        if (currentCardAnimalInstance != null)
        {
            Destroy(currentCardAnimalInstance);
            currentCardAnimalInstance = null;
        }

        CurrentAnimalData = null;
        CurrentAnimalId = null;
        CurrentHomeId = null;
        currentHomeTransform = null;
        animalScannedThisRound = false;
        careScore = 0;
        gaveFish = false;
        addedIce = false;
        arcticDiscovered = false;
        desertDiscovered = false;

        HideAllHomes();

        if (placementController != null)
        {
            placementController.StopPathBuilding();
            placementController.ClearPath();
        }

        if (uiManager != null)
        {
            uiManager.SetInstruction("Step 1: Scan an animal card.");
            uiManager.SetFeedback("");
            uiManager.SetInteractionHint("Tap points to create a path.");
            uiManager.ShowInteractionHint(false);
            uiManager.SetPathButtonsVisible(false);
            uiManager.SetDecisionButtonsVisible(false);
            uiManager.SetCareButtonsVisible(false);
            uiManager.ShowScanner(false);
            uiManager.ShowCarePanel(false);
            uiManager.SetProgressStep(0);
        }

        SetState(GameState.WaitingForAnimal);
    }

    private void WireButtons()
    {
        if (uiManager == null) return;

        if (uiManager.goHomeButton != null)
        {
            uiManager.goHomeButton.onClick.RemoveListener(GoHome);
            uiManager.goHomeButton.onClick.AddListener(GoHome);
        }

        if (uiManager.clearPathButton != null)
        {
            uiManager.clearPathButton.onClick.RemoveListener(ClearPath);
            uiManager.clearPathButton.onClick.AddListener(ClearPath);
        }

        if (uiManager.safeHomeButton != null)
        {
            uiManager.safeHomeButton.onClick.RemoveListener(ChooseSafeHome);
            uiManager.safeHomeButton.onClick.AddListener(ChooseSafeHome);
        }

        if (uiManager.notSafeButton != null)
        {
            uiManager.notSafeButton.onClick.RemoveListener(ChooseNotSafe);
            uiManager.notSafeButton.onClick.AddListener(ChooseNotSafe);
        }

        if (uiManager.fishButton != null)
        {
            uiManager.fishButton.onClick.RemoveAllListeners();
            uiManager.fishButton.onClick.AddListener(() => ChooseCareItem("fish"));
        }

        if (uiManager.iceButton != null)
        {
            uiManager.iceButton.onClick.RemoveAllListeners();
            uiManager.iceButton.onClick.AddListener(() => ChooseCareItem("ice"));
        }

        if (uiManager.sunButton != null)
        {
            uiManager.sunButton.onClick.RemoveAllListeners();
            uiManager.sunButton.onClick.AddListener(() => ChooseCareItem("sun"));
        }

        if (uiManager.cactusButton != null)
        {
            uiManager.cactusButton.onClick.RemoveAllListeners();
            uiManager.cactusButton.onClick.AddListener(() => ChooseCareItem("cactus"));
        }
    }

    private void HideAllHomes()
    {
        HomeTargetHandler[] allHomes = FindObjectsByType<HomeTargetHandler>();

        foreach (HomeTargetHandler home in allHomes)
        {
            home.HideHomeVisual();
        }
    }

    private Transform ShowHome(string homeId)
    {
        HomeTargetHandler[] allHomes = FindObjectsByType<HomeTargetHandler>();

        foreach (HomeTargetHandler home in allHomes)
        {
            if (home.homeId == homeId)
            {
                home.PrepareHomeVisualForCard();
                home.ShowHomeVisual();
                return home.homeVisual != null ? home.homeVisual.transform : home.transform;
            }
        }

        return null;
    }

    private void SetAnimalTapEnabled(bool enabled)
    {
        if (currentCardAnimalInstance == null) return;

        AnimalTapInteraction tap = currentCardAnimalInstance.GetComponentInChildren<AnimalTapInteraction>();
        if (tap != null)
            tap.SetInteractionEnabled(enabled);
    }

    private void StartRescuePath()
    {
        if (currentHomeTransform == null)
        {
            if (uiManager != null)
                uiManager.SetFeedback("Tap the correct home again so the penguin knows where to go.");

            SetState(GameState.WaitingForHome);
            return;
        }

        if (uiManager != null)
        {
            uiManager.SetInstruction("Step 4: Build a rescue path to the icy home.");
            uiManager.SetFeedback(CurrentAnimalData.correctHomeMessage);
            uiManager.SetInteractionHint("Tap between the penguin and home to add waypoint markers.");
            uiManager.ShowInteractionHint(true);
            uiManager.SetDecisionButtonsVisible(false);
            uiManager.SetPathButtonsVisible(true);
            uiManager.ShowScanner(false);
            uiManager.SetProgressStep(3);
        }

        if (placementController != null && currentCardAnimalInstance != null)
            placementController.BeginPathBuilding(currentCardAnimalInstance, currentHomeTransform);

        SetAnimalTapEnabled(false);
        SetState(GameState.PathBuilding);
    }

    private void SetCareFeedback(string message)
    {
        if (uiManager != null)
            uiManager.SetFeedback(message);
    }

    private void PlayHappyReaction()
    {
        PlayWrongReaction();
    }

    private void PlayWrongReaction()
    {
        if (currentCardAnimalInstance == null) return;

        AnimalReaction reaction = currentCardAnimalInstance.GetComponentInChildren<AnimalReaction>();
        if (reaction != null)
            reaction.PlayWrongReaction();
    }

    private void CompleteMission()
    {
        if (uiManager != null)
        {
            uiManager.SetInstruction("Mission complete!");
            uiManager.SetFeedback("Penguin fact: Penguins are birds that swim with flippers.");
            uiManager.SetCareButtonsVisible(false);
            uiManager.ShowCarePanel(false);
            uiManager.SetInteractionHint("Tap the penguin for a celebration run.");
            uiManager.ShowInteractionHint(true);
        }

        SetAnimalTapEnabled(true);
        SetState(GameState.Interaction);
    }

    private void UpdateDiscoveryFeedback()
    {
        if (uiManager == null || CurrentState == GameState.MovingHome || CurrentState == GameState.CareChallenge || CurrentState == GameState.Interaction) return;

        string discovered = "Found: ";
        discovered += animalScannedThisRound ? "Penguin" : "scan penguin";
        discovered += arcticDiscovered ? ", Arctic" : "";
        discovered += desertDiscovered ? ", Desert" : "";

        uiManager.SetFeedback(discovered);
    }

    private string GetHomeDisplayName(string homeId)
    {
        if (homeId == "arctic")
            return "Arctic";

        if (homeId == "desert")
            return "Desert";

        return string.IsNullOrEmpty(homeId) ? "Habitat" : homeId;
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("State changed to: " + CurrentState);
    }

    private AnimalData GetAnimalData(string animalId)
    {
        foreach (AnimalData data in animals)
        {
            if (data.animalId == animalId)
                return data;
        }

        return null;
    }
}
