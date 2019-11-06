using UnityEngine;
using System;
using System.Collections.Generic;

public class SpyMaster : MonoBehaviour {

    #region Public Fields
    static public SpyMaster S;

    #endregion

    #region Private Fields
    private Spy _selectedSpy;
    [SerializeField] private List<Spy> _spies;
    [SerializeField] private int _actionPoints;
    [SerializeField] private int _money;
    [SerializeField] private int _information;
    [SerializeField] private int _xp;
    [SerializeField] private int _lordsOwned;
    #endregion

    #region Properties
    public List<Spy> spies
    {
        get { return _spies; }
        private set { _spies = value; }
    }
    public int actionPoints
    {
        get { return _actionPoints; }
        private set
        {
            if (value >= 0 && value <= ACTION_POINTS)
                _actionPoints = value;
            
        }
    }
    public int money
    {
        get { return _money; }
        private set
        {
            if (value >= 0)
                _money = value;
        }
    }
    public int information
    {
        get { return _information; }
        private set
        {
            if (value >= 0)
                _information = value;
        }
    }
    public int xp
    {
        get { return _xp; }
        private set
        {
            _xp = value;
        }
    }
    public int lordsOwned
    {
        get { return _lordsOwned; }
        private set
        {
            if (value >= 0 && value <= MAX_LORDS)
            {
                _lordsOwned = value;
            }
        }
    }
    #endregion

    #region Events & Delegates
    public static event Action<int> OnAPChanged;
    public static event Action<int> OnMoneyChanged;
    public static event Action<int> OnInformationChanged;
    public static event Action<int> OnXPChanged;
    public static event Action<int> OnLordsOwnedChanged;
    public static event Action OnSpyMasterTurnEnded;
    public static event Action<Vector3, string> OnFarmSucceeded;
    public static event Action OnAttemptSucceeded;
    public static event Action OnAttemptFailed;
    public static event Action OnWin;
    public static event Action OnLose;
    #endregion

    #region Constants
    public const int ACTION_POINTS = 6;
    public const int MAX_LORDS = 5;
    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        S = this;
    }

    private void OnEnable()
    {
        Spy.OnSpySelected += SetSelectedSpy;
        Spy.OnSpyMove += UseAP;
        Guard.OnGuardEndedMove += ResetActionPoints;
        Guard.OnSpyKilled += KillSpy;
    }

    private void Start()
    {
        actionPoints = ACTION_POINTS;
        if (OnAPChanged != null)
            OnAPChanged(actionPoints);
        money = 0;
        if (OnMoneyChanged != null)
            OnMoneyChanged(money);
        information = 0;
        if (OnInformationChanged != null)
            OnInformationChanged(information);
        xp = 0;
        if (OnXPChanged != null)
            OnXPChanged(xp);
        lordsOwned = 0;
        if (OnLordsOwnedChanged != null)
            OnLordsOwnedChanged(lordsOwned);
        spies = new List<Spy>(FindObjectsOfType<Spy>());
    }

    private void OnDisable()
    {
        Spy.OnSpySelected -= SetSelectedSpy;
        Spy.OnSpyMove -= UseAP;
        Guard.OnGuardEndedMove -= ResetActionPoints;
        Guard.OnSpyKilled -= KillSpy;
    }

#if UNITY_EDITOR
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.J))
        {
            actionPoints = 6;
            if (OnAPChanged != null)
                OnAPChanged(actionPoints);
        }
    }
#endif
#endregion

    #region Public Methods
    public void SetSelectedSpy(Spy selected)
    {
        _selectedSpy = selected;
    }

    public void UseAP(int amountToDecrease)
    {
        if (actionPoints > 0)
        {
            actionPoints -= amountToDecrease;
            if (OnAPChanged != null)
                OnAPChanged(actionPoints);
        }
        if (actionPoints == 0)
        {
            if (OnSpyMasterTurnEnded != null)
                OnSpyMasterTurnEnded();
        }
    }

    public void MoveSpy(Tile tileToMoveTo)
    {
        //handling tile selection
        if (!tileToMoveTo)
        {
            Debug.LogError("No Tile was selected");
            return;
        }
        //handling spy selection
        else if (!_selectedSpy)
        {
            Debug.LogError("No Spy was selected");
            return;
        }
        //making sure there are enough action points
        if (actionPoints > 0)
        {
            //AssignMoveButton(Map.S.selectedTile);
            StartCoroutine(_selectedSpy.Move(tileToMoveTo, actionPoints));
        }
    }

    public void BribeLord(LordCastle lc)
    {
        if (!lc.isOwned)
        {
            UseAP(1);
            if (_selectedSpy.Bribe(GetBribeAmount()))
            {
                //reduce money
                money = 0;
                if (OnMoneyChanged != null)
                    OnMoneyChanged(money);

                lc.ToggleOwned(true);
                lc.ToggleHalo(false);
                lordsOwned++;
                if (OnLordsOwnedChanged != null)
                    OnLordsOwnedChanged(lordsOwned);
                xp++;
                if (OnXPChanged != null)
                    OnXPChanged(xp);
                UIManager.S.DisplayMessage("Bribe Succeeded");
                //raise suspicion
                if (OnAttemptSucceeded != null)
                    OnAttemptSucceeded();
                if(lordsOwned == 5)
                {
                    if (OnWin != null)
                        OnWin();
                }
            }
            else
            {
                UIManager.S.DisplayMessage("Bribe Failed");
                //raise suspicion
                if (OnAttemptFailed != null)
                    OnAttemptFailed();
            }
        }
        else
            UIManager.S.DisplayMessage("Lord already owned");
    }

    public void BlackmailLord(LordCastle lc)
    {
        if (!lc.isOwned)
        {
            UseAP(1);
            if (_selectedSpy.Blackmail(GetBlackmailStrength()))
            {
                //reduce information
                information = 0;
                if (OnInformationChanged != null)
                    OnInformationChanged(information);
                lc.ToggleOwned(true);
                lc.ToggleHalo(false);
                lordsOwned++;
                if (OnLordsOwnedChanged != null)
                    OnLordsOwnedChanged(lordsOwned);
                xp++;
                if (OnXPChanged != null)
                    OnXPChanged(xp);
                UIManager.S.DisplayMessage("Blackmail Succeeded");
                //raise suspicion
                if (OnAttemptSucceeded != null)
                    OnAttemptSucceeded();
                if (lordsOwned == 5)
                {
                    if (OnWin != null)
                        OnWin();
                }
            }
            else
            {
                //raise suspicion
                UIManager.S.DisplayMessage("Blackmail Failed");
                //raise suspicion
                if (OnAttemptFailed != null)
                    OnAttemptFailed();
            }
        }
        else
            UIManager.S.DisplayMessage("Lord already owned");
    }

    public int GetBribeAmount()
    {
        return money + xp;
    }

    public int GetBlackmailStrength()
    {
        return information + xp;
    }

    public void Farm(Farmable f)
    {
        //Debug.Log("Farm" + f);
        if (f.resourceType == ResourceType.money)
        {
            int moneyGained = _selectedSpy.Farm();
            money += moneyGained;
            if (OnMoneyChanged != null)
                OnMoneyChanged(money);
            if (OnFarmSucceeded != null)
                OnFarmSucceeded(f.GetComponent<Collider>().bounds.center, string.Format("+{0}\nMoney", moneyGained));
        }
        else if (f.resourceType == ResourceType.information)
        {
            int informationGained = _selectedSpy.Farm();
            information += informationGained;
            if (OnInformationChanged != null)
                OnInformationChanged(information);
            if (OnFarmSucceeded != null)
                OnFarmSucceeded(f.GetComponent<Collider>().bounds.center, string.Format("+{0}\nInformation", informationGained));
        }
            
        else
            Debug.LogError("Unknown resource type");
        UseAP(1);
    }

    public Spy GetSelectedSpy()
    {
        return _selectedSpy;
    }

    public int GetAPAmount()
    {
        return actionPoints;
    }

    public void ResetActionPoints()
    {
        actionPoints = 6;
        if (OnAPChanged != null)
            OnAPChanged(actionPoints);
    }

    public Spy GetSpyByMapPosition(Vector2Int mapPos)
    {
        for (int i = 0; i < spies.Count; i++)
        {
            if (spies[i].mapPosition == mapPos)
                return spies[i];
        }
        return null;
    }

    public void KillSpy(Spy s)
    {
        spies.Remove(s);
        Destroy(s.gameObject);
        if(spies.Count == 0)
        {
            if (OnLose != null)
                OnLose();
        }
    }

    public Spy GetRandomSpy()
    {
        return spies[UnityEngine.Random.Range(0, spies.Count)];
    }
    #endregion

}
