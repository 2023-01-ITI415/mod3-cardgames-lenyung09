using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Dynamic")]
    public char suit;
    public int rank;
    public Color color = Color.black;
    public string colS = "Black";
    public GameObject back;
    public JsonCard def;

    public List<GameObject> decoGOs = new List<GameObject>();

    public List<GameObject> pipGOs = new List<GameObject>();

    /// <summary>
    /// Creates this Card’s visuals based on suit and rank.
    /// Note that this method assumes it will be passed a valid suit and rank.
    /// </summary>
    /// <param name="eSuit">The suit of the card (e.g., ’C’)</param> > /// <param name="eRank">The rank from 1 to13</param>
    /// <returns></returns>
    public void Init(char eSuit, int eRank, int eFlag, bool startFaceUp = true)
    {
        gameObject.name = name = eSuit.ToString() + eRank;
        suit = eSuit;
        rank = eRank;
        if (suit == 'D' || suit == 'H')
        {
            colS = "Red";
            color = Color.red;
        }
        def = JsonParseDeck.GET_CARD_DEF(rank);
        AddDecorators();
        AddPips();
        AddFace();

        AddBack(eFlag);

        faceUp = startFaceUp;
    }

    /// <summary>
    /// Shortcut for setting transform.localPosition.
    /// </summary>
    /// <param name="v"></param>
    public virtual void SetLocalPos(Vector3 v)
    {
        transform.localPosition = v;
    }

    private Sprite _tSprite = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSRend = null;

    private Quaternion _flipRot = Quaternion.Euler(0, 0, 180); // d

    /// <summary>
    /// Adds the decorators to the top-left and bottom-right of each card.
    /// Decorators are the suit and rank in the corners ofmeach card.
    /// </summary>
    private void AddDecorators()
    {
        foreach (JsonPip pip in JsonParseDeck.DECORATORS)
        {
            if (pip.type == "suit")
            {
                _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
                _tSRend = _tGO.GetComponent<SpriteRenderer>();
                _tSRend.sprite = CardSpritesSO.SUITS[suit];
            }
            else
            {
                _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
                _tSRend = _tGO.GetComponent<SpriteRenderer>();
                _tSRend.sprite = CardSpritesSO.RANKS[rank];
                _tSRend.color = color;
            }

            _tSRend.sortingOrder = 1;
            _tGO.transform.localPosition = pip.loc;
            if (pip.flip)
                _tGO.transform.rotation = _flipRot;

            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            _tGO.name = pip.type;
            decoGOs.Add(_tGO);
        }
    }

    /// <summary>
    /// Adds pips to the front of all cards from A to 10
    /// </summary>
    private void AddPips()
    {
        int pipNum = 0;
        foreach (JsonPip pip in def.pips)
        {
            _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
            _tGO.transform.localPosition = pip.loc;
            if (pip.flip)
                _tGO.transform.rotation = _flipRot;
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            _tGO.name = "pip_" + pipNum++;

            _tSRend = _tGO.GetComponent<SpriteRenderer>();
            _tSRend.sprite = CardSpritesSO.SUITS[suit];
            _tSRend.sortingOrder = 1;
            // Add this to the Card’s list of pips
            pipGOs.Add(_tGO);
        }
    }

    /// <summary>
    /// Adds the face sprite for card ranks 11 to 13
    /// </summary>
    private void AddFace()
    {
        if (def.face == "")
            return;

        string faceName = def.face + suit;
        _tSprite = CardSpritesSO.GET_FACE(faceName);
        if (_tSprite == null)
        {
            Debug.LogError("Face sprite " + faceName + " not found.");
            return;
        }

        _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform); // d
        _tSRend = _tGO.GetComponent<SpriteRenderer>();
        _tSRend.sprite = _tSprite; // Assign the face Sprite to _tSRend
        _tSRend.sortingOrder = 1; // Set the sortingOrder
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = faceName;
    }

    /// <summary>
    /// Property to show and hide the back of the card.
    /// </summary>
    public bool faceUp
    {
        get { return (!back.activeSelf); }
        // a
        set { back.SetActive(!value); }
    }

    /// <summary> > /// Adds a back to the card so that renders on top of everything else
    /// </summary>
    private void AddBack(int flag)
    {
        _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
        _tSRend = _tGO.GetComponent<SpriteRenderer>();

        // Gold Card added
        if (flag == 1)
        {
            _tSRend.sprite = CardSpritesSO.BACKGOLD;
        }
        // Silver Card added
        else if (flag == 2)
        {
            _tSRend.sprite = CardSpritesSO.BACKSILVER;
        }
        else
        {
            _tSRend.sprite = CardSpritesSO.BACK;
        }

        //_tSRend.sprite = CardSpritesSO.BACK;

        _tGO.transform.localPosition = Vector3.zero;
        _tSRend.sortingOrder = 2;

        _tGO.name = "back";
        back = _tGO;
    }

    private SpriteRenderer[] spriteRenderers;

    /// <summary>
    /// Gather all SpriteRenderers on this and its children into an array.
    /// </summary>
    void PopulateSpriteRenderers()
    {
        // If we’ve already populated spriteRenderers, just return. // a
        if (spriteRenderers != null)
            return;
        // GetComponentsInChildren is slow, but we’re only doing it once per card
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Moves the Sprites of this Card into a specified sorting layer
    /// </summary>
    /// <param name="layerName">The name of the layer to move to</param>
    public void SetSpriteSortingLayer(string layerName)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer srend in spriteRenderers)
        {
            srend.sortingLayerName = layerName;
        }
    }

    /// <summary>
    /// Sets the sortingOrder of the Sprites on this Card. This allows multiple
    /// Cards to be in the same sorting layer and stilloverlap properly, and
    /// it is used by both the draw and discard piles.
    /// </summary>
    /// <param name="sOrd">The sortingOrder for the face of the Card</param>
    public void SetSortingOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer srend in spriteRenderers)
        {
            if (srend.gameObject == this.gameObject)
            {
                srend.sortingOrder = sOrd; // Set its order to sOrd
            }
            else if (srend.gameObject.name == "back") { }
            else
            {
                srend.sortingOrder = sOrd + 1;
            }
        }
    }

    // Virtual methods can be overridden by subclass methods with the same name
    virtual public void OnMouseUpAsButton()
    {
        print(name); // When clicked, this outputs the card name
    }

    /// <summary>
    /// Return true if the two cards are adjacent in rank.
    /// If wrap is true, Ace and King are adjacent.
    /// </summary>
    /// <param name="otherCard">The card to compare to</param>
    /// <param name="wrap">If true (default) Ace and King wrap</param>
    /// <returns>true, if the cards are adjacent</returns>
    public bool AdjacentTo(Card otherCard, bool wrap = true)
    {
        // If either card is face-down, it’s not a valid match.
        if (!faceUp || !otherCard.faceUp)
            return (false);

        // If the ranks are 1 apart, they are adjacent
        if (Mathf.Abs(rank - otherCard.rank) == 1)
            return (true);
        if (wrap)
        {
            if (rank == 1 && otherCard.rank == 13)
                return (true);
            if (rank == 13 && otherCard.rank == 1)
                return (true);
        }

        return (false);
    }
}
