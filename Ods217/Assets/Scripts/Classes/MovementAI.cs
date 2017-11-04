using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MovementAI {

    public bool ActionComplete;
    public GameObject myObject;
    public CController myCC; 

    public enum MovementAIType { Wander, SmartWander, Chase, SmartChase, Wait };
    public MovementAIType curType;
    public MovementAIType lastType;

    // Generic variables
    float timer;
    float desiredTime; 
    Vector3 direction;




    // Movement Specific Variables 
    float wanderSpeed;

    public MovementAI(GameObject _obj, CController _CC)
    {
        myObject = _obj;
        myCC = _CC;
        ActionComplete = true;
    } 

    public void Update()
    {
        if(!ActionComplete)
        {
            switch (curType)
            {
                case MovementAIType.Wait:
                    doWait();
                    break;
                case MovementAIType.Wander:
                    doWander();
                    break;
                case MovementAIType.SmartWander:
                    doWander(); // Doesn't matter if it's smart, the smart part is handled in the initial method
                    break;
            }
        }
        
    }


  

    void doWait()
    {
        timer += Time.deltaTime;

        if(timer >= desiredTime)
        {
            ActionComplete = true;
            lastType = MovementAIType.Wait;
        }
    }

    public void Wait(float _time)
    {
        if(ActionComplete)
        { 
            timer = 0;
            desiredTime = _time;
            curType = MovementAIType.Wait;
            ActionComplete = false;
        }
    }

    void doWander()
    {
        timer += Time.deltaTime;

        myCC.ApplyForce(direction * wanderSpeed);

        // Move in that random direction for a bit
        if (timer >= desiredTime)
        {
            ActionComplete = true;
            lastType = curType;
        }
    }

    public void Wander(float _time, float _speed)
    {
        if (ActionComplete)
        {
            wanderSpeed = _speed; 
            desiredTime = _time;
            curType = MovementAIType.Wander;  
            ActionComplete = false; 
            timer = 0;

            // Pick a direction
            Vector2 randV2Direction = Random.insideUnitCircle;
            direction = new Vector3(randV2Direction.x, 0, randV2Direction.y); // Make that v2 direction a v3 direction, while zeroing out that y component

        }
    }

    public void SmartWander(float _time, float _speed, Transform desiredLocation)
    {
        if (ActionComplete)
        {
            wanderSpeed = _speed;
            desiredTime = _time;
            curType = MovementAIType.SmartWander;
            ActionComplete = false;
            timer = 0;

            // Pick a direction
            // Smart wander will make the unit move towards the player, though it's still random by about 90 degrees
          
            Vector2 randV2Direction = Random.insideUnitCircle;
            direction = new Vector3(randV2Direction.x, 0, randV2Direction.y); // Make that v2 direction a v3 direction, while zeroing out that y component 

            Vector3 to = myObject.transform.position + direction;
            Vector3 from = desiredLocation.position - myObject.transform.position;

            for (int breakOut = 0; breakOut < 20; breakOut++)
            {

                randV2Direction = Random.insideUnitCircle;
                direction = new Vector3(randV2Direction.x, 0, randV2Direction.y); // Make that v2 direction a v3 direction, while zeroing out that y component 
                breakOut++;
                float angle = Mathf.Abs(Vector3.Angle(GlobalConstants.ZeroYComponent(to), GlobalConstants.ZeroYComponent(from)));
                Debug.Log(angle);

                if(angle < 45)
                {
                    breakOut = 20;
                }
            } 
        }
    }
}
