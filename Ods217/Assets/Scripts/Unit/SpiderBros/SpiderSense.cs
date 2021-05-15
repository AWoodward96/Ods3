using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class SpiderSense : MonoBehaviour {

    public SpiderPaths[] Paths;
    public SpiderPatrols[] Patrols;
    public SpiderFood[] Food;
    public SpiderMob Mob;


    const float SPEEDMULTIPLIER = .2f;

    [Space(20)]
    public bool T1 = true;
    public bool T2 = true;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Draw the paths and food
        if(T1)
        {
            DrawT1();
        }
         
        // Draw the patrol web
        if(T2)
        {
            DrawT2();
        }

    }

    public void DrawT1()
    {
        if (Paths == null)
            return;

        if (Paths.Length != 0)
        {
            for (int i = 0; i < Paths.Length; i++)
            {
                SpiderPaths path = Paths[i];
                for (int j = 0; j < Paths[i].Points.Length; j++)
                {
                    if (j == 0)
                        Gizmos.color = Color.magenta;
                    else
                        Gizmos.color = Color.red;

                    if (path.Points.Length > 1 && j > 0)
                        Gizmos.DrawLine(path.Points[j], path.Points[j - 1]);


                    Gizmos.DrawSphere(path.Points[j], .5f);

                }
            }
        }

        for (int i = 0; i < Food.Length; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(Food[i].Position, new Vector3(1, 1, 1));
        }
    }

    public void DrawT1(SpiderPaths _path)
    {
        SpiderPaths path = _path;
        for (int j = 0; j < _path.Points.Length; j++)
        {
            if (j == 0)
                Gizmos.color = Color.magenta;
            else
                Gizmos.color = Color.red;

            if (path.Points.Length > 1 && j > 0)
                Gizmos.DrawLine(path.Points[j], path.Points[j - 1]);


            Gizmos.DrawSphere(path.Points[j], .5f); 
        }


        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(Food[_path.myFood].Position, new Vector3(1, 1, 1));
    }

    public void DrawT2()
    {
        if (Patrols == null)
            return; 

        if (Patrols.Length == 0)
            return;

        Gizmos.color = Color.white;
        for (int pNum = 0; pNum < Patrols.Length; pNum++)
        {
            
            SpiderPatrols patrol = Patrols[pNum];

            if (!patrol.Draw)
                continue;

            if (patrol.Web)
            {
                for (int i = 0; i < patrol.Points.Length; i++)
                {
                    Vector3 point = patrol.Points[i];
                    Gizmos.DrawSphere(patrol.Points[i], .7f);

                    for (int j = 0; j < patrol.Points.Length; j++)
                    {
                        if (patrol.Points[j] == point)
                            continue;

                        Vector3 dst = point - patrol.Points[j];
                        if (dst.magnitude <= patrol.WebMaxDist)
                            Gizmos.DrawLine(point, patrol.Points[j]);
                    }
                }

            }
            else
            {
                // It's just a normal patrol. Lets draw that
                for (int i = 0; i < patrol.Points.Length; i++)
                {
                    if (patrol.Points.Length > 1 && i > 0)
                        Gizmos.DrawLine(patrol.Points[i], patrol.Points[i - 1]);

                    Gizmos.DrawSphere(patrol.Points[i], .7f);
                }


                if (patrol.Loop)
                { 
                    Gizmos.DrawLine(patrol.Points[0], patrol.Points[patrol.Points.Length-1]);
                }
            }


        }
    }

    public void DrawT2(SpiderPatrols _patrol)
    {
        if (Patrols.Length == 0)
            return;

        SpiderPatrols patrol = _patrol;
        if (patrol.Web)
        {
            for (int i = 0; i < patrol.Points.Length; i++)
            {
                Vector3 point = patrol.Points[i];
                Gizmos.DrawSphere(patrol.Points[i], .7f);

                for (int j = 0; j < patrol.Points.Length; j++)
                {
                    if (patrol.Points[j] == point)
                        continue;

                    Vector3 dst = point - patrol.Points[j];
                    if (dst.magnitude <= patrol.WebMaxDist)
                        Gizmos.DrawLine(point, patrol.Points[j]);
                }
            }

        }
        else
        {
            // It's just a normal patrol. Lets draw that
            for (int i = 0; i < patrol.Points.Length; i++)
            {
                if (patrol.Points.Length > 1 && i > 0)
                    Gizmos.DrawLine(patrol.Points[i], patrol.Points[i - 1]);

                Gizmos.DrawSphere(patrol.Points[i], .7f);
            }
             
            if (patrol.Loop)
            {
                Gizmos.DrawLine(patrol.Points[0], patrol.Points[patrol.Points.Length-1]);
            }
        }
    }

    private void FixedUpdate()
    {
        if(Mob.Mob.Count > 0)
        {
            Mob.Speed = Mathf.Min((Mob.Mob.Count * SPEEDMULTIPLIER) + .5f, 3);
        }
    }
}


[System.Serializable]
public class SpiderPaths
{
    public Vector3[] Points;
    public int myFood;
}

[System.Serializable]
public class SpiderPatrols
{
    public bool Draw = true;
    public Vector3[] Points;
    public bool Loop = false;
    public bool Web = false;
    public int WebMaxDist = 5;

}

[System.Serializable]
public class SpiderFood
{ 
    public Vector3 Position;
    public List<GameObject> AtFood = new List<GameObject>();
}

[System.Serializable]
public class SpiderMob
{
    public GameObject Leader;
    public List<IDamageable> Mob = new List<IDamageable>();
    public float Speed;

    public void GetNewLeader()
    {
        this.Leader = null;
        for(int i = 0; i < this.Mob.Count; i ++)
        {
            if(Mob[i].gameObject.activeInHierarchy)
                this.Leader = this.Mob[0].gameObject;
        }
    }
}