﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chess_Boss : BaseChess
{
    public static event GameOverEventHandlerWithParam BossKilledEventWithParam;
    public static event GameOverEventHandler BossKilledEvent;

    public override void Awake()
    {
        if (gameObject.tag == "Red")
            chineseChessName = "帅";
        else
            chineseChessName = "将";
        chessName = "Boss";
        base.Awake();
    }

    void Start()
    {

    }

    public override void Update()
    {
        base.Update();
    }
    /// <summary>
    /// 检测是否被将军
    /// </summary>
    public void DetectBeAttacked()
    {
        Vector2 currentPos = GameCache.Chess2Vector[gameObject];
        List<Vector2> enemyPoints;
        if (gameObject.tag == "Red")
            enemyPoints = GameUtil.getTeamMovePoints(PoolManager.work_List, "Black", GameCache.Chess2Vector, GameCache.Vector2Chess);
        else
            enemyPoints = GameUtil.getTeamMovePoints(PoolManager.work_List, "Red", GameCache.Chess2Vector, GameCache.Vector2Chess);
        foreach(Vector2 point in enemyPoints)
        {
            if (GameUtil.CompareVector2(currentPos, point))     //敌军可走点有此boss位置点，则说明在将军
            {
                Debug.Log("将军！！");
                chessSituationState = ChessSituationState.BeAttacked;
                //检测是否被将死
                //这时就要完善假设走步的逻辑了，处理起来比较庞大。
                break;
            }
        }
    }
    /// <summary>
    /// 被将死，换言之，无论怎么走都无法改变被将军的状态。
    /// </summary>
    public bool NoWayOut()
    {
        //若无论下一步怎么走都还无法避免正在被将军的状态，那就认为是被将死
        if (chessSituationState == ChessSituationState.BeAttacked)
        {

        }

        return false;
    }

    /// <summary>
    /// 重写被点击方法，规定将帅不可以加属性
    /// </summary>
    public override void ChesseClicked()
    {
        if (GameController.playing == Playing.BlackAdding || GameController.playing == Playing.RedAdding)
            return;
        base.ChesseClicked();
    }

    public override List<Vector2> CanMovePoints(Dictionary<GameObject, Vector2> chess2Vector, Dictionary<Vector2, GameObject> vector2Chess)
    {
        Vector2 currentPos = chess2Vector[gameObject];
        List<Vector2> canMovePoints = new List<Vector2>();

        if (gameObject.tag == "Red")
        {
            if (currentPos.y <= 1)  //可向上走
            {
                Vector2 value = new Vector2(currentPos.x, currentPos.y + 1);
                JudgeMovePoint(value, canMovePoints, gameObject, chess2Vector, vector2Chess);
            }
            if (currentPos.y >= 1)  //可向下走
            {
                Vector2 value = new Vector2(currentPos.x, currentPos.y - 1);
                JudgeMovePoint(value, canMovePoints, gameObject, chess2Vector, vector2Chess);
            }
        }
        if (gameObject.tag == "Black")
        {
            if (currentPos.y <= 8)  //可向上走
            {
                Vector2 value = new Vector2(currentPos.x, currentPos.y + 1);
                JudgeMovePoint(value, canMovePoints, gameObject, chess2Vector, vector2Chess);
            }
            if (currentPos.y >= 8)  //可向下走
            {
                Vector2 value = new Vector2(currentPos.x, currentPos.y - 1);
                JudgeMovePoint(value, canMovePoints, gameObject, chess2Vector, vector2Chess);
            }
        }

        if (currentPos.x <= 4)  //可向右走
        {
            Vector2 value = new Vector2(currentPos.x + 1, currentPos.y);
            JudgeMovePoint(value, canMovePoints, gameObject, chess2Vector, vector2Chess);
        }

        if (currentPos.x >= 4)  //可向左走
        {
            Vector2 value = new Vector2(currentPos.x - 1, currentPos.y);
            JudgeMovePoint(value, canMovePoints, gameObject, chess2Vector, vector2Chess);
        }

        return canMovePoints;
    }

    /// <summary>
    /// 帅/将专属判断是否可以走这个点
    /// </summary>
    /// <param name="value"></param>
    void JudgeMovePoint(Vector2 value, List<Vector2> canMovePoints, GameObject self, Dictionary<GameObject, Vector2> chess2Vector, Dictionary<Vector2, GameObject> vector2Chess)
    {
        GameObject enemyBoss;
        if (self == createManager.GetRedBoss())
            enemyBoss = createManager.GetBlackBoss();
        else
            enemyBoss = createManager.GetRedBoss();

        if (GameUtil.IsInChessBoard(value))
        {
            //不管value处有没有棋子，先判断value是否和敌方公照面(是否x坐标相同)
            bool existOtherChessOnSame_X_Axis = false;   //在公想要走的位置和对面公的位置之间是否有其他棋子
            //若想要走的位置和对面公同一条竖线，那要判断是否照面
            if (value.x == chess2Vector[enemyBoss].x)
            {
                float enemyBoss_Y = chess2Vector[enemyBoss].y;
                if (enemyBoss == createManager.GetBlackBoss())
                {
                    for (int i = (int)value.y + 1; i < enemyBoss_Y; i++)
                    {
                        if (vector2Chess.ContainsKey(new Vector2(value.x, i)))
                            existOtherChessOnSame_X_Axis = true;
                    }
                }
                else if (enemyBoss == createManager.GetRedBoss())
                {
                    for (int i = (int)value.y - 1; i > enemyBoss_Y; i--)
                    {
                        if (vector2Chess.ContainsKey(new Vector2(value.x, i)))
                            existOtherChessOnSame_X_Axis = true;
                    }
                }
                if (existOtherChessOnSame_X_Axis)
                {
                    //判断value位置是否有棋子
                    if (vector2Chess.ContainsKey(value))
                    {
                        GameObject otherChess = vector2Chess[value];
                        //判断value位置的棋子阵营
                        if (otherChess.tag != gameObject.tag)
                            canMovePoints.Add(value);
                    }
                    else
                        canMovePoints.Add(value);
                }
            }
            else
            {
                //判断value位置是否有棋子
                if (vector2Chess.ContainsKey(value))
                {
                    GameObject otherChess = vector2Chess[value];
                    //判断value位置的棋子阵营
                    if (otherChess.tag != gameObject.tag)
                        canMovePoints.Add(value);
                }
                else
                    canMovePoints.Add(value);
            }
        }
    }

    public override void Killed(GameObject chess)
    {
        base.Killed(chess);
        if (chess == gameObject)
        {
            GameController.gameStatus = GameStatus.End;
            BossKilledEventWithParam(gameObject.tag);
            BossKilledEvent();
        }
    }

    /// <summary>
    /// 增加监听检测被将军事件
    /// </summary>
    /// <param name="chess"></param>
    public override void SubscribeEvents(GameObject chess)
    {
        if (chess == gameObject)
        {
            GameController.UpdateGameDataCompleteEvent += DetectBeAttacked;
            base.SubscribeEvents(chess);
        }
    }

    public override void CancelSubscribeEvents(GameObject chess)
    {
        if (chess == gameObject)
        {
            GameController.UpdateGameDataCompleteEvent -= DetectBeAttacked;
            base.CancelSubscribeEvents(chess);
        }
    }
}

