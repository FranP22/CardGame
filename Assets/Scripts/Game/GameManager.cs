using Steamworks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public ulong clientId;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    [SerializeField]
    private int _deckSizeMin;
    [SerializeField]
    private int _deckSizeMax;
    [SerializeField]
    private int _equipSizeMin;
    [SerializeField]
    private int _equipSizeMax;
    [SerializeField]
    private int _fieldAllyAmount;

    public int equipSizeMin { get => _equipSizeMin; }
    public int equipSizeMax { get => _equipSizeMax; }
    public int deckSizeMin { get => _deckSizeMin; }
    public int deckSizeMax { get => _deckSizeMax; }
    public int fieldAllyAmount { get => _fieldAllyAmount; }
}
