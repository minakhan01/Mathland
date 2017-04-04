using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using UnityEngine.SceneManagement;
using Academy.HoloToolkit.Unity;


public class BallManager : Singleton<BallManager>
{
    // KeywordRecognizer object.
    KeywordRecognizer keywordRecognizer;

    public bool ballStopped;

    public GameObject ball;

    public GameObject canvas;
    // public GameObject cube;

    // public GameObject SpatialMapping;

    Vector3 oldVector;

    // Defines which function to call when a keyword is recognized.
    delegate void KeywordAction(PhraseRecognizedEventArgs args);
    Dictionary<string, KeywordAction> keywordCollection;
    // Use this for initialization
    void Start()
    {
        oldVector = new Vector3(0, 0, 0);
        ballStopped = false;
        ball.SetActive(false);
        // cube.SetActive(false);

        keywordCollection = new Dictionary<string, KeywordAction>();

        // Add keyword to start manipulation.
        keywordCollection.Add("Stop Ball", StopBallCommand);

        // 5.a: Add keyword Expand Model to call the ExpandModelCommand function.
        keywordCollection.Add("Play Ball", PlayBallCommand);

        // 5.a: Add keyword Expand Model to call the ExpandModelCommand function.
        keywordCollection.Add("Reset Game", ResetGame);

        // 5.a: Add keyword Expand Model to call the ExpandModelCommand function.
        keywordCollection.Add("Apply Force", ApplyForce);

        keywordCollection.Add("Place Ball", PlaceBall);
        keywordCollection.Add("Bounce Ball", BounceBall);

        keywordCollection.Add("Show Graph", ShowGraph);

        keywordCollection.Add("Hide Graph", HideGraph);

        keywordCollection.Add("Go Right", GoRight);

        keywordCollection.Add("Go Up", GoRight);

        keywordCollection.Add("Show Mesh", ShowMesh);

        keywordCollection.Add("Hide Mesh", HideMesh);

        // Initialize KeywordRecognizer with the previously added keywords.
        keywordRecognizer = new KeywordRecognizer(keywordCollection.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
        GoRight();
    }

    void OnDestroy()
    {
        keywordRecognizer.Dispose();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        KeywordAction keywordAction;

        if (keywordCollection.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke(args);
        }
    }

    private void ShowMesh(PhraseRecognizedEventArgs args)
    {
        SpatialMappingManager.Instance.DrawVisualMeshes = true;
    }

    private void HideMesh(PhraseRecognizedEventArgs args)
    {
        SpatialMappingManager.Instance.DrawVisualMeshes = false;
    }

    private void StopBallCommand(PhraseRecognizedEventArgs args)
    {
        Debug.Log("stop called");
        StopBall();
    }

    private void ShowGraph(PhraseRecognizedEventArgs args)
    {
        Debug.Log("show canvas called");
        canvas.SetActive(true);
    }

    private void HideGraph(PhraseRecognizedEventArgs args)
    {
        Debug.Log("hide canvas called");
        canvas.SetActive(false);
    }

    public void StopBall()
    {
        // oldVector = ball.GetComponent<Rigidbody>().velocity;
        //ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;
        ballStopped = true;
        Time.timeScale = 0;
    }

    private void PlayBallCommand(PhraseRecognizedEventArgs args)
    {
        Debug.Log("play called");
        PlayBall();
    }

    public void PlayBall()
    {
       // ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        Time.timeScale = 1;
        ballStopped = false;
    }

    private void ResetGame(PhraseRecognizedEventArgs args)
    {
        Debug.Log("reset game");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void PlaceBall(PhraseRecognizedEventArgs args)
    {
        Debug.Log("place ball");
        ball.SetActive(true);
        ball.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2 + Camera.main.transform.up; // Start to drop it in front of the camera
        
        // cube.SetActive(true);
    }

    void Update()
    {

    }

    void ApplyForce(PhraseRecognizedEventArgs args)
    {
        //ball.GetComponent<Rigidbody>().AddForce(transform.forward * 5);
        if (Time.timeScale == 0) {
            Time.timeScale = 1;
        }
        ball.GetComponent<Rigidbody>().AddForce(ball.transform.forward * 4, ForceMode.Impulse);
    }

    void randomForce() {
        ball.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 4, ForceMode.Impulse);
    }

    void GoUp(PhraseRecognizedEventArgs args)
    {
        GoUp();
    }

    public void GoUp()
    {
        ball.GetComponent<Rigidbody>().AddForce(Camera.main.transform.up * 5, ForceMode.Impulse);
    }

    void GoRight(PhraseRecognizedEventArgs args)
    {
        GoRight();
    }

    public void GoRight()
    {
        ball.GetComponent<Rigidbody>().AddForce(Camera.main.transform.right * 2, ForceMode.Impulse);
    }

    void GoForward(PhraseRecognizedEventArgs args)
    {
        GoForward();
    }

    public void GoForward()
    {
        ball.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 4, ForceMode.Impulse);
    }

    void BounceBall(PhraseRecognizedEventArgs args)
    {
        Debug.Log("bounce ball");
        ball.GetComponent<Rigidbody>().AddForce(Camera.main.transform.up * 10);
    }
}