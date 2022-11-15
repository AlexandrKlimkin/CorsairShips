using UnityEngine;
using System.Collections;

public class grgCameraRotate : MonoBehaviour
{

    [System.Serializable]
    public class Constrain2
    {
        public float min = 0;
        public float max = 0;
    }

    public float floatingSpeed = 1f;
    public float mouseSensX = 15f;
    public float mouseSensY = 15f;
    public Constrain2 verticalConstr;
    public float decalFOV = 40f;
    public float visualPartFov = 40f;
    public Transform car;
    public Camera uiCamera;
    public bool isSetupCamera = false;
    public Light cameraLight;
    private int moveDir = 1;
    private bool isFloatAround = true;
    private Transform curFocus;
    private Vector3 lastPos = Vector3.zero;
    private Quaternion lastRot = new Quaternion(0f, 0f, 0f, 0f);
    private bool isGetLastPos = false;
    private bool isCanMoveByMouse = true;
    private float origFOV;
    private float defaultFloatingSpeed;
    private Vector3 lastMousePos;
    private float FOVdestination;

    private bool isPhotoMode = false;

    //DEBUG
    private float rotateSum;
    private float deltaTimeSum;
    private int callsCount;

    public enum FOV_MODE
    {
        DECAL,
        VISUAL_PART
    }

    void Awake()
    {
#if UNITY_IPHONE || UNITY_ANDROID
	    //GetComponent<Vignetting>().enabled = false;
	    //GetComponent<BloomAndLensFlares>().enabled = false;
#endif
    }

    // Use this for initialization
    void Start()
    {
        origFOV = GetComponent<Camera>().fieldOfView;
        FOVdestination = origFOV;
        transform.LookAt(car.position);
        lastMousePos = Input.mousePosition;
        defaultFloatingSpeed = floatingSpeed;
    }

    public void SetFocus(Transform focus)
    {
        curFocus = focus;
        isFloatAround = false;
    }

    public void SetPhotoMode(bool isOn)
    {
        isPhotoMode = isOn;
        floatingSpeed = (isOn) ? 0f : defaultFloatingSpeed;

        if (!isOn)
        {
            SetFloat(true); //Only when exiting
        }
    }

    public void SetFloat(bool isFloating)
    {
        isFloatAround = isFloating;
        if (isFloating)
        {
            FOVdestination = origFOV;
        }
    }

    public void SetFovDestination(float val)
    {
        FOVdestination = val;
    }

    public void SetFOVMode(FOV_MODE mode)
    {
        switch (mode)
        {
            case (FOV_MODE.DECAL):
                FOVdestination = decalFOV;
                break;
            case (FOV_MODE.VISUAL_PART):
                FOVdestination = visualPartFov;
                break;
        }
    }

    public void ToggleLight(bool isOn)
    {
        cameraLight.enabled = isOn;
    }

    // Update is called once per frame
    void Update()
    {

        if (isFloatAround && !isSetupCamera)
        {
            if (isGetLastPos && !isPhotoMode)
            {
                isGetLastPos = !MoveToPosRot(lastPos, lastRot, 0.01f);
                return;
            }

            //         if (Input.GetKeyDown (KeyCode.Z)) {
            //            RacingLog.Log ("NumCalls: " + callsCount + "; GetAxisX sum: " + rotateSum + "; TimeSum: " + deltaTimeSum + "; AverageDeltaTime: " + deltaTimeSum / callsCount + "; AverageGetAxis: " + rotateSum / callsCount + "; Sens: " + mouseSensX + "; Screen: " + Screen.width + " x " + Screen.height);
            //            rotateSum = 0f;
            //            deltaTimeSum = 0f;
            //            callsCount = 0;
            //         }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                isCanMoveByMouse = IsClickOnScreen(uiCamera);
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                lastMousePos = Input.mousePosition;
            }

            if (Input.GetKey(KeyCode.Mouse0) && isCanMoveByMouse)
            {
                float inputDirX = (Input.mousePosition.x - lastMousePos.x); /// Screen.width * 1000f;

                if (inputDirX != 0)
                {
                    moveDir = (inputDirX < 0) ? -1 : 1;
                }
                float inputDirY = -(Input.mousePosition.y - lastMousePos.y);// / Screen.height * 1000f; //-Input.GetAxis("Mouse Y");

                float newY = Mathf.Lerp(transform.position.y, Mathf.Clamp(transform.position.y + inputDirY, verticalConstr.min, verticalConstr.max), mouseSensY * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                transform.LookAt(car.position);
                float rotX = Time.deltaTime * inputDirX * mouseSensX;
                transform.RotateAround(car.position, Vector3.up, rotX);
                rotateSum += rotX;
                deltaTimeSum += Time.deltaTime;
                callsCount += 1;
            }
            else
            {
                transform.RotateAround(car.position, Vector3.up, moveDir * floatingSpeed * Time.deltaTime);
            }
            ChangeFov(false);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && IsClickOnScreen(uiCamera))
            {
                SetFloat(true);
            }
            if (!isGetLastPos)
            {
                lastPos = transform.position;
                lastRot = transform.rotation;
                isGetLastPos = true;
            }
            if (curFocus != null)
            {
                MoveToPosRot(curFocus.position, curFocus.rotation, 0.01f);
            }
            //camera.fieldOfView = Mathf.Lerp (camera.fieldOfView, decalFOV, Time.deltaTime * 2f);
        }
        lastMousePos = Input.mousePosition;
    }

    private bool MoveToPosRot(Vector3 pos, Quaternion rot, float treshhold)
    {
        transform.position = Vector3.Slerp(transform.position, pos, Time.deltaTime * 2f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 2f);

        return ChangeFov((pos - transform.position).magnitude <= treshhold && GetComponent<Camera>().fieldOfView >= FOVdestination - 1f);
    }

    private bool ChangeFov(bool isImmediatelyReachDst)
    {
        GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, FOVdestination, Time.deltaTime * 2f);

        //RacingLog.Log ("Current FOV: " + camera.fieldOfView + "; origFov: " + origFOV + "; deltaTimex2: " + Time.deltaTime * 2);
        if (isImmediatelyReachDst)
        {
            GetComponent<Camera>().fieldOfView = FOVdestination;
            return true;
        }

        return false;
    }

    private bool IsClickOnScreen(Camera cam)
    {
        return true; //TODO: add checking, where we have clicked
    }
}