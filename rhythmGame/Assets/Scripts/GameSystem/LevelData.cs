using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RhythmGame
{
    [CreateAssetMenu(fileName = "LevelObject", menuName = "ScriptableObjects/LevelObject", order = 1)]
    public class LevelObject : ScriptableObject
    {
        public string levelName;            // 레벨의 이름
        [Range (0, 100)] 
        public int unlockLevel;             // 해금 레벨
        [Range(1, 20)] 
        public int difficulty;              // 음악의 난이도
        public Sprite LevelImage;           // 레벨의 이미지

        public SequenceData noteData;         // 노트 데이터
        public List<ClearReward> clearRewards;  // 클리어에 따른 보상
    }
    [System.Serializable]
    public class ClearReward
    {
        public int clearCount;              // 클리어 횟수
        // 보상은 차후 추가 예정
    }
}

