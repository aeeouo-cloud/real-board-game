// DataManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;

public class DataManager : MonoBehaviour
{

    public static DataManager Instance;

    public Dictionary<string, CardData> CardTable { get; private set; }

    public Dictionary<string, List<CardEffectSequenceData>> EffectSequenceTable { get; private set; }
    public Dictionary<string, List<CardParameterDetailsData>> ParameterDetailTable { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllGameData()
    {
        // 1. CardTable 로드
        CardTable = LoadTable<CardData>("CardData")
            .Where(data => !string.IsNullOrEmpty(data.card_ID))
            .ToDictionary(data => data.card_ID, data => data);

        // 2. EffectSequenceTable 로드
        EffectSequenceTable = LoadTable<CardEffectSequenceData>("CardEffectSequence")
            .Where(data => !string.IsNullOrEmpty(data.EffectGroup_ID))
            .GroupBy(data => data.EffectGroup_ID)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.sequence).ToList());

        // 3. ParameterDetailTable 로드
        ParameterDetailTable = LoadTable<CardParameterDetailsData>("CardParameterDetails")
            .Where(data => !string.IsNullOrEmpty(data.EffectStep_PK))
            .GroupBy(data => data.EffectStep_PK)
            .ToDictionary(g => g.Key, g => g.ToList());

        Debug.Log($"[DataManager] 데이터 로드 완료. 카드: {CardTable.Count}, 효과 그룹: {EffectSequenceTable.Count}");
    }

    private List<T> LoadTable<T>(string fileName) where T : new()
    {
        // CSV 파일 로드
        TextAsset asset = Resources.Load<TextAsset>(fileName);

        if (asset == null)
        {
            Debug.LogError($"CSV 파일 로드 실패: {fileName}. Resources 폴더에 있는지, 이름이 정확한지 확인하세요.");
            return new List<T>();
        }


        string[] lines = asset.text.Split('\n');

        // TrimStart/TrimEnd를 사용하여 모호성 제거 (헤더)
        string[] rawHeaders = lines[0].Split(',');
        string[] headers = new string[rawHeaders.Length];

        for (int j = 0; j < rawHeaders.Length; j++)
        {
            headers[j] = rawHeaders[j].TrimStart().TrimEnd();
        }

        List<T> dataList = new List<T>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] fields = lines[i].Split(',');

            if (fields.Length != headers.Length)
            {
                Debug.LogWarning($"데이터 오류: {fileName} 파일 {i + 1}번째 줄의 필드 개수({fields.Length})가 헤더({headers.Length})와 다릅니다. 이 행은 건너뜁니다.");
                continue;
            }
            T item = new T();
            for (int j = 0; j < headers.Length; j++)
            {
                string fieldName = headers[j] == "class" ? "@class" : headers[j];
                var prop = typeof(T).GetField(fieldName);
                if (prop != null)
                {
                    try
                    {
                        // 🚨 [핵심 수정 부분] string으로 명시적 캐스팅 적용 🚨
                        object value = ConvertValue(((string)fields[j]).TrimStart().TrimEnd(), prop.FieldType);
                        prop.SetValue(item, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"타입 변환 오류: 파일={fileName} / 필드={headers[j]} / 값={fields[j]} / 에러={ex.Message}");
                    }
                }
            }
            dataList.Add(item);
        }
        return dataList;
    }

    private object ConvertValue(string value, System.Type type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
        if (type == typeof(int))
        {
            if (int.TryParse(value, out int result)) return result;
            return 0;
        }
        else if (type == typeof(float))
        {
            value = value.Replace(',', '.');
            if (float.TryParse(value, out float result)) return result;
            return 0f;
        }
        else if (type == typeof(bool))
        {
            if (value.Equals("TRUE", StringComparison.OrdinalIgnoreCase) || value == "1") return true;
            return false;
        }
        // 문자열은 따옴표를 제거하고 공백을 제거합니다.
        return value.Replace("\"", "").Trim();
    }

    // 카드 ID를 통해 코스트를 조회하는 함수
    public bool TryGetCardCost(string cardID, out int cost)
    {
        cost = 0;

        if (CardTable.TryGetValue(cardID, out CardData cardData))
        {
            // CSV에서 읽은 cost 값(string)을 int로 변환합니다.
            if (int.TryParse(cardData.cost, out cost))
            {
                return true;
            }
            else
            {
                Debug.LogError($"[DataManager] 코스트 파싱 오류: ID {cardID}의 코스트 값 '{cardData.cost}'는 유효한 정수가 아닙니다.");
                return false;
            }
        }

        // 카드를 찾지 못한 경우
        return false;
    }
}