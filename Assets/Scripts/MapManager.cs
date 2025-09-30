using UnityEngine;


public class MapManager : MonoBehaviour     //육각형으로 이루어진 원형 타일 맵의 그리드를 계산해 왼쪽 위부터 오른쪽 아래까지 순서대로 배열 positions[] 에 위치를 저장하는 코드임. vector3로 평면임
{
    public float width;   //한칸마다 크기 (높이와 폭이 같아야함) 그렇지 않아도 또 프로그래머 개고생 하면 해결되긴 함. + 개고생 하게 됨. 이건 폭임.(최대) x가 폭임(좌우)
    public float height; //결국 한 축 더 계산해야할듯. 이건 높이임. (최대) z임
    public int length;  //맵의 가로 최대 길이(칸수, 정수)1  

    static public Vector3[] position;

    void Start()
    {
        int arrsize = ((3 * (length * length)) + 1) / 4;
        int minlength = (length + 1) / 2;

        Vector3[] positions = new Vector3[arrsize];

        positions[0] = this.GetComponent<Transform>().localPosition;
        positions[0].x -= ((length - minlength) / 2f) * width;
        positions[0].z += (length - minlength) * (height * 0.75f);  // 이 세줄이 맨 왼쪽 위칸 위치 구하는 코드임.

        int index = 0;
        Vector3 indexposition = positions[0];

        int linelength = minlength;
        int lineth = 0;

        for (; linelength <= length; linelength++)  //위에서 중간까지 위치설정. 중간도 설정함.
        {
            indexposition = positions[0];
            indexposition.x -= (lineth / 2f) * width;
            indexposition.z -= lineth * (height * 0.75f);

            for (int i = 0; i < linelength; i++)
            {
                positions[index] = indexposition;
                indexposition.x += width;
                index++;
            }
            lineth++;
        }
        //빠져나오면 라인렝스 8 7개 칸을 연산한 뒤이므로 8이 맞음 라인쓰는 높이 계산에 아직 써야됨.
        linelength -= 2; //하고나면 6
        int reverselineth = lineth;
        reverselineth -= 2;

        for (; linelength >= minlength; linelength--) // 중간부터 아래 끝까지 위치설정. 중간은 포함 안함.
        {
            indexposition = positions[0];
            indexposition.x -= (reverselineth / 2f) * width;
            indexposition.z -= lineth * (height * 0.75f);

            for (int i = 0; i < linelength; i++)
            {
                positions[index] = indexposition;
                indexposition.x += width;
                index++;
            }
            lineth++;
            reverselineth--;
        }
        position = positions as Vector3[]; //주소 복사됨. position 수정시 positions도 수정됨.

        Debug.Log(position.Length);
    }
}
