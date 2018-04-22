using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The primary camera script
/// Uses orthographic view and follows a target at a constant distance
/// </summary>
public class CamScript : MonoBehaviour
{

    CamScript instance;
    public Vector3 FollowBack;
    public Transform Target;
    public Vector3 ExtentsTL; // The restrictions of where the camera can move and where the edges are
    public Vector3 ExtentsBR;
    public static Vector3 CursorLocation;

    public Transform Vizualiser;

    PlayerScript player;

    bool Loaded;
    float loadedPoint = 0;

    public enum CamEffect { None, Shake, FadeIn, FadeOut }
    public CamEffect curEffect = CamEffect.None;

	SpriteRenderer fade;
	float timer = 0.0f;
	float effectLength = 0.0f;

    List<GameObject> Watching;

    // Use this for initialization
    void Awake()
    {
        instance = this;
        FollowBack = GlobalConstants.DEFAULTFOLLOWBACK;
        SceneManager.sceneLoaded += LevelLoad;
        Watching = new List<GameObject>();

		fade = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Target == null)
            Target = new GameObject().transform;

        // Load in the cursors new world position
        CursorToWorld();

        // If we have something to look at
        if (Target != null)
        {
            // Take its position and add a standard distance from it
            Vector3 Additive = Target.position + FollowBack;

            if(playerRef != null)
            {
                // If we're not in cutscene and the menu is not open, do not account for the cursors position
                if (playerRef.AcceptInput && !UpgradesManager.MenuOpen)
                {
                    // Otherwise, shift the cameras position towards where the cursor is
                    Vector3 toCursor = CursorLocation - transform.position;
                    toCursor = GlobalConstants.ZeroYComponent(toCursor);
                    if (toCursor.magnitude > 6)
                        toCursor = toCursor.normalized * 6;

                    Additive += toCursor;
                }
                else
                {
                    // W/o the cursor, the focus gets a bit weird, add a z vector to fix
                    Additive += (Vector3.forward * 6);
                }
            }
            else
            {
                // W/o the cursor, the focus gets a bit weird, add a z vector to fix
                Additive += (Vector3.forward * 6);
            }



            // Handle the list of important objects we need to watch
            for(int i = 0; i < Watching.Count; i ++)
            {
                Additive += (Watching[i].transform.position - Target.transform.position) / (Watching.Count + 2);
            }

            Additive += HandleEffects(Additive);

            Additive.y = Target.position.y + FollowBack.y;

            // If we just loaded a new scene then snap onto the location that the player is at
            if (Loaded)
                transform.position = Vector3.Lerp(transform.position, Additive, 6f * Time.deltaTime);
            else
                transform.position = Additive;

            HandleExtents();
        }else
        { 
            transform.position = Vector3.Lerp(transform.position, Target.position + FollowBack, 6f * Time.deltaTime);
        }

        // once a level is loaded, jump to where the camera is supposed to be instead of panning
        if (loadedPoint < 1)
        {
            loadedPoint += Time.deltaTime;
            if (loadedPoint > 1)
            {
                Loaded = true;
            }
        }

        if(Vizualiser != null)
        {
            Vizualiser.transform.position = CursorLocation;
        }
    }

    void LevelLoad(Scene level, LoadSceneMode _mode)
    {
        Loaded = false;
        loadedPoint = 0;
    }

    // Extents are the points where the camera is restricted from moving further
    void HandleExtents()
    {
        // Restrict the camera from moving outside the designated area
        Camera main = Camera.main;
        float size = main.orthographicSize;
        float verticleSize = size * 2;
        float horezontalSize = size * Screen.width / Screen.height;

        // Get the position on the ground that is right in the center of the screen
        Plane horezPlane = new Plane(Vector3.up, ExtentsTL);
        Ray CameraProjectionRay = main.ScreenPointToRay(new Vector2(0, Screen.height / 2));
        float dist;
        horezPlane.Raycast(CameraProjectionRay, out dist);
        Vector3 posOnGround = CameraProjectionRay.GetPoint(dist);
     
        // Get the bounds
        verticleSize = verticleSize / (2 * Mathf.Cos(Mathf.Deg2Rad * 30));
        float tiltDistance = (Mathf.Abs(posOnGround.z - transform.position.z));
        float topBounds = verticleSize + tiltDistance;
        float bottomBounds = tiltDistance - verticleSize;

        // Restrict the camera
        if (transform.position.x < (ExtentsTL.x + horezontalSize))
            transform.position = new Vector3(ExtentsTL.x + horezontalSize, transform.position.y, transform.position.z);

        if (transform.position.x > (ExtentsBR.x - horezontalSize))
            transform.position = new Vector3(ExtentsBR.x - horezontalSize, transform.position.y, transform.position.z);

        if (transform.position.z > (ExtentsTL.z - topBounds))
            transform.position = new Vector3(transform.position.x, transform.position.y, (ExtentsTL.z - topBounds));

        if (transform.position.z < (ExtentsBR.z - bottomBounds))
            transform.position = new Vector3(transform.position.x, transform.position.y, (ExtentsBR.z - bottomBounds));
    }

    Vector3 HandleEffects(Vector3 Addative)
    {
        switch(curEffect)
        { 
            case CamEffect.Shake:
			{
                return UnityEngine.Random.onUnitSphere * Random.Range(1,2);
				break;
			}

			case CamEffect.FadeIn:
			{
				if(fade != null)
				{
					Color myColor = fade.color;

					timer += Time.deltaTime;
					myColor.a = 1.0f - (timer / effectLength);

					if(timer >= effectLength)
					{
						myColor.a = 0.0f;
					}

					fade.color = myColor;
				}
				break;
			}

			case CamEffect.FadeOut:
			{
				if(fade != null)
				{
					Color myColor = fade.color;

					timer += Time.deltaTime;
					myColor.a = (timer / effectLength);

					if(timer >= effectLength)
					{
						myColor.a = 1.0f;
					}

					fade.color = myColor;
				}
				break;
			}

            default:
                return Vector3.zero;
        }

		return Vector3.zero;
    }

    public virtual PlayerScript playerRef
    {
        get
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

            return player;
        }
    }

    public void AddWatch(GameObject _obj)
    {
        if (!Watching.Contains(_obj))
            Watching.Add(_obj);
    }

    public void RemoveWatch(GameObject _obj)
    {
        if (Watching.Contains(_obj))
            Watching.Remove(_obj); 
    }

    public void AddEffect(CamEffect fx, float Time)
    {
        curEffect = fx;
        StopCoroutine("fxTimer");
        StartCoroutine(fxTimer(Time));
    }

    public void AddEffect(CamEffect fx)
    {
        curEffect = fx;
        StopAllCoroutines();
    }

    IEnumerator fxTimer(float Time)
    {
		timer = 0.0f;
		effectLength = Time;

        yield return new WaitForSeconds(Time);
        curEffect = CamEffect.None;
 
    }
   

    void CursorToWorld()
    {
        // Find in world space where the cursor is pointing
        Plane p = new Plane(Vector3.up, Target.transform.position);
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Ray ray = Camera.main.ScreenPointToRay(mousePosition); // This takes the mouse position, and starts a ray at the world position (based on where the mouse is on screen) shooting where the camera is pointer
        float dist;
        if (p.Raycast(ray, out dist))
        { 
            CursorLocation = ray.GetPoint(dist);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = (ExtentsTL - ExtentsBR) / 2;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(center.x + Mathf.Abs(center.x - ExtentsBR.x), 2, center.x + Mathf.Abs(center.z - ExtentsBR.z)));
    }

    public void LerpSize(float _newSize, float _speed)
    {
        StopCoroutine("lerpSizeCRT");
        StartCoroutine(lerpSizeCRT(_newSize, _speed));
    }

    IEnumerator lerpSizeCRT(float _newSize, float _speed)
    {
        float newVal= Camera.main.orthographicSize;
        while(Mathf.Abs( _newSize - newVal) > .1f)
        {
            newVal = Mathf.Lerp(newVal, _newSize, _speed * Time.deltaTime);
            Camera.main.orthographicSize = newVal;
            yield return null;
        }

        Camera.main.orthographicSize = _newSize;
    }
}
