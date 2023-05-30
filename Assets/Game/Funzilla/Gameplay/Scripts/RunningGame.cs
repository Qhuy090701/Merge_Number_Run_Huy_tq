using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunningGame : MonoBehaviour
{
    [SerializeField] private float speedTouch = 5f;
    [SerializeField] private float swipeThreshold = 20f;
    [SerializeField] private float canvasWidth = 1080;
    [SerializeField] private float canvasHeight = 1920;
    [SerializeField] private GameObject numberStart;
    private PlayerControllerState currentState;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private BoxCollider parentCollider;
    public List<Transform> childObjects;
    public List<Vector3> numberPositions;
    private List<Vector3> initialNumberPositions;
    private bool isSwipingAndHolding = false;
    public bool isFinish = false;

    private enum PlayerControllerState
    {
        StartGame,
        RunGame,
        EndGame
    }

    private void Start()
    {
        numberPositions = new List<Vector3>();
        childObjects = new List<Transform>();
        initialNumberPositions = new List<Vector3>();

        InstantiateNumberStart();
        parentCollider = gameObject.AddComponent<BoxCollider>();
        parentCollider.isTrigger = true;
        UpdateParentCollider();

        currentState = PlayerControllerState.StartGame;

        Canvas canvas = FindObjectOfType<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasWidth = canvasRect.rect.width;
        canvasHeight = canvasRect.rect.height;
    }

    private void Update()
    {
        UpdateParentCollider();

        switch (currentState)
        {
            case PlayerControllerState.StartGame:
                if (Input.GetMouseButtonDown(0))
                {
                    currentState = PlayerControllerState.RunGame;
                }
                break;
            case PlayerControllerState.RunGame:
                TouchMove();
                Move();
                if (isFinish)
                {
                    currentState = PlayerControllerState.EndGame;
                    return;
                }
                break;

            case PlayerControllerState.EndGame:
                Debug.Log("End Game Run");
                break;
        }
    }

    private void TouchMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            isSwipingAndHolding = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSwipingAndHolding = false;
        }

        if (isSwipingAndHolding)
        {
            endPosition = Input.mousePosition;
            float distance = Vector3.Distance(startPosition, endPosition);

            if (distance > swipeThreshold)
            {
                Vector3 direction = endPosition - startPosition;

                if (direction.x > 0)
                {
                    transform.position += Vector3.right * speedTouch * Time.deltaTime;
                }
                else
                {
                    transform.position += Vector3.left * speedTouch * Time.deltaTime;
                }
            }
        }

        Vector3 currentPosition = transform.position;
        float minX = -canvasWidth / 2f;
        float maxX = canvasWidth / 2f;
        currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);
        transform.position = currentPosition;
    }

    private void Move()
    {
        transform.position += Vector3.forward * speedTouch * Time.deltaTime;
    }

    private void InstantiateNumberStart()
    {
        GameObject newNumberStart = Instantiate(numberStart, transform.position, transform.rotation);
        newNumberStart.transform.SetParent(transform);
    }

    public void UpdateParentCollider()
    {
        if (childObjects.Count != transform.childCount)
        {
            childObjects.Clear();
            numberPositions.Clear();
            initialNumberPositions.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childObject = transform.GetChild(i);
                childObjects.Add(childObject);

                Renderer childRenderer = childObject.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    Bounds childBounds = childRenderer.bounds;
                    numberPositions.Add(transform.position + new Vector3(childBounds.center.x - transform.position.x, 0f, 0f));
                    initialNumberPositions.Add(childObject.position);
                }
            }

            parentCollider.center = Vector3.zero;
            parentCollider.size = Vector3.zero;
        }

        SortNumberObjects();
    }

    private void SortNumberObjects()
    {
        childObjects = childObjects.OrderBy(x => x.position.x).ToList();

        for (int i = 0; i < childObjects.Count; i++)
        {
            Transform childObject = childObjects[i];
            Vector3 newPosition = (i == 0 && childObject.GetComponent<Rigidbody>() != null) ? initialNumberPositions[i] : transform.position + new Vector3(i + 0.5f, 0f, 0f);
            childObject.position = newPosition;
        }
    }

    private void OnDestroy()
    {
        foreach (Transform childObject in childObjects)
        {
            if (childObject == null)
            {
                UpdateParentCollider();
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Number"))
        {
            other.transform.SetParent(transform);
            UpdateParentCollider();
        }
    }
}
