using UnityEngine;

public class Dice : MonoBehaviour   //dice throw, calculate logic
{
    public Rigidbody rb;
    public float force = 500;
    public TurnManager turnmanager;
    bool threw = false;
    bool canclick = true;
    Vector3 randomDir;
    Vector3 randomTorque;

    bool hasrun = true;
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();

        randomDir = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        randomTorque = new Vector3(
            Random.Range(-10f, 10f),
            Random.Range(-10f, 10f),
            Random.Range(-10f, 10f)
        );
        
        rb.AddForce(Vector3.up, ForceMode.Impulse);
    }

    void OnMouseDown()
    {
        if (canclick == true)   //prevent reclick 
        {
            rb.AddForce(randomDir * force, ForceMode.Impulse);
            rb.AddTorque(randomTorque, ForceMode.Impulse);
            canclick = false;
            threw = true;
        }
    }

    int CalculateDice()
    {
        
    Vector3 up = transform.up;
    Vector3 forward = transform.forward;
    Vector3 right = transform.right;

    float dotUp = Mathf.Abs(Vector3.Dot(Vector3.up, up));
    float dotForward = Mathf.Abs(Vector3.Dot(Vector3.up, forward));
    float dotRight = Mathf.Abs(Vector3.Dot(Vector3.up, right));

    if (dotUp >= dotForward && dotUp >= dotRight) return 1;
    if (dotRight >= dotUp && dotRight >= dotForward) return 2;
    return 3;
    
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.IsSleeping() && threw && hasrun)
        {
            hasrun = false;
            Debug.Log("dice stoped dice = " + CalculateDice());
            turnmanager.GetDiceResult(CalculateDice());
            Destroy(gameObject,2f);
        }
    }
}
