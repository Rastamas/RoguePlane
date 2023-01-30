using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lane
{
    public LaneType laneType;
    public float xCoordinate;
    public List<GameObject> enemies;

    public Lane(float x, LaneType laneType = LaneType.Generic)
    {
        xCoordinate = x;
        this.laneType = laneType;
        enemies = new List<GameObject>();
    }

    public bool isOccupied => enemies.Where(e => e != null).Any();
}

public enum LaneType
{
    Generic,
    Assasin,
    Tilter
}
