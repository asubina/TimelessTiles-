using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
  [Header("Board Variables")]
  public int column;
  public int row;
  public int previousColumn;
  public int previousRow;
  public int targetX;
  public int targetY;
  public bool isMatched = false;

  private EndGameManager endGameManager;
  private HintManager hintManager;
  private FindMatches findmatches;
  private Board board;
  public GameObject otherDot;
  private Vector2 firstTouchPosition;
  private Vector2 finalTouchPosition;
  private Vector2 tempPosition;

  //bool WaitingFor2ndPress = false;
  //bool PressModeUsed = false;

  [Header("Swipe Stuff")]
  public float swipeAngle = 0;
  public float swipeResist = .5f;

  [Header("Powerup Stuff")]
  public bool isColourBomb;
  public bool isColumnBomb;
  public bool isRowBomb;
  public bool isAdjacentBomb;
  public GameObject adjacentMarker;
  public GameObject rowArrow;
  public GameObject columnArrow;
  public GameObject colourBomb;

  // Start is called before the first frame update
  void Start()
  {
    isColumnBomb = false;
    isRowBomb = false;
    isColourBomb = false;
    isAdjacentBomb = false;

    endGameManager = FindAnyObjectByType<EndGameManager>();
    hintManager = FindObjectOfType<HintManager>();
    board = FindObjectOfType<Board>();
    findmatches = FindObjectOfType<FindMatches>();
    //targetX = (int)transform.position.x;
    //targetY = (int)transform.position.y;
    //column = targetX;
    //row = targetY;
    //previousRow = row;
    //previousColumn = column;

  }

  //This is for testing and debug only.
  private void OnMouseOver()
  {
    if (Input.GetMouseButtonDown(1))
    {
      isAdjacentBomb = true;
      GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
      marker.transform.parent = this.transform;
    }

  }
  // Update is called once per frame
  void Update()
  {
    /*
     if (isMatched)
     {
       SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
       mySprite.color = new Color(1f, 1f, 1f, .2f);
     }
     */
    targetX = column;
    targetY = row;
    if (Mathf.Abs(targetX - transform.position.x) > .1)
    {
      //move towards the target
      tempPosition = new Vector2(targetX, transform.position.y);
      transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
      if (board.allDots[column, row] != this.gameObject)
      {
        board.allDots[column, row] = this.gameObject;
      }
      findmatches.FindAllMatches();
    }
    else
    {
      //directly set the position
      tempPosition = new Vector2(targetX, transform.position.y);
      transform.position = tempPosition;

    }
    if (Mathf.Abs(targetY - transform.position.y) > .1)
    {
      //move towards the target
      tempPosition = new Vector2(transform.position.x, targetY);
      transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
      if (board.allDots[column, row] != this.gameObject)
      {
        board.allDots[column, row] = this.gameObject;
      }
      findmatches.FindAllMatches();

    }
    else
    {
      //directly set the position
      tempPosition = new Vector2(transform.position.x, targetY);
      transform.position = tempPosition;

    }
  }

  public IEnumerator CheckMoveCo()
  {
    if (isColourBomb)
    {
      //This piece is a colour bomb and the other piece is the colour to destroy
      findmatches.MatchPiecesOfColour(otherDot.tag);
      isMatched = true;
    }
    else if (otherDot.GetComponent<Dot>().isColourBomb)
    {
      //The other piece is a colour bomb, and this piece has a colour to destroy 
      findmatches.MatchPiecesOfColour(this.gameObject.tag);
      otherDot.GetComponent<Dot>().isMatched = true;
    }
    yield return new WaitForSeconds(.5f);
    if (otherDot != null)
    {
      if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
      {
        otherDot.GetComponent<Dot>().row = row;
        otherDot.GetComponent<Dot>().column = column;
        row = previousRow;
        column = previousColumn;
        yield return new WaitForSeconds(.5f);
        board.currentDot = null;
        board.currentState = GameState.move;
      }
      else
      {
                if(endGameManager != null)
                {
                    if(endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
        board.DestroyMatches();
      }
      //otherDot = null;
    }
  }

    static Dot _LastPressedDot=null;
    static Dot _PrevPressedDot=null;

    static bool DotPressed(Dot dotObject)
    {
        _PrevPressedDot = _LastPressedDot;
        _LastPressedDot = dotObject;
        return (_LastPressedDot != _PrevPressedDot) && _PrevPressedDot != null ;

    }
    private void OnMouseDown()
  {
    //Destroy the hint 
    if (hintManager != null)
    {
      hintManager.DestroyHint();
    }
    if (board.currentState == GameState.move) 
    {
      firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }

  }

  private void OnMouseUp()
  {
    // if (PressModeUsed)
    // {
    //   PressModeUsed=false;
    //   WaitingFor2ndPress=false;
    //   return;
    // }
    if (board.currentState == GameState.move)
    {
      finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      bool swipe = CalculateAngle();
      if (!swipe)
      {
                if (DotPressed(this))
                {
                    //Debug.Log("activating alternative input");
                    _PrevPressedDot.firstTouchPosition = _PrevPressedDot.transform.position;
                    _PrevPressedDot.finalTouchPosition = _LastPressedDot.transform.position;
                    //Debug.Log("first: " + _PrevPressedDot.name);
                    //Debug.Log("second: " + _LastPressedDot.name);

                    //Debug.Log("A: "+ _PrevPressedDot.firstTouchPosition + " B: "+ _PrevPressedDot.finalTouchPosition);
                   if ( _PrevPressedDot.CalculateAngle2())
                    {
                        _PrevPressedDot = null;
                        _LastPressedDot = null;
                    }
                }

            }
        }
  }

  bool CalculateAngle()
  {
    if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
    {
      board.currentState = GameState.wait;
      swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
      MovePieces();
      board.currentDot = this;
      return true;
    }
    else
    {
      board.currentState = GameState.move;
      return false;
    }
  }
    bool CalculateAngle2()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > 0.5 || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > 0.5f)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentDot = this;
            return true;
        }
        else
        {
            board.currentState = GameState.move;
            return false;
        }
    }
    void MovePiecesActual(Vector2 direction)
  {
    otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
    previousRow = row;
    previousColumn = column;
    if (otherDot != null)
    {
      otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
      otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
      column += (int)direction.x;
      row += (int)direction.y;
      StartCoroutine(CheckMoveCo());
    }
    else
    {
      board.currentState = GameState.move;
    }
  }

  void MovePieces()
  {
    if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
    {
      //Right swipe
      /*
      otherDot = board.allDots[column + 1, row];
      previousRow = row;
      previousColumn = column;
      otherDot.GetComponent<Dot>().column -= 1;
      column += 1;
      StartCoroutine(CheckMoveCo());
      */
      MovePiecesActual(Vector2.right);
    }
    else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
    {
      //Up swipe
      /*
      otherDot = board.allDots[column, row + 1];
      previousRow = row;
      previousColumn = column;
      otherDot.GetComponent<Dot>().row -= 1;
      row += 1;
      StartCoroutine(CheckMoveCo());
      */
      MovePiecesActual(Vector2.up);
    }
    else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
    {
      //Left swipe
      /*
      otherDot = board.allDots[column - 1, row];
      previousRow = row;
      previousColumn = column;
      otherDot.GetComponent<Dot>().column += 1;
      column -= 1;
      StartCoroutine(CheckMoveCo());
      */
      MovePiecesActual(Vector2.left);
    }
    else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
    {
      //Down swipe
      /*
      otherDot = board.allDots[column, row - 1];
      previousRow = row;
      previousColumn = column;
      otherDot.GetComponent<Dot>().row += 1;
      row -= 1;
      StartCoroutine(CheckMoveCo());
      */
      MovePiecesActual(Vector2.down);
    }
    else
    {
      board.currentState = GameState.move;
    }
  }

  void FindMatches()
  {
    if (column > 0 && column < board.width - 1)
    {
      GameObject leftDot1 = board.allDots[column - 1, row];
      GameObject rightDot1 = board.allDots[column + 1, row];
      if (leftDot1 != null && rightDot1 != null)
      {
        if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
        {
          leftDot1.GetComponent<Dot>().isMatched = true;
          rightDot1.GetComponent<Dot>().isMatched = true;
          isMatched = true;
        }
      }
    }
    if (row > 0 && row < board.height - 1)
    {
      GameObject upDot1 = board.allDots[column, row + 1];
      GameObject downDot1 = board.allDots[column, row - 1];
      if (upDot1 != null && downDot1 != null)
      {
        if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
        {
          upDot1.GetComponent<Dot>().isMatched = true;
          downDot1.GetComponent<Dot>().isMatched = true;
          isMatched = true;
        }
      }
    }
  }

  public void MakeRowBomb()
  {
    isRowBomb = true;
    GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
    arrow.transform.parent = this.transform;
  }

  public void MakeColumnBomb()
  {
    isColumnBomb = true;
    GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
    arrow.transform.parent = this.transform;
  }

  public void MakeColourBomb()
  {
    isColourBomb = true;
    GameObject colour = Instantiate(colourBomb, transform.position, Quaternion.identity);
    colour.transform.parent = this.transform;
    this.gameObject.tag = "Color";
  }

  public void MakeAdjacentBomb()
  {
    isAdjacentBomb = true;
    GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
    marker.transform.parent = this.transform;
  }
}









