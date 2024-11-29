using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace RhythmGame
{
    [System.Serializable]
    public class LevelData
    {
        [SerializeField]
        private int highScore;
        [SerializeField]
        private int clearCount;
        [SerializeField]
        private int playCount;
        private LevelObject levelObject;

        public LevelData(LevelObject _levelObject)
        {
            levelObject = _levelObject;
        }

        public int GetValue(string dataName)
        {
            switch (dataName)
            {
                case nameof(highScore):
                    return highScore;
                case nameof(clearCount):
                    return clearCount;
                case nameof(playCount):
                    return playCount;
                default:
                    return -1;
            }
        }

        public LevelObject GetLevelObject()
        {
            return levelObject;
        }
    }

    public class LevelManager : MonoBehaviour
    {
        public List<LevelObject> levelObjects;
        private Dictionary<string, LevelData> levels = new Dictionary<string, LevelData>();
        private List<string> levelKeys = new List<string>();

        public Button leftArrowButton;
        public Button rightArrowButton;
        public Image mainLevelImage;
        public Image leftPreviewImage;
        public Image rightPreviewImage;
        public TextMeshProUGUI levelNameText;
        public TextMeshProUGUI levelInfoText;

        private int currentSelectedIndex = 0;

        public static LevelManager Instance;

        public float transitionDuration = 0.3f;
        public AnimationCurve transitionCurve;

        private bool isTransitioning = false;


        private void Awake()
        {           

            if (levelObjects != null && levelObjects.Count != 0)
            {
                AddLevelDataToDictionary();
            }
        }

        private void Start()
        {
            SetupArrowButtons();
            UpdateLevelDisplay();
        }

        private void SetupArrowButtons()
        {
            if (leftArrowButton != null)
                leftArrowButton.onClick.AddListener(SelectPreviousLevel);
            if (rightArrowButton != null)
                rightArrowButton.onClick.AddListener(SelectNextLevel);
        }

        private void SelectPreviousLevel()
        {
            if (!isTransitioning)
            {
                StartCoroutine(TransitionToLevel(-1));
            }
        }

        private void SelectNextLevel()
        {
            if (!isTransitioning)
            {
                StartCoroutine(TransitionToLevel(1));
            }
        }

        private IEnumerator TransitionToLevel(int direction)
        {
            isTransitioning = true;

            int newIndex = (currentSelectedIndex + direction + levelKeys.Count) % levelKeys.Count;

            // 시작 위치 설정
            Vector2 mainStartPos = mainLevelImage.rectTransform.anchoredPosition;
            Vector2 leftStartPos = leftPreviewImage.rectTransform.anchoredPosition;
            Vector2 rightStartPos = rightPreviewImage.rectTransform.anchoredPosition;

            // 목표 위치 설정
            Vector2 mainEndPos = mainStartPos - new Vector2(direction * Screen.width, 0);
            Vector2 leftEndPos = leftStartPos - new Vector2(direction * Screen.width, 0);
            Vector2 rightEndPos = rightStartPos - new Vector2(direction * Screen.width, 0);

            // 애니메이션
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = transitionCurve.Evaluate(elapsedTime / transitionDuration);

                mainLevelImage.rectTransform.anchoredPosition = Vector2.Lerp(mainStartPos, mainEndPos, t);
                leftPreviewImage.rectTransform.anchoredPosition = Vector2.Lerp(leftStartPos, leftEndPos, t);
                rightPreviewImage.rectTransform.anchoredPosition = Vector2.Lerp(rightStartPos, rightEndPos, t);

                yield return null;
            }

            // 새 레벨로 업데이트
            currentSelectedIndex = newIndex;
            UpdateLevelDisplay();

            // 위치 리셋
            mainLevelImage.rectTransform.anchoredPosition = mainStartPos;
            leftPreviewImage.rectTransform.anchoredPosition = leftStartPos;
            rightPreviewImage.rectTransform.anchoredPosition = rightStartPos;

            isTransitioning = false;
        }

        private void UpdateLevelDisplay()
        {
            if (levelKeys.Count == 0) return;

            // 현재 레벨 정보 업데이트
            UpdateLevelInfo(currentSelectedIndex, mainLevelImage, levelNameText, levelInfoText);

            // 왼쪽 미리보기 업데이트
            int leftIndex = (currentSelectedIndex - 1 + levelKeys.Count) % levelKeys.Count;
            UpdatePreviewImage(leftIndex, leftPreviewImage);

            // 오른쪽 미리보기 업데이트
            int rightIndex = (currentSelectedIndex + 1) % levelKeys.Count;
            UpdatePreviewImage(rightIndex, rightPreviewImage);
            
        }

        private void UpdateLevelInfo(int index, Image image, TextMeshProUGUI nameText, TextMeshProUGUI infoText)
        {
            string levelKey = levelKeys[index];
            LevelData levelData = levels[levelKey];
            LevelObject levelObject = levelData.GetLevelObject();

            if (image != null)
                image.sprite = levelObject.LevelImage;
            if (nameText != null)
                nameText.text = levelObject.levelName;
            if (infoText != null)
                infoText.text = $" High Score: {levelData.GetValue("highScore")}" +
                                $" Clear Count: {levelData.GetValue("clearCount")}" +
                                $" Play Count: {levelData.GetValue("playCount")}";
        }

        private void UpdatePreviewImage(int index, Image image)
        {
            if (image != null)
            {
                string levelKey = levelKeys[index];
                LevelObject levelObject = levels[levelKey].GetLevelObject();
                image.sprite = levelObject.LevelImage;
            }
        }

        public void StartLevel(LevelData levelData)
        {
            Debug.Log("게임 시작! " + levelData.GetLevelObject().levelName);
            GameManager.Instance.SetLevelData(levelData);
            SceneManager.LoadScene("GameScene");
        }

        public LevelData GetLevelDataFromDictionary(string levelKey, string levelName = null, int levelDifficulty = -1)
        {
            if (levelKey == null)
            {
                if (levelName == null || levelDifficulty == -1)
                {
                    return null;
                }

                levelKey = levelName + "_" + levelDifficulty.ToString();
            }

            if (levelKeys.Contains(levelKey))
            {
                return levels[levelKey];
            }

            return null;
        }

        private void Update()
        {
            if (!isTransitioning)
            {
                //if (Input.GetKeyDown(KeyCode.LeftArrow))
                //{
                //    SelectPreviousLevel();
                //}
                //else if (Input.GetKeyDown(KeyCode.RightArrow))
                //{
                //    SelectNextLevel();
                //}
                //else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                //{
                //    StartSelectedLevel();
                //}
            }
        }

        private void AddLevelDataToDictionary()
        {
            foreach (var levelObject in levelObjects)
            {
                string levelKey = levelObject.levelName + "_" + levelObject.difficulty.ToString();

                if (!levels.ContainsKey(levelKey))
                {
                    LevelData levelData = new LevelData(levelObject);
                    levels.Add(levelKey, levelData);
                    levelKeys.Add(levelKey);
                    Debug.Log($"{levelObject.levelName} is Added in Dictionary");
                }
            }
        }

        public void StartSelectedLevel()
        {
            if (levelKeys.Count > 0)
            {
                StartLevel(levels[levelKeys[currentSelectedIndex]]);
            }
        }
    }
}