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

    [Header("Animal Setup")]
    public AnimalData[] animals;

    [Header("Scene References")]
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

    private bool farmDiscovered;
    private bool arcticDiscovered;
    private bool forestDiscovered;
    private bool jungleDiscovered;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        if (animalScannedThisRound)
            return;

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
            uiManager.SetInstruction("Step 2: Scan the correct habitat for the " + data.displayName + ".");
            uiManager.SetFeedback("");
            uiManager.SetInteractionHint("Scan a habitat card.");
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
        MarkHomeDiscovered(homeId);

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
                uiManager.SetFeedback("Found " + GetHomeDisplayName(homeId) + ".\nTap a habitat to select it.");
                uiManager.SetInteractionHint("Look at the AR habitat, then tap it to select.");
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
                uiManager.SetFeedback("Scan an animal first, then choose a habitat.");

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
            uiManager.SetInstruction("Selected: " + GetHomeDisplayName(homeId) + ".\nIs this a safe home?");
            uiManager.SetInteractionHint("Choose Safe Home or Not Safe.");
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
        if (CurrentState != GameState.EnvironmentDecision)
            return;

        if (CurrentAnimalData == null)
            return;

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

            PlayWrongReaction();

            CurrentHomeId = null;
            currentHomeTransform = null;

            SetState(GameState.WaitingForHome);
        }
    }

    public void ChooseNotSafe()
    {
        if (CurrentState != GameState.EnvironmentDecision)
            return;

        if (CurrentAnimalData == null)
            return;

        if (CurrentHomeId == CurrentAnimalData.correctHomeId)
        {
            if (uiManager != null)
            {
                uiManager.SetFeedback("Look again! The " + GetHomeDisplayName(CurrentHomeId) +
                                      " is the correct habitat for the " + CurrentAnimalData.displayName + ".");
            }

            return;
        }

        if (uiManager != null)
        {
            uiManager.SetInstruction("Good thinking! Find the correct habitat for the " + CurrentAnimalData.displayName + ".");
            uiManager.SetFeedback("That habitat is not safe for the " + CurrentAnimalData.displayName + ".");
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
        if (CurrentState != GameState.PathBuilding)
            return;

        if (placementController == null || currentCardAnimalInstance == null)
            return;

        AnimalMoveToHome mover = currentCardAnimalInstance.GetComponentInChildren<AnimalMoveToHome>();

        if (mover == null)
            return;

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
            uiManager.SetInstruction("Mission complete!");
            uiManager.SetFeedback(CurrentAnimalData.displayName + " made it safely to the " +
                                  GetHomeDisplayName(CurrentAnimalData.correctHomeId) + " habitat.");
            uiManager.SetInteractionHint("Tap the animal for a celebration.");
            uiManager.ShowInteractionHint(true);
            uiManager.SetPathButtonsVisible(false);
            uiManager.SetDecisionButtonsVisible(false);
            uiManager.SetCareButtonsVisible(false);
            uiManager.ShowScanner(false);
            uiManager.ShowCarePanel(false);
            uiManager.SetProgressStep(4);
        }

        SetAnimalTapEnabled(true);
        SetState(GameState.Interaction);
    }

    public void ChooseCareItem(string itemId)
    {
        // This method is kept so your old UI buttons do not break.
        // You can redesign the care challenge later for each animal.
        if (uiManager != null)
            uiManager.SetFeedback("Care challenge can be customized later for each animal.");
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

        farmDiscovered = false;
        arcticDiscovered = false;
        forestDiscovered = false;
        jungleDiscovered = false;

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
            uiManager.SetInteractionHint("Scan one of the animal cards.");
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
        if (uiManager == null)
            return;

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
    }

    private void HideAllHomes()
    {
        HomeTargetHandler[] allHomes = FindObjectsByType<HomeTargetHandler>(FindObjectsSortMode.None);

        foreach (HomeTargetHandler home in allHomes)
        {
            home.HideHomeVisual();
        }
    }

    private Transform ShowHome(string homeId)
    {
        HomeTargetHandler[] allHomes = FindObjectsByType<HomeTargetHandler>(FindObjectsSortMode.None);

        foreach (HomeTargetHandler home in allHomes)
        {
            if (home.homeId == homeId)
            {
                home.PrepareHomeVisualForCard();
                home.ShowHomeVisual();

                if (home.homeVisual != null)
                    return home.homeVisual.transform;

                return home.transform;
            }
        }

        return null;
    }

    private void SetAnimalTapEnabled(bool enabled)
    {
        if (currentCardAnimalInstance == null)
            return;

        AnimalTapInteraction tap = currentCardAnimalInstance.GetComponentInChildren<AnimalTapInteraction>();

        if (tap != null)
            tap.SetInteractionEnabled(enabled);
    }

    private void StartRescuePath()
    {
        if (currentHomeTransform == null)
        {
            if (uiManager != null)
                uiManager.SetFeedback("Tap the correct home again so the animal knows where to go.");

            SetState(GameState.WaitingForHome);
            return;
        }

        if (uiManager != null)
        {
            uiManager.SetInstruction("Step 4: Build a rescue path to the " +
                                     GetHomeDisplayName(CurrentAnimalData.correctHomeId) + ".");
            uiManager.SetFeedback(CurrentAnimalData.correctHomeMessage);
            uiManager.SetInteractionHint("Tap between the animal and habitat to add waypoint markers.");
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

    private void PlayWrongReaction()
    {
        if (currentCardAnimalInstance == null)
            return;

        AnimalReaction reaction = currentCardAnimalInstance.GetComponentInChildren<AnimalReaction>();

        if (reaction != null)
            reaction.PlayWrongReaction();
    }

    private void MarkHomeDiscovered(string homeId)
    {
        if (homeId == "farm")
            farmDiscovered = true;
        else if (homeId == "arctic")
            arcticDiscovered = true;
        else if (homeId == "forest")
            forestDiscovered = true;
        else if (homeId == "jungle")
            jungleDiscovered = true;
    }

    private void UpdateDiscoveryFeedback()
    {
        if (uiManager == null)
            return;

        if (CurrentState == GameState.MovingHome ||
            CurrentState == GameState.CareChallenge ||
            CurrentState == GameState.Interaction)
            return;

        string discovered = "Found: ";

        if (animalScannedThisRound && CurrentAnimalData != null)
            discovered += CurrentAnimalData.displayName;
        else
            discovered += "scan an animal";

        if (farmDiscovered)
            discovered += ", Farm";

        if (arcticDiscovered)
            discovered += ", Arctic";

        if (forestDiscovered)
            discovered += ", Forest";

        if (jungleDiscovered)
            discovered += ", Jungle";

        uiManager.SetFeedback(discovered);
    }

    private string GetHomeDisplayName(string homeId)
    {
        if (homeId == "farm")
            return "Farm";

        if (homeId == "arctic")
            return "Arctic";

        if (homeId == "forest")
            return "Forest";

        if (homeId == "jungle")
            return "Jungle";

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