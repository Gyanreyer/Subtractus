using UnityEngine;
using System.Collections;

public class buttonScroller : MonoBehaviour {

    private Vector2 swipeStartPosition;//Start position of swipe made, used to keep track of direction/magnitude of swipe
    private Vector3 initialButtonsPosition;//Initial position of buttons from when scroll was started

    private Vector3 velocity;//Velocity at which buttons scrolling up/down

    private const float scrollEdgeWidth = 1f;//Width of soft edge on max/min y positions to keep buttons in bounds/bring them back in toward center

    private const float minSwipeDist = 50f;//Minimum distance to detect a swipe

    //Variables used to see how much swipe has moved since last frame to calculate velocity
    private float lastTouchPositionY;
    private float vertSwipeSpeed;

    //Friction to apply to scroll to slow it down when swipe released
    private const float SCROLL_FRICTION = 0.91f;

    private float upperLimitY;//Upper limit for y pos, can't scroll up past here + soft edge width


	// Update is called once per frame
	void Update () {
//Code to make scrolling work with mouse, screw it all I really care about is Android
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPosition = Input.mousePosition;
            initialButtonsPosition = transform.position;

            lastTouchPositionY = swipeStartPosition.y;
        }
        else if (Input.GetMouseButton(0))
        {

            Vector2 swipeVector = new Vector2(Input.mousePosition.x - swipeStartPosition.x, Input.mousePosition.y - swipeStartPosition.y);

            if (Mathf.Abs(swipeVector.y) > minSwipeDist)
            {
                velocity = new Vector3(0, (Input.mousePosition.y - lastTouchPositionY) / (Time.deltaTime * Screen.dpi), 0);

                lastTouchPositionY = Input.mousePosition.y;
            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            velocity = new Vector3(0, (Input.mousePosition.y - lastTouchPositionY) / (Time.deltaTime * Screen.dpi), 0);
        }

        velocity *= SCROLL_FRICTION;

        transform.position += velocity * Time.deltaTime;

        if(transform.position.y < 0)
        {
            if (transform.position.y < -scrollEdgeWidth)
            {
                transform.position = new Vector3(transform.position.x, -scrollEdgeWidth, transform.position.z);
                velocity.y = 0;
            }
            else
            {
                velocity.y = (-transform.position.y) * 5;
            }
            
        }

        if (transform.position.y > upperLimitY)
        {
            if (transform.position.y > upperLimitY + scrollEdgeWidth)
            {
                transform.position = new Vector3(transform.position.x, upperLimitY + scrollEdgeWidth, transform.position.z);
                velocity.y = 0;
            }
            else
            {
                velocity.y = -(transform.position.y - upperLimitY) * 5;
            }
        }


#endif


#if UNITY_ANDROID
        //If player isn't touching the screen, we don't need to do anything so just return early
        if (Input.touchCount > 0)
        {
            //If the player is touching the screen more than once, get the stored touch at index 0
            Touch touch = Input.GetTouch(0);

            //Switch statement based on phase that touch is in, could be started, moved, or ended
            switch (touch.phase)
            {
                //If touch just began, store the position of the touch as the start position of swipe for future calculations
                case TouchPhase.Began:
                    swipeStartPosition = touch.position;
                    initialButtonsPosition = transform.position;

                    lastTouchPositionY = touch.position.y;
                    break;

                //If touch has moved from start point, update swipe vector and partially slide tiles in appropriate direction
                case TouchPhase.Moved:

                    //Vertical distance between y pos of start and end of swipe
                    float vertSwipeDist = Mathf.Abs(touch.position.y - swipeStartPosition.y);

                    //If swipe dist is greater than minimum, register as a swipe
                    if (vertSwipeDist > minSwipeDist)
                    {
                        //Set velocity to reflect how much swipe moved in last frame
                        velocity = new Vector3(0, (touch.position.y - lastTouchPositionY) / (Time.deltaTime * Screen.dpi), 0);

                        lastTouchPositionY = touch.position.y;//Store latest y position
                    }

                    break;
            }
        }

        velocity *= SCROLL_FRICTION;//Apply friction to velocity to slow down scrolling after release

        transform.position += velocity * Time.deltaTime;//Scroll based on velocity

        //Lower y limit is 0
        if (transform.position.y < 0)
        {
            //If scrolled past scroll edge width, stop on that edge completely
            if (transform.position.y < -scrollEdgeWidth)
            {
                transform.position = new Vector3(transform.position.x, -scrollEdgeWidth, transform.position.z);
                velocity.y = 0;
            }
            else//If inside of soft edge, change velocity to gently move back up out of soft edge zone
            {
                velocity.y = (-transform.position.y) * 5;
            }
        }
        //Upper y limit
        else if (transform.position.y > upperLimitY)
        {
            //If past edge width, stop on edge
            if (transform.position.y > upperLimitY + scrollEdgeWidth)
            {
                transform.position = new Vector3(transform.position.x, upperLimitY + scrollEdgeWidth, transform.position.z);
                velocity.y = 0;
            }
            else//If inside soft edge, move back down out of soft edge zone
            {
                velocity.y = -(transform.position.y - upperLimitY) * 5;
            }
        }
#endif
    }

    //Do initial setup to make scrolling work
    public void setupNewChapter()
    {
        //Get furthest level button down to calculate limit for max y pos
        RectTransform lastChild = transform.GetChild(transform.childCount - 1).GetComponent<RectTransform>();

        //Get upper limit based on height and position of last button from bottom coords of screen
        upperLimitY = -Camera.main.orthographicSize - (lastChild.position.y - (lastChild.rect.height));
    }


}
