using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TMP_Text scoretext;
    public TMP_Text hstext;
    public Piece[] pieces; // All piece prefabs
    public Transform[] blockSpawns; // Where to spawn them in UI
    public bool graboffset = false;
    public Toggle grabToggle;
    int score = 0;
    int highScore = 0;
    private List<Piece> currPieces = new List<Piece>();
    private void Awake()
    {
        instance = this;
        //Screen.SetResolution(540, 960, false );
    }
    private void Start()
    {
        grabToggle.onValueChanged.AddListener(OnToggleValueChanged);
        if (PlayerPrefs.GetInt("GrabOffset", 0) == 1)
        {
            grabToggle.isOn = true;
            graboffset = true;
        }
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        hstext.text = highScore.ToString();
    }
    public void SpawnBlocks()
    {
        foreach (Transform t in blockSpawns)
        {
            Piece piece = GetValidPiece();
            if (piece != null)
            {
                piece.transform.position = t.position;
                currPieces.Add(piece);
            }
            else
            {
                Debug.Log("Game Over! No valid piece can fit.");
                // TODO: Handle game over state here
            }
        }
    }

    Piece GetValidPiece()
    {
        List<Piece> allShapes = new List<Piece>(pieces);
        Shuffle(allShapes);

        foreach (Piece shapePrefab in allShapes)
        {
            Piece temp = Instantiate(shapePrefab, transform);
            temp.SetRandomRotation();
            temp.SetRandomColor();

            if (GridManager.instance.CanPieceFit(temp))
            {
                return temp; // Found a valid piece
            }

            Destroy(temp.gameObject);
        }

        return null; // Nothing fits game over
    }

    // Fisher-Yates shuffle
    void Shuffle(List<Piece> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Piece temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void CheckBlocks(Piece checkPiece)
    {
        currPieces.Remove(checkPiece);
        if (currPieces.Count <= 0)
        {
            SpawnBlocks();
            return;
        }
        else
        {
            int dontfit = 0;
            foreach (Piece piece in currPieces)
            {
                if (!GridManager.instance.CanPieceFit(piece))
                {
                    dontfit++;
                }

            }
            if (dontfit == currPieces.Count)
            {
                GameOver();
            }
        } 
    }

    public void addScore(int add)
    {
        score += add;
        scoretext.text = score.ToString();

        if (score > highScore)
        {
            highScore = score;
            hstext.text = highScore.ToString();
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnToggleValueChanged(bool newValue)
    {
        graboffset = newValue;
        if (newValue)
        {
            PlayerPrefs.SetInt("GrabOffset", 1);
        }
        else
        {
            PlayerPrefs.SetInt("GrabOffset", 0);
        }
        PlayerPrefs.Save();
    }
}