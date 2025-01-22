using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    public string gameMode;
    public GameObject cross, nought, bar;

    public Text Instructions, player2Name;

    public enum Seed { EMPTY, CROSS, NOUGHT };

    public Seed Turn;

    public GameObject[] allSpawns = new GameObject[9];

    public Seed[] player = new Seed[9];

    Vector2 pos1, pos2;

    private void Awake()
    {
        GameObject persistantObj = GameObject.FindGameObjectWithTag("PersistantObj") as GameObject;
        gameMode = persistantObj.GetComponent<PersistanceScript>().gameMode;
        Destroy(persistantObj);

        if (gameMode == "vscpu")
            player2Name.text = "CPU Player";
        else
            player2Name.text = "2nd Player";
        Turn = Seed.CROSS;

        Instructions.text = "Turn: 1st Player";

        for (int i = 0; i < 9; i++)
            player[i] = Seed.EMPTY;
    }

    public void Spawn(GameObject emptycell, int id)
    {
        if (Turn == Seed.CROSS)
        {
            allSpawns[id] = Instantiate(cross, emptycell.transform.position, Quaternion.identity);
            player[id] = Turn;

            if (Won(Turn))
            {
                Turn = Seed.EMPTY;
                Instructions.text = "Player-1 has won!!!";
                float slope = calculateSlope();
                Instantiate(bar, calculateCenter(), Quaternion.Euler(0, 0, slope));
            }
            else
            {
                Turn = Seed.NOUGHT;
                Instructions.text = "Turn: 2nd Player";
            }
        }
        else if (Turn == Seed.NOUGHT && gameMode == "2player")
        {
            allSpawns[id] = Instantiate(nought, emptycell.transform.position, Quaternion.identity);
            player[id] = Turn;

            if (Won(Turn))
            {
                Turn = Seed.EMPTY;
                Instructions.text = "Player-2 has won!!!";
                float slope = calculateSlope();
                Instantiate(bar, calculateCenter(), Quaternion.Euler(0, 0, slope));
            }
            else
            {
                Turn = Seed.CROSS;
                Instructions.text = "Turn: 1st Player";
            }
        }

        if (Turn == Seed.NOUGHT && gameMode == "vscpu")
        {
            int bestScore = -1, bestPos = -1, score;

            for (int i = 0; i < 9; i++)
            {
                if (player[i] == Seed.EMPTY)
                {
                    player[i] = Seed.NOUGHT;
                    score = minimax(Seed.CROSS, player, -1000, +1000);
                    player[i] = Seed.EMPTY;

                    if (bestScore < score)
                    {
                        bestScore = score;
                        bestPos = i;
                    }
                }
            }

            if (bestPos > -1)
            {
                allSpawns[bestPos] = Instantiate(nought, allSpawns[bestPos].transform.position, Quaternion.identity);
                player[bestPos] = Turn;
            }

            if (Won(Turn))
            {
                Turn = Seed.EMPTY;
                Instructions.text = "Player-2 has won!!!";
                float slope = calculateSlope();
                Instantiate(bar, calculateCenter(), Quaternion.Euler(0, 0, slope));
            }
            else
            {
                Turn = Seed.CROSS;
                Instructions.text = "Turn: 1st Player";
            }
        }

        if (IsDraw())
        {
            Turn = Seed.EMPTY;
            Instructions.text = "It is a draw!!";
        }

        Destroy(emptycell);
    }

    bool IsAnyEmpty()
    {
        bool empty = false;
        for (int i = 0; i < 9; i++)
        {
            if (player[i] == Seed.EMPTY)
            {
                empty = true;
                break;
            }
        }
        return empty;
    }

    bool Won(Seed currPlayer)
    {
        bool hasWon = false;

        int[,] allConditions = new int[8, 3] { {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
                                                {0, 3, 6}, {1, 4, 7}, {2, 5, 8},
                                                {0, 4, 8}, {2, 4, 6} };

        for (int i = 0; i < 8; i++)
        {
            if (player[allConditions[i, 0]] == currPlayer &
                player[allConditions[i, 1]] == currPlayer &
                player[allConditions[i, 2]] == currPlayer)
            {
                hasWon = true;
                pos1 = allSpawns[allConditions[i, 0]].transform.position;
                pos2 = allSpawns[allConditions[i, 2]].transform.position;
                break;
            }
        }
        return hasWon;
    }

    bool IsDraw()
    {
        bool player1Won, player2Won, anyEmpty;
        player1Won = Won(Seed.CROSS);
        player2Won = Won(Seed.NOUGHT);
        anyEmpty = IsAnyEmpty();
        bool isDraw = false;

        if (player1Won == false & player2Won == false & anyEmpty == false)
            isDraw = true;

        return isDraw;
    }

    Vector2 calculateCenter()
    {
        float x = (pos1.x + pos2.x) / 2,
            y = (pos1.y + pos2.y) / 2;

        return new Vector2(x, y);
    }

    float calculateSlope()
    {
        float slope;

        if (pos1.x == pos2.x)
            slope = 0.0f;
        else if (pos1.y == pos2.y)
            slope = 90.0f;
        else if (pos1.x > 0.0f)
            slope = -45.0f;
        else
            slope = 45.0f;

        return slope;
    }

    int minimax(Seed currPlayer, Seed[] board, int alpha, int beta)
    {
        if (IsDraw())
            return 0;

        if (Won(Seed.NOUGHT))
            return +1;

        if (Won(Seed.CROSS))
            return -1;

        int score;

        if(currPlayer == Seed.NOUGHT)
        {
            for(int i=0; i < 9; i++)
            {
                if(board[i] == Seed.EMPTY)
                {
                    board[i] = Seed.NOUGHT;
                    score = minimax(Seed.CROSS, board, alpha, beta);
                    board[i] = Seed.EMPTY;

                    if (score > alpha)
                        alpha = score;

                    if (alpha > beta)
                        break;
                }
            }

            return alpha;
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == Seed.EMPTY)
                {
                    board[i] = Seed.CROSS;
                    score = minimax(Seed.NOUGHT, board, alpha, beta);
                    board[i] = Seed.EMPTY;

                    if (score < beta)
                        beta = score;

                    if (alpha > beta)
                        break;
                }
            }

            return beta;
        }
    }
}