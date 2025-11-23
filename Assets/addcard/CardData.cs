// CardData.cs
using System;
using UnityEngine; // Unity에서 사용하기 위해 포함

[Serializable]
public class CardData // 파일명: CardData.cs
{
    // CSV 헤더와 일치하는 필드 (모든 스탯 필드 포함)
    public string card_ID;
    public string type;
    public string @class;
    public string name;
    public string Description;
    public string cost;
    public string EffectGroup_ID;

    // 스탯 필드 (CSV에서 Int로 변환될 예정)
    public int Range;
    public int Damage;
    public int Slow;
    public int Draw;
    public int Move;
    public int Wall_DUR;
    public int Wall_AMT;
    public int Heal;
}