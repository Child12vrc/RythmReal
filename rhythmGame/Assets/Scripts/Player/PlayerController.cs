using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //점수 콤보 타격 - 필요행필요행!!
    public NoteManager noteManager;

    public NoteObject noteObject;

    public List<Note> notes = new List<Note>();         //모든 노트 정보를 담는 리스트

    public GameObject player;

    public Transform playertransform;

    public int playerHp;                         //HP네여
    public int playerAttack;                     //공격이에여
    public float attackCoolTime;                 //어택이 바로바로 되면 안돼여...
    public int jumpPosition;                    //점프 할 힘!!!!!!!!!!!
    public float jumpCoolTime;                  //점프 쿨타임

    public float maxHigh;                       //점프 시 최대 높이

    public float startTime;                      //시작 시간 잴게여

    public Rigidbody playerRigidbody;            //점프를 위한 리지드바디를 가져와볼게요

    public bool isDead;         //죽었나여?
    public bool isStart;        //시작했나여?
    public bool isEnd;          //게임이 끝났나여?
    public bool isJumping;      //뛰고 있나여?
    
    void Start()
    {
        startTime = 3;                  //시작 시간 전 3초구여
        playerHp = 150;                 //플레이어에게 Hp를 150으로 지정해줘요
        playerAttack = 20;             //플레이어의 공격력을 주는 이유는 나중에 연타 하는 아이가 생길 수 있기 때문이에여
        attackCoolTime = 0f;            //플레이어가 연타하면 이기겠죠~? 그럼 안돼요! 그렇기 때문에 시간을 줄거에여
        jumpCoolTime = 0f;            //플레이어가 계속 점프하면 재미가 없겠죠~? 깔깔! 그렇게 때문에 쿨타임을 줍니다요~~
        jumpPosition = 6;                 //6 주니까 딱 맞네여!!
        
        //이 친구들은 일단 다 비활성화 해줍니다요
        isDead = false;
        isEnd = false;
        isJumping = false;
        isStart = false;
    }

    void Update()
    {
        startTime -= Time.deltaTime;    //빼줘여
        if (startTime <= 0)             //0이거나 0보다 아래면
        {
            startTime = 0;              //0으로 만들어줘여
            isStart = true;             //게임 스타트 불 값을 트루로 바꿔주구여
        }   

        if(isStart == true)
        {
            GameStart();
         
            if (playerHp <= 0)
            {
                //트위닝 합시당 혹은 애니메이션 넣을게요~
                Dead();                     //죽어라 얍~
            }

            if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.F))
            {
                if(jumpCoolTime <= 0)
                {
                    Jump();
                    jumpCoolTime = 0.3f;                //점프 쿨타임 줍니다요오
                }
            }
            jumpCoolTime -= Time.deltaTime;             //점프 쿨타임 없애 없애~~
        }
    }

    void GameStart()                        //만든 함수들을 다 넣을거에요
    {
        //함수 총집합!!
    }
    
    void Jump()
    {
        player.transform.position = new Vector3(-7, jumpPosition, 0);       //점프 점프
        playerRigidbody.velocity = Vector3.zero;                            //점프 중력 차단차단
    }

    void Dead()
    {
        isEnd = true;                                        //게임이 끝났구요
        isDead = true;                                      //죽었어요
        SceneManager.LoadScene("DeadScene");                //죽었을 때 씬 전환
        playerRigidbody.velocity = Vector3.zero;            //끝났으니까요.
    }

    public void OnTriggerEnter(Collider other)
    {
        //저히가 생각한건 그냥 트리거로 충돌 판정하기에요 !>!
        //어차피 날라오는건 노트밖에 없으니까 꺄핫~!
    }
}
