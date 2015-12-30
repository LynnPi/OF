using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PalmPoineer.Mobile;

/// <summary>
/// 布阵总控制器
/// </summary>
public class DeploySceneManager : MonoBehaviour {
    public static DeploySceneManager Instance { get; private set; }

    public Action<Vector3> GridClickCallback = delegate { };
    public DeployFormationRecorder FormationRecorder;
    public DeployPlayerGridDrawer PlayerGridDrawer;
    public DeployEnemyGridDrawer EnemyGridDrawer;
    public DeployValidAreaGuider ValidDeployAreaGuider;

    private GFGridRenderCamera GridRenderCtrl_;
    private Vector3 ILLEGAL_POSITION = new Vector3( 10000f, 10000f, 10000f );
    private DeployBoundaryDisplayer EnemyBoundaryDisplayer_;
    private DeployBoundaryDisplayer PlayerBoundaryDisplayer_;

    private void Awake() {
        Instance = this;
       
        InitFormationRecorder();
        InitScene();
        StartCoroutine(GenerateEnemyFormation());
        StartCoroutine(DrawGrid());      
        StartCoroutine(StartAreaGuide());
        StartCoroutine(DrawBoundary());
    }

    private IEnumerator DrawGrid() {
        yield return new WaitForSeconds(UserStatusPanel.DEPLOY_ENTER_SCENE_DURATION);

        GameObject playerDrawerGo = new GameObject("__PlayerGridDrawer__");
        PlayerGridDrawer = playerDrawerGo.AddComponent<DeployPlayerGridDrawer>();

        GameObject enemyDrawerGo = new GameObject("__EnemyGridDrawer__");
        EnemyGridDrawer = enemyDrawerGo.AddComponent<DeployEnemyGridDrawer>();

        StartCoroutine(CheckForPlayerInput());
    }


    private IEnumerator DrawBoundary() {
        yield return new WaitForSeconds(UserStatusPanel.DEPLOY_ENTER_SCENE_DURATION);

        Vector3[] linePoints;
        Material lineMaterial;
        linePoints = DeployBoundaryDisplayer.CalculateLinePointList();
        lineMaterial = Resources.Load(DeployBoundaryDisplayer.MAT_BOUNDARY_DEPLOY_ENEMY) as Material;
        EnemyBoundaryDisplayer_ = DeployBoundaryDisplayer.CreateInstance(transform, linePoints, lineMaterial);
        EnemyBoundaryDisplayer_.gameObject.name = "__EnemyBoundaryDisplayer__";
    }

    private void InitFormationRecorder() {
        GameObject go = new GameObject("__DeployFormationRecorder__");
        go.transform.SetParent(transform);
        FormationRecorder = go.AddComponent<DeployFormationRecorder>();
    }

    private IEnumerator StartAreaGuide() {
        yield return new WaitForSeconds(UserStatusPanel.DEPLOY_ENTER_SCENE_DURATION);
        ValidDeployAreaGuider = DeployValidAreaGuider.CreateInstance();
        ValidDeployAreaGuider.ShowGuideAnimation(false);
        ValidDeployAreaGuider.SetSize(2*GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).battlefield_wid, 2*GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).deployarea_len);
    }
    private void InitScene() {
        CameraManager.Instance.InitDepolyCamera();
        CameraManager.Instance.StartDeployCameraThread();

        GridRenderCtrl_ = CameraManager.Instance.MainCamera.AddComponent<GFGridRenderCamera>();
       
        SnappingUnits.IntersectMaterial = Resources.Load("deploy/intersection") as Material;
        SnappingUnits.CanPlacedMaterial = Resources.Load("deploy/can_placed") as Material;
    }

    private void OnDestroy() {
        Destroy( GridRenderCtrl_ );
        GridRenderCtrl_ = null;
        DeployBoundaryDisplayer.EnableVectorCanvas(false);
        DeployUnitMenu.DestroyCurrent();
    }

    private IEnumerator CheckForPlayerInput() {
        while( true ) {
            Vector3 pos;
            if (CheckClickOnGrid(out pos) && !CheckClickOnDeployedUnit()) {
                //Debug.Log("click on grid");  
                GridClickCallback( pos );
            }
            yield return null;
        }
    }

    private bool CheckClickOnGrid(out Vector3 pos){
        if( Input.GetMouseButtonUp(0) ) {
            RaycastHit hit;
            PlayerGridDrawer.GridCollider.Raycast( Camera.main.ScreenPointToRay( Input.mousePosition ), out hit, Mathf.Infinity );

            if( hit.collider ) {
                pos = hit.point;
                return true;           
            }
            else{
                pos = ILLEGAL_POSITION;
                return false;
            }
        }
        else {
            pos = ILLEGAL_POSITION;
            return false;
        } 
    }

    private bool CheckClickOnDeployedUnit() {
        bool isOnDeployedUnit = false;
        if (Input.GetMouseButtonUp(0)) {
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);

            if (hit.collider.tag == "ShipModel") {
                isOnDeployedUnit = true;
            }
        }
        return isOnDeployedUnit;
    }


    private IEnumerator GenerateEnemyFormation() {
        GameObject root = new GameObject("__EnemyRoot__");
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;
        root.transform.localScale = Vector3.one;

        int shipCount = BattleSys.GetShipCount(false);
        for (int i = 0; i < shipCount; i++) {
            ClientShip cs = BattleSys.GetShipByIndex(false, i);
            Ship ship = AssetLoader.GetShipDefine(cs.Reference.model_res);
            if (ship == null) {
                Debug.LogError(string.Format("can't create ship, id : {0}, modelRes : {1}",
                    cs.Reference.id,
                    cs.Reference.model_res));
                yield break;
            }

            //成组上阵，组中舰船的数量
            int stackNum = cs.Reference.stack ? cs.Reference.stack_num : 1;

            GameObject group = new GameObject(string.Format("id_{0}", cs.Reference.id));
            group.transform.SetParent(root.transform);
            group.transform.position = cs.Position;

            //保证缓存中有资源
            yield return StartCoroutine(AssetLoader.PreloadShipModel(ship));

            for (int j = 0; j < stackNum; j++) {
                GameObject go = AssetLoader.GetInstantiatedGameObject(ship.ModelPath);
                AssetLoader.ReplaceShader(go);
                go.transform.SetParent(group.transform);
                go.transform.localPosition = cs.FormationList == null ? Vector3.zero : cs.FormationList[j];
                go.transform.eulerAngles = new Vector3(0f, 180f, 0f);//朝向与攻击方相反
            }
            //yield return new WaitForSeconds(0.2f);//支持间隔上阵
        }
    }
}
