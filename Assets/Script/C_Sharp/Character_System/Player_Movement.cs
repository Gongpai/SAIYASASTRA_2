using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Image = UnityEngine.UI.Image;

public class Player_Movement : FuntionLibraly
{
    [Header("Setting")]
    [SerializeField] private bool Is_Spawn_In_Room = false;

    [SerializeField] private float WalkSpeed = 1;

    [SerializeField] private float RunSpeed = 3;

    [SerializeField] private float JumpForce = 1;

    [SerializeField] private Vector3 BoxSize;

    [SerializeField] private float MaxDistance;

    [SerializeField] private LayerMask layerMask;

    [SerializeField] private MovementModes movementMode = MovementModes.FreeRoam;

    [SerializeField] private GameObject Playerinput;

    [SerializeField] public GameObject HeadPoint;

    [SerializeField] public GameObject EquipPoint;

    [SerializeField] public ShowMessage showMessage;

    [SerializeField] public TextMeshProUGUI TextDebug;

    [SerializeField] public GameObject Essential_Menu;

    [SerializeField] public GameObject PauseGame_Ui;

    [SerializeField] public GameObject Death_Ui;

    [SerializeField] public GameObject Touch_screen_UI;

    [SerializeField] public GameObject Ghost_Effect;

    [SerializeField] public GameObject Switch_Scene;

    [SerializeField] private Image HP_ProgressBar;

    [SerializeField] private AudioSource SoundBG;

    [Header("NotSet")]
    float moveSpeed = 0;
    public PlayerInput playerInput;
    private InputAction JumpAction, RunAction, InventoryAction, NoteAction, CraftAction, aimAction, useitemAction, shootAction, PauseMenuAction, Turn_On_LightAction;
    private InputManager inputManager;

    private Object_interact Ob_interact = Object_interact.Cupboard_Hide;

    private Vector3 velocity, Pos;

    public float HP = 100;

    public GameObject ObjectHide;
    private GameObject Object_Intaeract;
    private Rigidbody rb;
    private bool Is_Run = false;

    private List<bool> Can_Press_Use_item = new List<bool>();

    private enum MovementModes
    {
        MoveHorizontally,
        MoveVertically,
        FreeRoam
    };

    // Start is called before the first frame update
    void Start()
    {
        SoundBG.Play();

        if (inputManager == null)
        {
            inputManager = InputManager.instance;
        }

        if (inputManager == null)
        {
            Debug.LogWarning("No player");
        }

        moveSpeed = WalkSpeed;
        GameInstance.Player = gameObject;
        UpdateHPWidget();
        inputManager.OnUseTouch = Touch_Use_Button;
        inputManager.OnShootTouch = Touch_Shoot_Button;

        rb = GetComponent<Rigidbody>();

        GetComponent<Inventory_System>().Set_Item_Element();
        GetComponent<Inventory_System>().Reset_Select_Index();
    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        Character_movement();
        Ui_Control();
        EquipPoint_Turn();
        OnCharacterDeath();
        UpdateHPWidget();
    }

    void FixedUpdate()
    {
        Set_Platform();
    }

    void Awake()
    {
        Pos = transform.position;
        playerInput = Playerinput.GetComponent<PlayerInput>();
        JumpAction = playerInput.actions["Jump"];
        RunAction = playerInput.actions["Run"];
        InventoryAction = playerInput.actions["Inventory"];
        NoteAction = playerInput.actions["Note"];
        CraftAction = playerInput.actions["Craft"];
        aimAction = playerInput.actions["Aim"];
        useitemAction = playerInput.actions["Use_Item"];
        shootAction = playerInput.actions["Shoot"];
        PauseMenuAction = playerInput.actions["PauseMenu"];
        Turn_On_LightAction = playerInput.actions["Turn_On_Light"];
        Game_State_Manager.Instance.OnGameStateChange += OnGamestateChanged;
        Set_Platform();
    }

    void OnDestroy()
    {
        Game_State_Manager.Instance.OnGameStateChange -= OnGamestateChanged;
    }

    void OnEnable()
    {
        if (PauseGame_Ui.active || Death_Ui.active)
        {
            SoundBG.Play();
            StopSoundGhost(false);
        }
        Set_Platform();
        JumpAction.Enable();
        RunAction.Enable();
        InventoryAction.Enable();
        NoteAction.Enable();
        CraftAction.Enable();
        aimAction.Enable();
        useitemAction.Enable();
        shootAction.Enable();
        PauseMenuAction.Enable();
        Turn_On_LightAction.Enable();
        OnGamePause(false);
    }

    void OnDisable()
    {
        JumpAction.Disable();
        RunAction.Disable();
        InventoryAction.Disable();
        NoteAction.Disable();
        CraftAction.Disable();
        aimAction.Disable();
        useitemAction.Disable();
        shootAction.Disable();
        PauseMenuAction.Disable();
        Turn_On_LightAction.Disable();
        OnGamePause(true);
        Touch_screen_UI.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        PauseMenuAction.Disable();
    }

    public void Set_Platform()
    {
        switch (SlateModeDetect.currentMode)
        {
            case ConvertibleMode.SlateTabletMode:
                Touch_screen_UI.SetActive(true);
                //Debug.LogWarning("SlateTabletMode");
                break;
            case ConvertibleMode.LaptopDockedMode:
                //Debug.LogWarning("LaptopDockedMode");
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsPlayer:
                        //print("Windowssssssssssssssssss");
                        Touch_screen_UI.SetActive(false);
                        break;
                    case RuntimePlatform.WindowsEditor:
                        //print("Windowssssssssssssssssss");
                        Touch_screen_UI.SetActive(true);
                        break;
                    case RuntimePlatform.Android:
                        //print("Androiddddddddddddddddddddd");
                        Touch_screen_UI.SetActive(true);
                        break;
                    case RuntimePlatform.WebGLPlayer:
                        Touch_screen_UI.SetActive(false);
                        break;
                }
                break;
        }
    }

    private void OnGamePause(bool isPause)
    {
        if (isPause)
        {
            GamePause_Component(gameObject, true);
            Item_Attack_System.OnPauseGame?.Invoke();
        }
        else
        {
            GamePause_Component(gameObject, false);
            Item_Attack_System.UnPauseGame?.Invoke();
        }
    }

    private void OnGamestateChanged(GameState gameState)
    {
        enabled = gameState == GameState.Play;
    }

    public void OnSwitchScene()
    {
        Switch_Scene.SetActive(true);
    }

    void OnCharacterDeath()
    {
        if(HP <= 0)
        {
            GameObject.FindGameObjectWithTag("Camera_Setting").GetComponent<ZoomSmoothCameraSystem>().IsZoomCamera = true;
            GameObject[] AllGhost = GameObject.FindGameObjectsWithTag("Ghost");
            for (int i = 0; i < AllGhost.Length - 1;)
            {
                if (AllGhost[i].GetComponent<Ai_Movement>().IsSeeCharacter)
                {
                    AllGhost[i].GetComponent<Ai_Movement>().PlaySound(false);
                }
                i++;
            }
            GameInstance.Ghost.GetComponent<Ai_Movement>().PlaySound(false);
            SoundBG.Pause();
            StopSoundGhost(true);
            Death_Ui.SetActive(true);
            Death_Ui.GetComponent<Animator>().SetBool("Is_Death", true);
        }
    }
    void EquipPoint_Turn()
    {
        if (GetComponent<SpriteRenderer>().flipX)
        {
            EquipPoint.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            EquipPoint.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
    }

    public void Set_Block_Use_item(bool Is_Block)
    {
        if (Is_Block)
        {
            Can_Press_Use_item.Add(true);
        }
        else
        {
            if (Can_Press_Use_item.Count > 0)
                Can_Press_Use_item.RemoveAt(0);
        }
    }

    void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case "Character_Hide":
                Ob_interact = Object_interact.Cupboard_Hide;
                Object_Intaeract = other.gameObject;
                break;
            case "Door_Lawson":
                Ob_interact = Object_interact.Lawson_Door;
                Object_Intaeract = other.gameObject;
                break;
            case "PickUpItem":
                Ob_interact = Object_interact.PickUp_Item;
                Object_Intaeract = other.gameObject;
                break;
            case "PickUpNote":
                Ob_interact = Object_interact.PickUp_Note;
                Object_Intaeract = other.gameObject;
                break;
            case "Untagged":
                if(other.GetComponent<Show_Puzzle>() != null)
                {
                    Ob_interact = Object_interact.Puzzle;
                    Object_Intaeract = other.gameObject;
                }
                if(other.GetComponent<Open_Room_New_Scene>() != null)
                {
                    Ob_interact = Object_interact.Room_Door;
                    Object_Intaeract = other.gameObject;
                }
                break;
            default:
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case "Character_Hide":
                Ob_interact = Object_interact.Cupboard_Hide;
                Object_Intaeract = null;
                break;
            case "Door_Lawson":
                Ob_interact = Object_interact.Lawson_Door;
                Object_Intaeract = null;
                break;
            case "PickUpItem":
                Ob_interact = Object_interact.PickUp_Item;
                Object_Intaeract = null;
                break;
            case "PickUpNote":
                Ob_interact = Object_interact.PickUp_Note;
                Object_Intaeract = null;
                break;
            case "Untagged":
                if (other.GetComponent<Show_Puzzle>() != null)
                {
                    Ob_interact = Object_interact.Puzzle;
                    Object_Intaeract = other.gameObject;
                }
                if (other.GetComponent<Open_Room_New_Scene>() != null)
                {
                    Ob_interact = Object_interact.Room_Door;
                    Object_Intaeract = other.gameObject;
                }
                break;
            default:
                break;
        }
    }

    public void HP_System(Collider other, float hp)
    {
        float Damage = other.GetComponent<Variables>().declarations.Get<float>("Damage");

        if (HP > 0)
        {
            HP += hp * Damage;

            if (HP <= 0)
                HP = 0;
            if (HP >= 100)
                HP = 100;
        }
        print("Hp------- : " + HP);
        other.GetComponent<Item_Script>().Destroy_Item();
    }

    void UpdateHPWidget()
    {
        FuntionLibraly.ProgressBar_Fill(HP_ProgressBar, HP, 100);
    }

    [System.Obsolete]
    void Character_movement()
    {
        //����ǹ�ͧ�������͹������Ф�
        if (movementMode == MovementModes.FreeRoam)
        {
            Vector3 movementVector = new Vector3(inputManager.horizontalMoveAxis, 0, inputManager.verticalMoveAxis);
            rb.transform.position = rb.transform.position + (movementVector * Time.deltaTime * moveSpeed);
        }
        if (Touch_screen_UI.active)
        {
            Joystick floatingJoystick = Touch_screen_UI.transform.GetChild(0).GetComponent<Joystick>();
            Vector3 movementVector = new Vector3(floatingJoystick.Horizontal, 0, floatingJoystick.Vertical);
            rb.transform.position = rb.transform.position + (movementVector * Time.deltaTime * moveSpeed);
            if (floatingJoystick.Horizontal < 0)
                this.gameObject.GetComponent<SpriteRenderer>().flipX = true;
            else if (floatingJoystick.Horizontal > 0)
                this.gameObject.GetComponent<SpriteRenderer>().flipX = false;
        }

        //�鴡��ⴴ
        if (GroundCheck())
        {
            this.GetComponent<Animator>().SetBool("IsJump", false);
            //print("StopJump");
        }
        else
        {
            this.GetComponent<Animator>().SetBool("IsJump", true);
        }

        if (JumpAction.WasPressedThisFrame() && GroundCheck())
        {
            rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);
            //print("Jump");
        }

        

        //�����
        if (RunAction.IsPressed() == true)
        {
            moveSpeed = RunSpeed;
        }
        else if (RunAction.WasReleasedThisFrame())
        {
            moveSpeed = WalkSpeed;
        }

        //�������ǵ���Ф�
        velocity = (transform.position - Pos) / Time.deltaTime;
        this.GetComponent<Animator>().SetFloat("Speed", velocity.magnitude);
        //Debug.Log("Speed is : " + velocity.magnitude);
        Pos = transform.position;

        //����Ф��ѹ���¢��
        if (inputManager.horizontalMoveAxis < 0)
            this.gameObject.GetComponent<SpriteRenderer>().flipX = true;
        else if (inputManager.horizontalMoveAxis > 0)
            this.gameObject.GetComponent<SpriteRenderer>().flipX = false;

        //����Ф��Թ���˹��ŧ��ѧ
        if (inputManager.verticalMoveAxis < 0)
            this.GetComponent<Animator>().SetBool("IsWalkForward", false);
        else if (inputManager.verticalMoveAxis > 0)
            this.GetComponent<Animator>().SetBool("IsWalkForward", true);
    }


    //���������� - �����
    public void Touch_Run_Button(bool isPress)
    {
        if (isPress)
        {
            print("Runnnnnnnnnnnnnnnnnnnnnnnn");
            moveSpeed = RunSpeed;
            Is_Run = true;
        }
        else
        {
            print("Walkkkkkkkkkkkkkkkkkkkkkkk");
            moveSpeed = WalkSpeed;
            Is_Run = false;
        }
    }

    //���������� - �����ⴴ
    public void Touch_Jump_Button()
    {
        if (GroundCheck())
        {
            rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);
            print("Jump");
        }
    }

    //���������� - ���ԧ
    [System.Obsolete]
    public void Touch_Use_Button()
    {
        gameObject.GetComponent<Inventory_System>().Shoot_Item();
        gameObject.GetComponent<Inventory_System>().Use_Item_Equip();
    }
    
    public void Touch_Shoot_Button(float angle, float duration)
    {
        print("Shoot!!!!!!!");
        gameObject.GetComponent<Inventory_System>().Aim(true);
        gameObject.GetComponent<Inventory_System>().Shoot_Item(angle);
    }

    //���������� - ���ͺʹͧ�ͧ���㹩ҡ
    public void Touch_Interact_Button()
    {
        if (Object_Intaeract != null)
        {
            switch (Ob_interact)
            {
                case Object_interact.Cupboard_Hide:
                    Object_Intaeract.GetComponent<Cupboard_Hide>().TriggerOpenDoor();
                    break;
                case Object_interact.Lawson_Door:
                    Object_Intaeract.transform.parent.GetComponent<Door_Lawson_System>().OpenOrClose();
                    break;
                case Object_interact.PickUp_Item:
                    Object_Intaeract.GetComponent<Pick_up_Item_System>().PickUp_Item();
                    break;
                case Object_interact.PickUp_Note:
                    Object_Intaeract.GetComponent<PickUp_Note_System>().PickUp_Note();
                    break;
                case Object_interact.Puzzle:
                    Object_Intaeract.GetComponent<Show_Puzzle>().OpenPuzzle_Ui();
                    break;
                case Object_interact.Room_Door:
                    Object_Intaeract.GetComponent<Open_Room_New_Scene>().Enter_Door();
                    break;
                default:
                    break;
            }
        }
    }

    //���������� - ���Դ��ͧ�红ͧ
    public void Touch_OpenInventoryUi()
    {
        if (GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect != null)
            GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect.SetActive(false);

        gameObject.GetComponent<Inventory_System>().Set_Inventory_Element();
        gameObject.GetComponent<Inventory_System>().Set_Item_Element();
        Essential_Menu.SetActive(true);
        Essential_Menu.GetComponent<Navigate_Menu>().OpenPage(0);
        Game_State_Manager.Instance.Setstate(GameState.Pause);
    }

    [System.Obsolete]
    private bool Check_PauseMenu()
    {
        bool check = !PauseGame_Ui.active && !Death_Ui.active;
        return check;
    }

    //���������� - ����ش��
    [System.Obsolete]
    public void Touch_OpenPauseMenu()
    {
        if (Check_PauseMenu())
        {
            if (GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect != null)
                GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect.SetActive(false);

            StopSoundGhost(true);
            SoundBG.Pause();
            PauseGame_Ui.SetActive(true);
            Game_State_Manager.Instance.Setstate(GameState.Pause);
        }
    }
        /**
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position - transform.up * MaxDistance, BoxSize);
        }
        **/

    private void StopSoundGhost(bool IsPause)
    {
        AudioSource audioSourceGhost = GetAllSoundGhost();

        if (audioSourceGhost != null)
        {
            if (IsPause)
            {
                audioSourceGhost.volume = 0;
            }
            else
            {
                audioSourceGhost.volume = 1;
            }
        }
        else
        {
            Debug.LogWarning("Not Found Sound Ghost");
        }
    }

    private AudioSource GetAllSoundGhost()
    {
        GameObject[] ghostSound = GameObject.FindGameObjectsWithTag("Ghost");
        AudioSource audioSource = null;

        foreach (GameObject ghost in ghostSound)
        {
            
            if (ghost.GetComponent<AudioSource>().isPlaying)
            {
                if (ghost.GetComponent<Ai_Movement>().IsSeeCharacter || ghost.GetComponent<Variables>().declarations.Get<AiGhost>("Ai_Ghost") == AiGhost.Home_ghost)
                {
                    audioSource = ghost.GetComponent<AudioSource>();
                }
                else
                {
                    Debug.LogWarning("Ghost Not See");
                }

                Debug.LogWarning("Found Sound Ghost Is Playing");
                break;
            }
        }

        return audioSource;
    }

    private bool GroundCheck()
    {
        if (Physics.BoxCast(transform.position, BoxSize, -transform.up, transform.rotation, MaxDistance, layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [System.Obsolete]
    void Ui_Control()
    {
        //�Դ˹�� PauseMenu
        if(PauseMenuAction.WasPerformedThisFrame())
        {
            
            if (!PauseGame_Ui.active && !Death_Ui.active)
            {
                if (GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect != null)
                    GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect.SetActive(false);

                StopSoundGhost(true);
                SoundBG.Pause();
                PauseGame_Ui.SetActive(true);
                Game_State_Manager.Instance.Setstate(GameState.Pause);
            }
        }

        //�Դ˹�Ҫ�ͧ�红ͧ
        if (InventoryAction.WasPressedThisFrame() == true)
        {
            if (GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect != null)
                GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect.SetActive(false);

            gameObject.GetComponent<Inventory_System>().Set_Inventory_Element();
            gameObject.GetComponent<Inventory_System>().Set_Item_Element();
            Essential_Menu.SetActive(true);
            Essential_Menu.GetComponent<Navigate_Menu>().OpenPage(0);
            GetComponent<Inventory_System>().PlayAnim(true);
            Game_State_Manager.Instance.Setstate(GameState.Pause);
        }

        //�Դ˹�Һѹ�֡
        if (NoteAction.WasPressedThisFrame() == true)
        {
            if (GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect != null)
                GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect.SetActive(false);

            gameObject.GetComponent<Note_System>().Set_Note_Element();
            Essential_Menu.SetActive(true);
            Essential_Menu.GetComponent<Navigate_Menu>().OpenPage(2);
            GetComponent<Note_System>().PlayAnim(true);
            Game_State_Manager.Instance.Setstate(GameState.Pause);
        }

        //�Դ˹�� craft
        if (CraftAction.WasPressedThisFrame() == true)
        {
            if (GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect != null)
                GameInstance.Player.GetComponent<Player_Movement>().Ghost_Effect.SetActive(false);

            gameObject.GetComponent<Craft_System>().Set_Craft_Inventory_Element();
            Essential_Menu.SetActive(true);
            Essential_Menu.GetComponent<Navigate_Menu>().OpenPage(1);
            GetComponent<Craft_System>().PlayAnim(true);
            Game_State_Manager.Instance.Setstate(GameState.Pause);
        }

        //��������ԧ
        if (aimAction.WasPressedThisFrame() && Application.platform != RuntimePlatform.Android)
        {
            gameObject.GetComponent<Inventory_System>().Aim(!gameObject.GetComponent<Inventory_System>().IsAim);
        }

        //��ҹ����
        if (useitemAction.WasPressedThisFrame() == true)
        {
            if (Can_Press_Use_item.Count <= 0)
            {
                gameObject.GetComponent<Inventory_System>().Use_Item_Equip();
            }
            
        }

        //�����ԧ/��
        if (shootAction.WasPressedThisFrame() && Application.platform != RuntimePlatform.Android)
        {
            if (!Touch_screen_UI.active)
            {
                gameObject.GetComponent<Inventory_System>().Shoot_Item();
            }
        }

        //�����Դ俩��
        if (Turn_On_LightAction.WasPressedThisFrame()  && Application.platform != RuntimePlatform.Android)
        {
            gameObject.GetComponent<Inventory_System>().Use_Item_Equip();
        }

        //�Դ��е���������
        if (useitemAction.WasPressedThisFrame() == true)
        {
            if (Object_Intaeract != null)
            {
                switch (Ob_interact)
                {
                    case Object_interact.Cupboard_Hide:
                        Object_Intaeract.GetComponent<Cupboard_Hide>().TriggerOpenDoor();
                        break;
                    case Object_interact.Lawson_Door:
                        Object_Intaeract.transform.parent.GetComponent<Door_Lawson_System>().OpenOrClose();
                        Object_Intaeract.transform.parent.GetComponent<Door_Lawson_System>().Cant_Open_Door();
                        break;
                    case Object_interact.PickUp_Item:
                        Object_Intaeract.GetComponent<Pick_up_Item_System>().PickUp_Item();
                        break;
                    case Object_interact.PickUp_Note:
                        Object_Intaeract.GetComponent<PickUp_Note_System>().PickUp_Note();
                        break;
                    case Object_interact.Puzzle:
                        Object_Intaeract.GetComponent<Show_Puzzle>().OpenPuzzle_Ui();
                        break;
                    case Object_interact.Room_Door:
                        Object_Intaeract.GetComponent<Open_Room_New_Scene>().Enter_Door();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
